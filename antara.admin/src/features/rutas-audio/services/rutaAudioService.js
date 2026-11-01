import { apiClient, parseErrorMessage, ApiError } from '../../../services/api/apiClient'
import { ENDPOINTS } from '../../../services/api/endpoints'

export const rutaAudioService = {
  async getRutas() {
    const response = await apiClient.get(ENDPOINTS.RUTAS_AUDIO.LIST)
    if (!response.ok) {
      const msg = await parseErrorMessage(response, 'No se pudieron cargar las rutas de audio.')
      throw new ApiError(msg, response.status)
    }
    const data = await response.json()
    return Array.isArray(data) ? data : []
  },

  async createRuta(data) {
    const response = await apiClient.post(ENDPOINTS.RUTAS_AUDIO.CREATE, data)
    if (!response.ok) {
      const msg = await parseErrorMessage(response, 'No se pudo crear la ruta de audio.')
      throw new ApiError(msg, response.status)
    }
    return response.json()
  },

  async updateRuta(id, data) {
    const response = await apiClient.put(ENDPOINTS.RUTAS_AUDIO.BY_ID(id), data)
    if (!response.ok) {
      const msg = await parseErrorMessage(response, 'No se pudo actualizar la ruta de audio.')
      throw new ApiError(msg, response.status)
    }
    return response.json()
  },

  async deleteRuta(id) {
    const response = await apiClient.delete(ENDPOINTS.RUTAS_AUDIO.BY_ID(id))
    if (!response.ok) {
      const msg = await parseErrorMessage(response, 'No se pudo eliminar la ruta de audio.')
      throw new ApiError(msg, response.status)
    }
    return true
  },

  async testRuta(id) {
    const response = await apiClient.post(ENDPOINTS.RUTAS_AUDIO.TEST(id))
    if (!response.ok) {
      const msg = await parseErrorMessage(response, 'No se pudo probar la ruta de audio.')
      throw new ApiError(msg, response.status)
    }
    return response.json()
  },

  async getCurrentScan() {
    const response = await apiClient.get(ENDPOINTS.ESCANEO_AUDIO.CURRENT)
    if (response.status === 204) return null
    if (!response.ok) return null
    return response.json()
  },

  async getScanById(id) {
    const response = await apiClient.get(ENDPOINTS.ESCANEO_AUDIO.BY_ID(id))
    if (!response.ok) return null
    return response.json()
  },

  async startScan() {
    const response = await apiClient.post(ENDPOINTS.ESCANEO_AUDIO.START)
    if (!response.ok) {
      const msg = await parseErrorMessage(response, 'No se pudo iniciar el escaneo.')
      throw new ApiError(msg, response.status)
    }
    return response.json()
  },

  async cancelScan(id) {
    const response = await apiClient.post(ENDPOINTS.ESCANEO_AUDIO.CANCEL(id))
    if (!response.ok) {
      const msg = await parseErrorMessage(response, 'No se pudo cancelar el escaneo.')
      throw new ApiError(msg, response.status)
    }
    return response.json()
  },
}

