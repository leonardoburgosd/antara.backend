import { createContext, useCallback, useContext, useEffect, useMemo, useRef, useState } from 'react'
import { pistaService } from '../services/pistaService'

const ColaSubidasContext = createContext(null)

const crearId = () =>
  typeof crypto !== 'undefined' && crypto.randomUUID
    ? crypto.randomUUID()
    : `subida-${Date.now()}-${Math.random().toString(16).slice(2)}`

const normalizarTrabajo = (trabajo) => ({
  ...trabajo,
  id: crearId(),
  estado: 'pendiente',
  creadoEn: Date.now(),
  subcarpeta: trabajo.subcarpeta?.trim() || '',
})

export const ColaSubidasProvider = ({ children }) => {
  const [items, setItems] = useState([])
  const [procesando, setProcesando] = useState(false)
  const [ultimaCompletada, setUltimaCompletada] = useState(null)
  const procesandoRef = useRef(false)
  const montadoRef = useRef(true)

  useEffect(() => {
    montadoRef.current = true
    return () => {
      montadoRef.current = false
    }
  }, [])

  const encolar = useCallback((trabajos) => {
    const lista = (Array.isArray(trabajos) ? trabajos : [trabajos])
      .filter(Boolean)
      .map(normalizarTrabajo)

    if (!lista.length) return 0

    setItems((actuales) => [...actuales, ...lista])
    return lista.length
  }, [])

  const cancelar = useCallback((id) => {
    setItems((actuales) =>
      actuales.filter((item) => item.id !== id || item.estado !== 'pendiente'),
    )
  }, [])

  const quitar = useCallback((id) => {
    setItems((actuales) =>
      actuales.filter((item) => item.id !== id || item.estado === 'subiendo'),
    )
  }, [])

  const reintentar = useCallback((id) => {
    setItems((actuales) =>
      actuales.map((item) =>
        item.id === id && item.estado === 'error'
          ? { ...item, estado: 'pendiente', error: '' }
          : item,
      ),
    )
  }, [])

  const limpiarFinalizadas = useCallback(() => {
    setItems((actuales) =>
      actuales.filter((item) => item.estado === 'pendiente' || item.estado === 'subiendo'),
    )
  }, [])

  useEffect(() => {
    if (procesandoRef.current) return

    const siguiente = items.find((item) => item.estado === 'pendiente')
    if (!siguiente) return

    procesandoRef.current = true
    setProcesando(true)

    const ejecutar = async () => {
      setItems((actuales) =>
        actuales.map((item) =>
          item.id === siguiente.id ? { ...item, estado: 'subiendo', error: '' } : item,
        ),
      )

      try {
        const resultado =
          siguiente.tipo === 'youtube'
            ? await pistaService.subirDesdeUrl({
                url: siguiente.url,
                rutaId: siguiente.rutaId,
                subcarpeta: siguiente.subcarpeta,
              })
            : await pistaService.subirPista({
                archivo: siguiente.archivo,
                rutaId: siguiente.rutaId,
                subcarpeta: siguiente.subcarpeta,
              })

        if (!montadoRef.current) return

        const completada = {
          ...siguiente,
          estado: 'completado',
          nombre: resultado?.nombre || siguiente.nombre,
          resultado,
          completadoEn: Date.now(),
          error: '',
        }

        setItems((actuales) =>
          actuales.map((item) => (item.id === siguiente.id ? completada : item)),
        )
        setUltimaCompletada(completada)
      } catch (err) {
        if (!montadoRef.current) return

        setItems((actuales) =>
          actuales.map((item) =>
            item.id === siguiente.id
              ? {
                  ...item,
                  estado: 'error',
                  error: err?.message || 'No se pudo completar la subida.',
                }
              : item,
          ),
        )
      } finally {
        if (montadoRef.current) {
          procesandoRef.current = false
          setProcesando(false)
        }
      }
    }

    ejecutar()
  }, [items])

  const resumen = useMemo(() => {
    const pendientes = items.filter((item) => item.estado === 'pendiente').length
    const finalizadas = items.filter(
      (item) => item.estado === 'completado' || item.estado === 'error',
    ).length

    return {
      total: items.length,
      pendientes,
      finalizadas,
    }
  }, [items])

  const value = useMemo(
    () => ({
      items,
      procesando,
      ultimaCompletada,
      ...resumen,
      encolar,
      cancelar,
      quitar,
      reintentar,
      limpiarFinalizadas,
    }),
    [
      items,
      procesando,
      ultimaCompletada,
      resumen,
      encolar,
      cancelar,
      quitar,
      reintentar,
      limpiarFinalizadas,
    ],
  )

  return <ColaSubidasContext.Provider value={value}>{children}</ColaSubidasContext.Provider>
}

export const useColaSubidas = () => {
  const contexto = useContext(ColaSubidasContext)
  if (!contexto) {
    throw new Error('useColaSubidas debe usarse dentro de ColaSubidasProvider.')
  }
  return contexto
}

export default ColaSubidasContext
