import { useState, useEffect, useCallback, useMemo } from 'react'
import { pistaService } from '../services/pistaService'
import { normalizeString } from '../../../utils/formatters'

export function usePistas() {
  const [pistas, setPistas] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [busqueda, setBusqueda] = useState('')
  const [filtro, setFiltro] = useState('todas')

  const cargar = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      const data = await pistaService.getPistas()
      setPistas(data)
    } catch (err) {
      setError(err.message || 'Error de conexión con el servidor.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    cargar()
  }, [cargar])

  const conteos = useMemo(() => {
    const conLetra = pistas.filter((pista) => pista.tieneLetra).length
    return {
      todas: pistas.length,
      letra: conLetra,
      'sin-letra': pistas.length - conLetra,
    }
  }, [pistas])

  const estadisticas = useMemo(() => {
    const albumes = new Set(pistas.map((pista) => pista.albumId).filter(Boolean)).size
    const reproducciones = pistas.reduce((total, pista) => total + (Number(pista.reproducciones) || 0), 0)
    const duracion = pistas.reduce((total, pista) => total + (Number(pista.duracionSegundos) || 0), 0)
    return {
      total: pistas.length,
      conLetra: conteos.letra,
      albumes,
      reproducciones,
      duracion,
    }
  }, [pistas, conteos.letra])

  const filtradas = useMemo(() => {
    const termino = normalizeString ? normalizeString(busqueda) : busqueda.toLowerCase().trim()

    return pistas.filter((pista) => {
      if (filtro === 'letra' && !pista.tieneLetra) return false
      if (filtro === 'sin-letra' && pista.tieneLetra) return false
      if (!termino) return true

      return [pista.nombre, pista.interprete, pista.albumNombre, pista.generoNombre, pista.anoCreacion]
        .filter((valor) => valor !== null && valor !== undefined && valor !== '')
        .some((valor) =>
          normalizeString
            ? normalizeString(valor).includes(termino)
            : String(valor).toLowerCase().includes(termino),
        )
    })
  }, [pistas, busqueda, filtro])

  const actualizarPistaLocal = useCallback((actualizada) => {
    if (!actualizada?.id) return
    setPistas((actuales) =>
      actuales.map((pista) => (pista.id === actualizada.id ? actualizada : pista)),
    )
  }, [])

  return {
    pistas,
    filtradas,
    conteos,
    estadisticas,
    loading,
    error,
    busqueda,
    setBusqueda,
    filtro,
    setFiltro,
    cargar,
    actualizarPistaLocal,
  }
}

export default usePistas

