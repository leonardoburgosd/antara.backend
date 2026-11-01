import { apiConfig } from './apiConfig'
import { storage } from '../storage/localStorage'

export class ApiError extends Error {
  constructor(message, status, data = null) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.data = data
  }
}

export const parseErrorMessage = async (response, fallback = 'No se pudo completar la operación.') => {
  try {
    const data = await response.json()
    if (data?.error) return data.error
    if (data?.errors) {
      const primero = Object.values(data.errors).flat()[0]
      if (primero) return primero
    }
    if (data?.title) return data.title
  } catch {
    // Response was not JSON
  }
  return fallback
}

export const request = async (path, options = {}) => {
  const { timeout = apiConfig.timeout, headers: customHeaders, body, ...restOptions } = options
  const url = path.startsWith('http') ? path : `${apiConfig.baseURL}${path}`

  const headers = new Headers(customHeaders || {})
  const token = storage.getToken()

  if (token && !headers.has('Authorization')) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  let finalBody = body
  if (body && !(body instanceof FormData) && typeof body === 'object') {
    if (!headers.has('Content-Type')) {
      headers.set('Content-Type', 'application/json')
    }
    finalBody = JSON.stringify(body)
  }

  const controller = new AbortController()
  const timeoutId = setTimeout(() => controller.abort(), timeout)

  try {
    const response = await fetch(url, {
      ...restOptions,
      headers,
      body: finalBody,
      signal: options.signal || controller.signal,
    })

    if (response.status === 401) {
      storage.clearAuth()
      if (typeof window !== 'undefined' && !window.location.pathname.startsWith('/login')) {
        window.location.replace('/login?sesion=expirada')
      }
    }

    return response
  } catch (error) {
    if (error.name === 'AbortError') {
      throw new ApiError('La solicitud ha tardado demasiado tiempo.', 408)
    }
    throw new ApiError('Error de conexión con el servidor.', 0)
  } finally {
    clearTimeout(timeoutId)
  }
}

export const apiClient = {
  async get(path, options = {}) {
    const response = await request(path, { ...options, method: 'GET' })
    return response
  },

  async post(path, body, options = {}) {
    const response = await request(path, { ...options, method: 'POST', body })
    return response
  },

  async put(path, body, options = {}) {
    const response = await request(path, { ...options, method: 'PUT', body })
    return response
  },

  async delete(path, options = {}) {
    const response = await request(path, { ...options, method: 'DELETE' })
    return response
  },
}

