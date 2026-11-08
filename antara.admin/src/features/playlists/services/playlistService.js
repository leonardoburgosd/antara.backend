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

const formularioPlaylist = ({ nombre, descripcion, portada, portadaUrl, quitarPortada }) => {
  const datos = new FormData()
  datos.append('nombre', nombre.trim())
  datos.append('descripcion', descripcion?.trim() || '')
  if (portada) {
    datos.append('imagenDePortada', portada)
  } else {
    datos.append('portadaUrl', portadaUrl?.trim() || '')
    // Un campo vacío llega como null al backend; la bandera quita la portada.
    datos.append('quitarPortada', String(Boolean(quitarPortada)))
  }
  return datos
}

export const playlistService = {
  // Playlists con el conteo de canciones y artistas distintos que reúnen.
  async getPlaylists() {
    const data = await leer(
      await apiClient.get(ENDPOINTS.PLAYLISTS.LIST),
      'No se pudieron cargar las playlists.',
    )
    return Array.isArray(data) ? data : []
  },

  async getPlaylist(id) {
    return leer(await apiClient.get(ENDPOINTS.PLAYLISTS.BY_ID(id)), 'No se pudo cargar la playlist.')
  },

  // Canciones de la playlist con artista, álbum, portada y duración.
  async getPistas(id) {
    const data = await leer(
      await apiClient.get(ENDPOINTS.PLAYLISTS.PISTAS(id)),
      'No se pudieron cargar las canciones de la playlist.',
    )
    return Array.isArray(data) ? data : []
  },

  async crearPlaylist(datos) {
    return leer(
      await apiClient.post(ENDPOINTS.PLAYLISTS.CREATE, formularioPlaylist(datos), {
        timeout: 5 * 60 * 1000,
      }),
      'No se pudo crear la playlist.',
    )
  },

  async actualizarPlaylist(id, datos) {
    return leer(
      await apiClient.put(ENDPOINTS.PLAYLISTS.BY_ID(id), formularioPlaylist(datos), {
        timeout: 5 * 60 * 1000,
      }),
      'No se pudo actualizar la playlist.',
    )
  },

  async eliminarPlaylist(id) {
    return leer(
      await apiClient.delete(ENDPOINTS.PLAYLISTS.BY_ID(id)),
      'No se pudo eliminar la playlist.',
    )
  },

  // Devuelve la lista actualizada de canciones de la playlist.
  async agregarPista(playlistId, pistaId) {
    const data = await leer(
      await apiClient.post(ENDPOINTS.PLAYLISTS.AGREGAR_PISTA(playlistId), { pistaId }),
      'No se pudo agregar la canción.',
    )
    return Array.isArray(data) ? data : []
  },

  async quitarPista(playlistId, pistaId) {
    const data = await leer(
      await apiClient.delete(ENDPOINTS.PLAYLISTS.QUITAR_PISTA(playlistId, pistaId)),
      'No se pudo quitar la canción.',
    )
    return Array.isArray(data) ? data : []
  },
}

export default playlistService
