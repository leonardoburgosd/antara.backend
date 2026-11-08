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

  // Proveedores habilitados en el backend (vienen de la configuración del servidor).
  async getMetadataProviders() {
    const response = await apiClient.get(ENDPOINTS.PISTAS.METADATA_PROVIDERS)
    if (!response.ok) return []
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

  // Rutas registradas que pueden recibir archivos subidos desde el panel.
  async getRutasSubida() {
    const response = await apiClient.get(ENDPOINTS.PISTAS.SUBIR_RUTAS)
    if (!response.ok) {
      const msg = await parseErrorMessage(response, 'No se pudieron cargar las rutas de audio.')
      throw new ApiError(msg, response.status)
    }
    const data = await response.json()
    return Array.isArray(data) ? data : []
  },

  // Sube un archivo de audio a la ruta indicada; la API valida la extensión.
  async subirPista({ archivo, rutaId, subcarpeta }) {
    const datos = new FormData()
    datos.append('archivo', archivo)
    if (rutaId) datos.append('rutaId', rutaId)
    datos.append('subcarpeta', subcarpeta?.trim() || '')

    const response = await apiClient.post(ENDPOINTS.PISTAS.SUBIR, datos, {
      timeout: 30 * 60 * 1000,
    })

    if (!response.ok) {
      const msg = await parseErrorMessage(response, 'No se pudo subir la canción.')
      throw new ApiError(msg, response.status)
    }

    return response.json()
  },

  // Descarga el audio de un enlace de YouTube en la ruta indicada.
  async subirDesdeUrl({ url, rutaId, subcarpeta }) {
    const response = await apiClient.post(
      ENDPOINTS.PISTAS.SUBIR_URL,
      { url: url.trim(), rutaId: rutaId || null, subcarpeta: subcarpeta?.trim() || null },
      { timeout: 30 * 60 * 1000 },
    )

    if (!response.ok) {
      const msg = await parseErrorMessage(response, 'No se pudo descargar la canción de YouTube.')
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

