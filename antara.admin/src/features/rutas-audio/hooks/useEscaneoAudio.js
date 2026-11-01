import { useState, useEffect, useRef, useCallback } from 'react'
import { rutaAudioService } from '../services/rutaAudioService'

export function useEscaneoAudio({ onToast = () => {} } = {}) {
  const [escaneo, setEscaneo] = useState(null)
  const [iniciando, setIniciando] = useState(false)
  const [cancelando, setCancelando] = useState(false)
  const [error, setError] = useState('')
  const estadoPrevio = useRef(null)

  const cargarActual = useCallback(async () => {
    try {
      const data = await rutaAudioService.getCurrentScan()
      setEscaneo(data)
    } catch {
      // Ignored
    }
  }, [])

  useEffect(() => {
    cargarActual()
  }, [cargarActual])

  // Polling when active
  useEffect(() => {
    if (!escaneo?.activo || !escaneo?.id) return undefined

    const timer = setInterval(async () => {
      try {
        const data = await rutaAudioService.getScanById(escaneo.id)
        if (data) setEscaneo(data)
      } catch {
        // Retry next time
      }
    }, 1500)

    return () => clearInterval(timer)
  }, [escaneo?.activo, escaneo?.id])

  // Status transitions notifications
  useEffect(() => {
    if (!escaneo) return

    const anterior = estadoPrevio.current
    if (anterior && anterior !== escaneo.estado && !escaneo.activo) {
      if (escaneo.estado === 'completado') {
        onToast(
          `Escaneo completado: ${escaneo.nuevos} nuevas, ${escaneo.sinMetadatos} sin metadatos, ${escaneo.errores} errores.`,
        )
      } else if (escaneo.estado === 'cancelado') {
        onToast('Escaneo cancelado.')
      } else if (escaneo.estado === 'error') {
        onToast(escaneo.mensaje || 'El escaneo de biblioteca falló.')
      }
    }

    estadoPrevio.current = escaneo.estado
  }, [escaneo, onToast])

  const iniciarEscaneo = async () => {
    setIniciando(true)
    setError('')
    try {
      const data = await rutaAudioService.startScan()
      estadoPrevio.current = data.estado
      setEscaneo(data)
      onToast('Escaneo de biblioteca iniciado.')
    } catch (err) {
      setError(err.message || 'No se pudo iniciar el escaneo.')
    } finally {
      setIniciando(false)
    }
  }

  const cancelarEscaneo = async () => {
    if (!escaneo?.id) return
    setCancelando(true)
    try {
      await rutaAudioService.cancelScan(escaneo.id)
      onToast('Cancelando escaneo...')
    } catch (err) {
      onToast(err.message || 'No se pudo cancelar el escaneo.')
    } finally {
      setCancelando(false)
    }
  }

  return {
    escaneo,
    iniciando,
    cancelando,
    error,
    iniciarEscaneo,
    cancelarEscaneo,
    recargarEscaneo: cargarActual,
  }
}

export default useEscaneoAudio

