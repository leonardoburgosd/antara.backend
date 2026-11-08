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

export const usuariosService = {
  async getUsuarios() {
    const data = await leer(
      await apiClient.get(ENDPOINTS.USUARIOS.LIST),
      'No se pudieron cargar los usuarios.',
    )
    return Array.isArray(data) ? data : []
  },

  async crearUsuario(datos) {
    return leer(
      await apiClient.post(ENDPOINTS.USUARIOS.CREATE, datos),
      'No se pudo crear el usuario.',
    )
  },

  async actualizarUsuario(id, datos) {
    return leer(
      await apiClient.put(ENDPOINTS.USUARIOS.UPDATE(id), datos),
      'No se pudo actualizar el usuario.',
    )
  },

  async eliminarUsuario(id) {
    return leer(
      await apiClient.delete(ENDPOINTS.USUARIOS.DELETE(id)),
      'No se pudo eliminar el usuario.',
    )
  },
}

export default usuariosService
