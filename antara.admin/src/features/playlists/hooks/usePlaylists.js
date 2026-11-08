import { useState, useEffect, useCallback, useMemo } from 'react'
import { playlistService } from '../services/playlistService'
import { pistaService } from '../../pistas/services/pistaService'
import { normalizeText } from '../../../utils/formatters'

export function usePlaylists() {
  const [playlists, setPlaylists] = useState([])
  const [biblioteca, setBiblioteca] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [busqueda, setBusqueda] = useState('')

  const cargar = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      setPlaylists(await playlistService.getPlaylists())
    } catch (err) {
      setError(err.message || 'Error de conexión con el servidor.')
    } finally {
      setLoading(false)
    }
  }, [])

  // Toda la biblioteca: la playlist puede mezclar canciones de cualquier artista.
  const cargarBiblioteca = useCallback(async () => {
    try {
      setBiblioteca(await pistaService.getPistas())
    } catch {
      // Sin biblioteca el alta de canciones queda vacía, pero la playlist se
      // puede crear y ver igual.
    }
  }, [])

  useEffect(() => {
    cargar()
    cargarBiblioteca()
  }, [cargar, cargarBiblioteca])

  const estadisticas = useMemo(() => {
    const artistas = new Set()
    let canciones = 0

    playlists.forEach((playlist) => {
      canciones += Number(playlist.totalPistas) || 0
      String(playlist.artistas || '')
        .split(',')
        .map((nombre) => normalizeText(nombre))
        .filter(Boolean)
        .forEach((nombre) => artistas.add(nombre))
    })

    return {
      total: playlists.length,
      canciones,
      artistas: artistas.size,
      vacias: playlists.filter((playlist) => !playlist.totalPistas).length,
    }
  }, [playlists])

  const filtradas = useMemo(() => {
    const termino = normalizeText(busqueda)
    if (!termino) return playlists

    return playlists.filter((playlist) =>
      [playlist.nombre, playlist.descripcion, playlist.artistas]
        .filter(Boolean)
        .some((valor) => normalizeText(valor).includes(termino)),
    )
  }, [playlists, busqueda])

  const crear = useCallback(
    async (datos) => {
      const creada = await playlistService.crearPlaylist(datos)
      await cargar()
      return creada
    },
    [cargar],
  )

  const actualizar = useCallback(
    async (id, datos) => {
      const actualizada = await playlistService.actualizarPlaylist(id, datos)
      await cargar()
      return actualizada
    },
    [cargar],
  )

  const eliminar = useCallback(async (id) => {
    await playlistService.eliminarPlaylist(id)
    setPlaylists((actuales) => actuales.filter((playlist) => playlist.id !== id))
  }, [])

  const obtenerPistas = useCallback((id) => playlistService.getPistas(id), [])

  const agregarPista = useCallback(
    async (playlistId, pistaId) => {
      const canciones = await playlistService.agregarPista(playlistId, pistaId)
      await cargar()
      return canciones
    },
    [cargar],
  )

  const quitarPista = useCallback(
    async (playlistId, pistaId) => {
      const canciones = await playlistService.quitarPista(playlistId, pistaId)
      await cargar()
      return canciones
    },
    [cargar],
  )

  return {
    playlists,
    filtradas,
    biblioteca,
    estadisticas,
    loading,
    error,
    busqueda,
    setBusqueda,
    cargar,
    cargarBiblioteca,
    crear,
    actualizar,
    eliminar,
    obtenerPistas,
    agregarPista,
    quitarPista,
  }
}

export default usePlaylists
