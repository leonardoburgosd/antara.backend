import { useState, useCallback } from 'react'
import { authService } from '../services/authService'

export function useCreateFirstAdmin() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState(false)

  const createFirstAdmin = useCallback(async (formData, onSuccess) => {
    setError('')
    setLoading(true)
    setSuccess(false)
    try {
      const res = await authService.createFirstAdmin(formData)
      setSuccess(true)
      if (onSuccess) {
        onSuccess(res)
      }
      return res
    } catch (err) {
      setError(err.message || 'Error al crear el primer administrador.')
      throw err
    } finally {
      setLoading(false)
    }
  }, [])

  return {
    createFirstAdmin,
    loading,
    isLoading: loading,
    error,
    setError,
    success,
  }
}

export default useCreateFirstAdmin

