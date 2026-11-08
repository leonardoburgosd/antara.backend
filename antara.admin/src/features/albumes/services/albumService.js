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

export const albumService = {
  async getAlbumes() {
    const data = await leer(
      await apiClient.get(ENDPOINTS.ALBUMES.LIST),
      'No se pudieron cargar los álbumes.',
    )
    return Array.isArray(data) ? data : []
  },

  async crearAlbum(datos) {
    return leer(await apiClient.post(ENDPOINTS.ALBUMES.CREATE, datos), 'No se pudo crear el álbum.')
  },

  async actualizarAlbum(id, datos) {
    return leer(
      await apiClient.put(ENDPOINTS.ALBUMES.UPDATE(id), datos),
      'No se pudo actualizar el álbum.',
    )
  },

  async eliminarAlbum(id) {
    return leer(
      await apiClient.delete(ENDPOINTS.ALBUMES.DELETE(id)),
      'No se pudo eliminar el álbum.',
    )
  },

  async alternarPublicacion(id) {
    return leer(
      await apiClient.put(ENDPOINTS.ALBUMES.PUBLICAR(id)),
      'No se pudo cambiar la publicación del álbum.',
    )
  },
}

export default albumService
