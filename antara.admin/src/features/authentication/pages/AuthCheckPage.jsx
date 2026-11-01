import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { authService } from '../services/authService'
import { Spinner } from '../../../components/ui/Spinner'

export const AuthCheckPage = () => {
  const navigate = useNavigate()
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    checkSystemStatus()
  }, [])

  const checkSystemStatus = async () => {
    if (authService.hasSession()) {
      navigate('/dashboard', { replace: true })
      setLoading(false)
      return
    }

    try {
      const data = await authService.checkHealth()
      if (data?.adminsRegistrados) {
        navigate('/login', { replace: true })
      } else {
        navigate('/create-admin', { replace: true })
      }
    } catch (error) {
      console.error('Error checking system status:', error)
      navigate('/login', { replace: true })
    } finally {
      setLoading(false)
    }
  }

  if (loading) {
    return (
      <div className="min-h-screen bg-black text-white flex flex-col items-center justify-center gap-4">
        <Spinner size="lg" />
        <div className="text-sm text-white/50">Verificando sistema...</div>
      </div>
    )
  }

  return null
}

export default AuthCheckPage

