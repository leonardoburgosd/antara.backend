import { useState, useEffect, useCallback } from 'react'
import { configuracionService } from '../services/configuracionService'
import { storage } from '../../../services/storage/localStorage'

export function usePerfil() {
  const [perfil, setPerfil] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const cargar = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      setPerfil(await configuracionService.getPerfil())
    } catch (err) {
      setError(err.message || 'No se pudo cargar tu perfil.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    cargar()
  }, [cargar])

  const guardarPerfil = useCallback(async (datos) => {
    const actualizado = await configuracionService.actualizarPerfil(datos)
    setPerfil(actualizado)
    // Mantiene sincronizada la sesión (sidebar, saludo del dashboard).
    storage.updateUser(actualizado)
    return actualizado
  }, [])

  const cambiarPassword = useCallback(
    (actual, nueva) => configuracionService.cambiarPassword(actual, nueva),
    [],
  )

  return { perfil, loading, error, cargar, guardarPerfil, cambiarPassword }
}

export default usePerfil
