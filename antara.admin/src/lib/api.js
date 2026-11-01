import { storage } from '../services/storage/localStorage'
import { request, parseErrorMessage } from '../services/api/apiClient'

export const getToken = () => storage.getToken()

export const getStoredUser = () => storage.getUser()

export const hasSession = () => storage.hasSession()

export const saveSession = (usuario) => storage.saveSession(usuario)

export const clearSession = () => storage.clearAuth()

export const apiFetch = async (path, options = {}) => {
  return request(path, options)
}

export const leerError = async (response, fallback = 'No se pudo completar la operación.') => {
  return parseErrorMessage(response, fallback)
}
