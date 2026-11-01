import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useCreateFirstAdmin } from '../hooks/useCreateFirstAdmin'

export const CreateFirstAdminPage = () => {
  const navigate = useNavigate()
  const { createFirstAdmin, loading, error } = useCreateFirstAdmin()

  const [formData, setFormData] = useState({
    email: '',
    password: '',
    nombre: '',
    fechaNacimiento: '',
    genero: 'M',
    pais: '',
    tipo: 'admin',
  })

  const handleChange = (e) => {
    setFormData((prev) => ({
      ...prev,
      [e.target.name]: e.target.value,
    }))
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    try {
      await createFirstAdmin(formData, () => {
        alert('Administrador creado exitosamente')
        navigate('/login')
      })
    } catch {
      // Handled in hook
    }
  }

  return (
    <div className="min-h-screen bg-black text-white flex flex-col items-center justify-center px-6 py-12">
      <div className="w-full max-w-md">
        <h1 className="text-4xl font-bold text-center mb-2">Crear Primer Administrador</h1>
        <p className="text-gray-400 text-center mb-8">No hay administradores registrados</p>

        <form onSubmit={handleSubmit} className="space-y-4">
          <input
            type="email"
            name="email"
            placeholder="Correo electrónico"
            value={formData.email}
            onChange={handleChange}
            required
            className="w-full px-6 py-4 rounded-full bg-transparent border border-gray-600 text-white placeholder-gray-500 focus:outline-none focus:border-white transition-colors"
          />
          <input
            type="password"
            name="password"
            placeholder="Contraseña (mínimo 6 caracteres)"
            value={formData.password}
            onChange={handleChange}
            required
            minLength={6}
            className="w-full px-6 py-4 rounded-full bg-transparent border border-gray-600 text-white placeholder-gray-500 focus:outline-none focus:border-white transition-colors"
          />
          <input
            type="text"
            name="nombre"
            placeholder="Nombre completo"
            value={formData.nombre}
            onChange={handleChange}
            required
            className="w-full px-6 py-4 rounded-full bg-transparent border border-gray-600 text-white placeholder-gray-500 focus:outline-none focus:border-white transition-colors"
          />
          <input
            type="date"
            name="fechaNacimiento"
            value={formData.fechaNacimiento}
            onChange={handleChange}
            required
            className="w-full px-6 py-4 rounded-full bg-transparent border border-gray-600 text-white focus:outline-none focus:border-white transition-colors"
          />
          <select
            name="genero"
            value={formData.genero}
            onChange={handleChange}
            className="w-full px-6 py-4 rounded-full bg-black border border-gray-600 text-white focus:outline-none focus:border-white transition-colors"
          >
            <option value="M">Masculino</option>
            <option value="F">Femenino</option>
          </select>
          <input
            type="text"
            name="pais"
            placeholder="País"
            value={formData.pais}
            onChange={handleChange}
            required
            className="w-full px-6 py-4 rounded-full bg-transparent border border-gray-600 text-white placeholder-gray-500 focus:outline-none focus:border-white transition-colors"
          />

          {error && (
            <div className="text-red-500 text-sm text-center">{error}</div>
          )}

          <button
            type="submit"
            disabled={loading}
            className="w-full py-4 rounded-full bg-white text-black font-medium text-lg hover:bg-gray-100 transition-colors disabled:opacity-50"
          >
            {loading ? 'Creando...' : 'Crear Administrador'}
          </button>
        </form>
      </div>
    </div>
  )
}

export default CreateFirstAdminPage

