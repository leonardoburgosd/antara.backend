import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useLogin } from '../hooks/useLogin'
import { authService } from '../services/authService'

export const LoginPage = () => {
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const { login, loading, error, setError } = useLogin()

  useEffect(() => {
    if (authService.hasSession()) {
      navigate('/dashboard', { replace: true })
      return
    }

    if (window.location.search.includes('sesion=expirada')) {
      setError('Tu sesión expiró. Inicia sesión nuevamente.')
    }
  }, [navigate, setError])

  const handleSubmit = async (e) => {
    e.preventDefault()
    try {
      await login(email, password, () => {
        navigate('/dashboard', { replace: true })
      })
    } catch {
      // Error is already captured in the hook's state
    }
  }

  return (
    <div className="min-h-screen bg-black text-white flex flex-col items-center justify-between px-6 py-12">
      {/* Animated Graphic */}
      <div className="mb-12 relative w-72 h-72 mt-8">
        <svg viewBox="0 0 200 200" className="w-full h-full">
          <g>
            {[...Array(25)].map((_, i) => {
              const x1 = Math.random() * 200
              const y1 = Math.random() * 200
              const x2 = Math.random() * 200
              const y2 = Math.random() * 200
              return (
                <line
                  key={i}
                  x1={x1}
                  y1={y1}
                  x2={x2}
                  y2={y2}
                  stroke="white"
                  strokeWidth="0.8"
                  opacity={Math.random() * 0.7 + 0.3}
                  className="animate-float"
                  style={{
                    animationDelay: `${i * 0.08}s`,
                    animationDuration: `${4 + Math.random() * 3}s`,
                  }}
                />
              )
            })}
            {[...Array(12)].map((_, i) => (
              <circle
                key={`circle-${i}`}
                cx={Math.random() * 200}
                cy={Math.random() * 200}
                r="0.8"
                fill="white"
                opacity={Math.random() * 0.5 + 0.5}
                className="animate-pulse"
                style={{
                  animationDelay: `${i * 0.2}s`,
                  animationDuration: `${2 + Math.random() * 2}s`,
                }}
              />
            ))}
          </g>
        </svg>
      </div>

      {/* Welcome Text */}
      <div className="text-center mb-10">
        <h1 className="text-5xl md:text-6xl font-bold mb-2 tracking-tight">Welcome</h1>
        <p className="text-gray-400 text-base">Your journey starts from here</p>
      </div>

      {/* Login Form */}
      <form onSubmit={handleSubmit} className="w-full max-w-sm space-y-3 mb-8">
        <input
          type="email"
          placeholder="Email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
          autoComplete="email"
          className="w-full px-6 py-4 rounded-full bg-transparent border border-gray-600 text-white placeholder-gray-500 focus:outline-none focus:border-white transition-colors"
        />
        <input
          type="password"
          placeholder="Password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
          autoComplete="current-password"
          className="w-full px-6 py-4 rounded-full bg-transparent border border-gray-600 text-white placeholder-gray-500 focus:outline-none focus:border-white transition-colors"
        />

        {error && (
          <div className="text-red-500 text-sm text-center px-2">{error}</div>
        )}

        <button
          type="submit"
          disabled={loading}
          className="w-full py-4 rounded-full bg-white text-black font-medium text-lg hover:bg-gray-100 transition-colors mt-4 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {loading ? 'Ingresando...' : 'Continue'}
        </button>
      </form>

      {/* Footer */}
      <div className="text-center text-xs text-gray-500 px-4 mb-4">
        <p>By pressing on "Continue" you agree</p>
        <p>
          to our <span className="underline cursor-pointer">Terms of Service</span> and{' '}
          <span className="underline cursor-pointer">Privacy Policy</span>
        </p>
      </div>

      {/* Home Indicator */}
      <div className="w-32 h-1 bg-white rounded-full"></div>
    </div>
  )
}

export default LoginPage

