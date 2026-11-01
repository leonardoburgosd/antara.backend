import { apiClient, parseErrorMessage, ApiError } from '../../../services/api/apiClient'
import { ENDPOINTS } from '../../../services/api/endpoints'

export const pistaService = {
  async getPistas() {
    const response = await apiClient.get(ENDPOINTS.PISTAS.LIST)
    if (!response.ok) {
      const msg = await parseErrorMessage(response, 'No se pudieron cargar las pistas.')
      throw new ApiError(msg, response.status)
    }
    const data = await response.json()
    return Array.isArray(data) ? data : []
  },

  async searchMetadata(cadena, proveedor = 'todos', signal) {
    const params = new URLSearchParams({
      cadena: cadena.trim(),
      proveedor: proveedor || 'todos',
    })
    const response = await apiClient.get(`${ENDPOINTS.PISTAS.SEARCH_METADATA}?${params.toString()}`, { signal })
    if (!response.ok) {
      const msg = await parseErrorMessage(response, 'No se pudieron buscar coincidencias.')
      throw new ApiError(msg, response.status)
    }
    const data = await response.json()
    return Array.isArray(data) ? data : []
  },

  async updateMetadata(pistaId, idExterno, proveedor) {
    const response = await apiClient.post(ENDPOINTS.PISTAS.UPDATE_METADATA(pistaId), {
      idExterno,
      proveedor,
    })
    if (!response.ok) {
      const msg = await parseErrorMessage(response, 'No se pudieron actualizar los metadatos.')
      throw new ApiError(msg, response.status)
    }
    return response.json()
  },

  getStreamUrl(pistaId) {
    return ENDPOINTS.PISTAS.STREAM(pistaId)
  },

  async registerPlay(pistaId) {
    try {
      await apiClient.put(ENDPOINTS.PISTAS.PLAY(pistaId))
    } catch {
      // Ignorar fallo secundario de conteo
    }
  },
}

