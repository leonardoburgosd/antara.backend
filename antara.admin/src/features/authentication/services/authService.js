import { apiClient, parseErrorMessage, ApiError } from '../../../services/api/apiClient'
import { ENDPOINTS } from '../../../services/api/endpoints'
import { storage } from '../../../services/storage/localStorage'

export const authService = {
  async checkHealth() {
    const response = await apiClient.get(ENDPOINTS.AUTH.HEALTH)
    if (!response.ok) {
      throw new ApiError('Error al verificar el estado del sistema.', response.status)
    }
    return response.json()
  },

  async login(email, password) {
    const response = await apiClient.post(ENDPOINTS.AUTH.LOGIN, {
      email: email?.trim(),
      password,
    })

    if (response.status === 503) {
      throw new ApiError('El sistema aún no tiene un administrador registrado.', 503)
    }

    if (response.status === 404) {
      throw new ApiError('Correo o contraseña incorrectos.', 404)
    }

    if (!response.ok) {
      const msg = await parseErrorMessage(response, 'No se pudo iniciar sesión. Verifica los datos.')
      throw new ApiError(msg, response.status)
    }

    const user = await response.json()

    if (user?.tipo && !['admin', 'administrador'].includes(String(user.tipo).toLowerCase())) {
      throw new ApiError('Esta cuenta no tiene permisos de administrador.', 403)
    }

    if (!storage.saveSession(user)) {
      throw new ApiError('El servidor no devolvió un token de autenticación.', 500)
    }

    return user
  },

  async createFirstAdmin(data) {
    const payload = {
      email: data.email?.trim(),
      password: data.password,
      nombre: data.nombre?.trim(),
      fechaNacimiento: data.fechaNacimiento
        ? new Date(data.fechaNacimiento).toISOString()
        : new Date().toISOString(),
      genero: data.genero || 'M',
      pais: data.pais?.trim(),
      tipo: data.tipo || 'admin',
    }

    const response = await apiClient.post(ENDPOINTS.AUTH.CREATE_FIRST_ADMIN, payload)

    if (!response.ok) {
      const msg = await parseErrorMessage(response, 'Error al crear administrador.')
      throw new ApiError(msg, response.status)
    }

    return response.json()
  },

  logout() {
    storage.clearAuth()
  },

  getCurrentUser() {
    return storage.getUser()
  },

  hasSession() {
    return storage.hasSession()
  },
}

