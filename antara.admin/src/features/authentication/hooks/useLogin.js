import { useState, useCallback } from 'react'
import { authService } from '../services/authService'

export function useLogin() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const login = useCallback(async (email, password, onSuccess) => {
    setError('')
    setLoading(true)
    try {
      const user = await authService.login(email, password)
      if (onSuccess) {
        onSuccess(user)
      }
      return user
    } catch (err) {
      setError(err.message || 'Error al iniciar sesión.')
      throw err
    } finally {
      setLoading(false)
    }
  }, [])

  return {
    login,
    loading,
    isLoading: loading,
    error,
    setError,
  }
}

export default useLogin

