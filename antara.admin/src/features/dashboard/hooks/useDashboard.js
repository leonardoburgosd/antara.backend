import { useState, useEffect, useCallback } from 'react'
import { dashboardService } from '../services/dashboardService'
import { storage } from '../../../services/storage/localStorage'

export function useDashboard() {
  const [user, setUser] = useState(null)

  // Vuelve a leer la sesión guardada (por ejemplo tras editar el perfil).
  const refreshUser = useCallback(() => setUser(storage.getUser()), [])
  const [dashboard, setDashboard] = useState(null)
  const [health, setHealth] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    setUser(storage.getUser())
  }, [])

  const loadData = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      const data = await dashboardService.getDashboardData()
      setDashboard(data.dashboard)
      setHealth(data.health)
    } catch (err) {
      setError(err.message || 'Error de conexión con el servidor.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    loadData()
  }, [loadData])

  return {
    user,
    refreshUser,
    dashboard,
    health,
    loading,
    isLoading: loading,
    error,
    refetch: loadData,
  }
}

export default useDashboard

