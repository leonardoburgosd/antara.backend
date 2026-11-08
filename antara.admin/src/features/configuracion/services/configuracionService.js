import { apiClient, parseErrorMessage, ApiError } from '../../../services/api/apiClient'
import { ENDPOINTS } from '../../../services/api/endpoints'

const leer = async (response, fallback) => {
  if (!response.ok) {
    const mensaje = await parseErrorMessage(response, fallback)
    throw new ApiError(mensaje, response.status)
  }
  try {
    return await response.json()
  } catch {
    return null
  }
}

export const configuracionService = {
  async getPerfil() {
    return leer(await apiClient.get(ENDPOINTS.USUARIOS.PERFIL), 'No se pudo cargar tu perfil.')
  },

  async actualizarPerfil(datos) {
    return leer(
      await apiClient.put(ENDPOINTS.USUARIOS.PERFIL, datos),
      'No se pudo actualizar tu perfil.',
    )
  },

  async cambiarPassword(passwordActual, passwordNueva) {
    return leer(
      await apiClient.post(ENDPOINTS.USUARIOS.PERFIL_PASSWORD, { passwordActual, passwordNueva }),
      'No se pudo cambiar la contraseña.',
    )
  },
}

export default configuracionService
