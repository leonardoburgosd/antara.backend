import { useState, useEffect, useCallback, useMemo } from 'react'
import { albumService } from '../services/albumService'
import { normalizeString } from '../../../utils/formatters'

export function useAlbumes() {
  const [albumes, setAlbumes] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [busqueda, setBusqueda] = useState('')
  const [filtro, setFiltro] = useState('todos')

  const cargar = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      setAlbumes(await albumService.getAlbumes())
    } catch (err) {
      setError(err.message || 'Error de conexión con el servidor.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    cargar()
  }, [cargar])

  const estadisticas = useMemo(
    () => ({
      total: albumes.length,
      publicados: albumes.filter((album) => album.estaPublicado).length,
      borradores: albumes.filter((album) => !album.estaPublicado).length,
      inactivos: albumes.filter((album) => !album.estaActivo).length,
    }),
    [albumes],
  )

  const filtrados = useMemo(() => {
    const termino = normalizeString ? normalizeString(busqueda) : busqueda.toLowerCase().trim()

    return albumes.filter((album) => {
      if (filtro === 'publicados' && !album.estaPublicado) return false
      if (filtro === 'borradores' && album.estaPublicado) return false
      if (filtro === 'inactivos' && album.estaActivo) return false
      if (filtro === 'activos' && !album.estaActivo) return false
      if (!termino) return true

      return [album.nombre, album.artista, album.descripcion]
        .filter(Boolean)
        .some((valor) =>
          normalizeString
            ? normalizeString(valor).includes(termino)
            : String(valor).toLowerCase().includes(termino),
        )
    })
  }, [albumes, busqueda, filtro])

  const crear = useCallback(
    async (datos) => {
      await albumService.crearAlbum(datos)
      await cargar()
    },
    [cargar],
  )

  const actualizar = useCallback(async (id, datos) => {
    const actualizado = await albumService.actualizarAlbum(id, datos)
    setAlbumes((actuales) =>
      actuales.map((album) => (album.id === id ? { ...album, ...actualizado } : album)),
    )
    return actualizado
  }, [])

  const alternarPublicacion = useCallback(async (album) => {
    const actualizado = await albumService.alternarPublicacion(album.id)
    setAlbumes((actuales) =>
      actuales.map((item) => (item.id === album.id ? { ...item, ...actualizado } : item)),
    )
    return actualizado
  }, [])

  const eliminar = useCallback(async (id) => {
    await albumService.eliminarAlbum(id)
    setAlbumes((actuales) => actuales.filter((album) => album.id !== id))
  }, [])

  return {
    albumes,
    filtrados,
    estadisticas,
    loading,
    error,
    busqueda,
    setBusqueda,
    filtro,
    setFiltro,
    cargar,
    crear,
    actualizar,
    alternarPublicacion,
    eliminar,
  }
}

export default useAlbumes
