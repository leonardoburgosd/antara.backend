import { apiClient, parseErrorMessage, ApiError } from '../../../services/api/apiClient'
import { ENDPOINTS } from '../../../services/api/endpoints'
import { storage } from '../../../services/storage/localStorage'

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

  // Letra LRC con marcas de tiempo; null si la canción no tiene versión sincronizada.
  async getLetraSincronizada(pistaId, signal) {
    const response = await apiClient.get(ENDPOINTS.PISTAS.LETRA_SINCRONIZADA(pistaId), { signal })
    if (response.status === 404) return null
    if (!response.ok) {
      const msg = await parseErrorMessage(response, 'No se pudo cargar la letra sincronizada.')
      throw new ApiError(msg, response.status)
    }
    const data = await response.json()
    return data?.letraSincronizada || null
  },

  // Paquete offline: audio, portada, letras y datos de la pista en un ZIP.
  async descargarPista(pistaId) {
    const response = await apiClient.get(ENDPOINTS.PISTAS.DESCARGA(pistaId), {
      timeout: 5 * 60 * 1000,
    })

    if (!response.ok) {
      const msg = await parseErrorMessage(response, 'No se pudo descargar la pista.')
      throw new ApiError(msg, response.status)
    }

    const disposicion = response.headers.get('Content-Disposition') || ''
    const utf8 = /filename\*=UTF-8''([^;]+)/i.exec(disposicion)
    const simple = /filename="?([^";]+)"?/i.exec(disposicion)

    let nombre = simple?.[1]?.trim() || 'pista.zip'
    if (utf8?.[1]) {
      try {
        nombre = decodeURIComponent(utf8[1].trim())
      } catch {
        // Se conserva el nombre simple si el encabezado no se puede decodificar.
      }
    }

    return { blob: await response.blob(), nombre }
  },

  getStreamUrl(pistaId) {
    const url = ENDPOINTS.PISTAS.STREAM(pistaId)
    const token = storage.getToken()
    // Las pistas privadas exigen el token tambien en el elemento <audio>,
    // que no envia el encabezado Authorization.
    return token ? `${url}?token=${encodeURIComponent(token)}` : url
  },

  async registerPlay(pistaId) {
    try {
      await apiClient.put(ENDPOINTS.PISTAS.PLAY(pistaId))
    } catch {
      // Ignorar fallo secundario de conteo
    }
  },
}

