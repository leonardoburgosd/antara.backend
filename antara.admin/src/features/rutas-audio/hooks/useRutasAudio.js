import { useState, useEffect, useCallback } from 'react'
import { rutaAudioService } from '../services/rutaAudioService'

export function useRutasAudio({ onToast = () => {} } = {}) {
  const [rutas, setRutas] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const [procesandoId, setProcesandoId] = useState(null)
  const [probandoId, setProbandoId] = useState(null)
  const [prueba, setPrueba] = useState(null)
  const [guardando, setGuardando] = useState(false)
  const [eliminando, setEliminando] = useState(false)

  const cargar = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      const data = await rutaAudioService.getRutas()
      setRutas(data)
    } catch (err) {
      setError(err.message || 'Error de conexión con el servidor.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    cargar()
  }, [cargar])

  const guardar = async (form, rutaEditando) => {
    setGuardando(true)
    try {
      const esEdicion = Boolean(rutaEditando)
      const payload = {
        nombre: form.nombre.trim(),
        rutaFisica: form.rutaFisica.trim(),
        rutaContenedor: form.rutaContenedor.trim(),
        descripcion: form.descripcion.trim(),
        estaActiva: form.estaActiva,
        esPredeterminada: form.esPredeterminada,
      }

      if (esEdicion) {
        await rutaAudioService.updateRuta(rutaEditando.id, payload)
        onToast('Ruta de audio actualizada.')
      } else {
        await rutaAudioService.createRuta(payload)
        onToast('Ruta de audio creada.')
      }

      await cargar()
      return true
    } catch (err) {
      throw err
    } finally {
      setGuardando(false)
    }
  }

  const alternarActiva = async (ruta) => {
    setProcesandoId(ruta.id)
    try {
      const payload = {
        nombre: ruta.nombre,
        rutaFisica: ruta.rutaFisica || '',
        rutaContenedor: ruta.rutaContenedor || '',
        descripcion: ruta.descripcion || '',
        estaActiva: !ruta.estaActiva,
        esPredeterminada: ruta.esPredeterminada,
      }
      await rutaAudioService.updateRuta(ruta.id, payload)
      onToast(ruta.estaActiva ? 'Ruta desactivada.' : 'Ruta activada.')
      await cargar()
    } catch (err) {
      onToast(err.message || 'Error al actualizar la ruta.')
    } finally {
      setProcesandoId(null)
    }
  }

  const marcarPredeterminada = async (ruta) => {
    setProcesandoId(ruta.id)
    try {
      const payload = {
        nombre: ruta.nombre,
        rutaFisica: ruta.rutaFisica || '',
        rutaContenedor: ruta.rutaContenedor || '',
        descripcion: ruta.descripcion || '',
        estaActiva: ruta.estaActiva,
        esPredeterminada: true,
      }
      await rutaAudioService.updateRuta(ruta.id, payload)
      onToast('Ruta predeterminada actualizada.')
      await cargar()
    } catch (err) {
      onToast(err.message || 'Error al marcar predeterminada.')
    } finally {
      setProcesandoId(null)
    }
  }

  const probar = async (ruta) => {
    setProbandoId(ruta.id)
    try {
      const data = await rutaAudioService.testRuta(ruta.id)
      setPrueba({ ruta, data })
    } catch (err) {
      onToast(err.message || 'Error al comprobar la ruta.')
    } finally {
      setProbandoId(null)
    }
  }

  const eliminar = async (ruta) => {
    if (!ruta) return
    setEliminando(true)
    try {
      await rutaAudioService.deleteRuta(ruta.id)
      onToast('Ruta de audio eliminada.')
      await cargar()
      return true
    } catch (err) {
      onToast(err.message || 'Error al eliminar la ruta.')
      throw err
    } finally {
      setEliminando(false)
    }
  }

  return {
    rutas,
    loading,
    error,
    cargar,
    guardar,
    alternarActiva,
    marcarPredeterminada,
    probar,
    eliminar,
    prueba,
    setPrueba,
    procesandoId,
    probandoId,
    guardando,
    eliminando,
  }
}

export default useRutasAudio

