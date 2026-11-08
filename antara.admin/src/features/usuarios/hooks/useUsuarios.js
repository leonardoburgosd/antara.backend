import { useState, useEffect, useCallback, useMemo } from 'react'
import { usuariosService } from '../services/usuariosService'
import { normalizeString } from '../../../utils/formatters'

const esAdmin = (tipo) => ['admin', 'administrador'].includes(String(tipo || '').toLowerCase())

export function useUsuarios() {
  const [usuarios, setUsuarios] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [busqueda, setBusqueda] = useState('')
  const [filtro, setFiltro] = useState('todos')

  const cargar = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      setUsuarios(await usuariosService.getUsuarios())
    } catch (err) {
      setError(err.message || 'Error de conexión con el servidor.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    cargar()
  }, [cargar])

  const estadisticas = useMemo(
    () => ({
      total: usuarios.length,
      admins: usuarios.filter((u) => esAdmin(u.tipo)).length,
      activos: usuarios.filter((u) => u.estaActivo).length,
      inactivos: usuarios.filter((u) => !u.estaActivo).length,
    }),
    [usuarios],
  )

  const filtrados = useMemo(() => {
    const termino = normalizeString ? normalizeString(busqueda) : busqueda.toLowerCase().trim()

    return usuarios.filter((usuario) => {
      if (filtro === 'admin' && !esAdmin(usuario.tipo)) return false
      if (filtro === 'usuario' && esAdmin(usuario.tipo)) return false
      if (filtro === 'inactivos' && usuario.estaActivo) return false
      if (filtro === 'activos' && !usuario.estaActivo) return false
      if (!termino) return true

      return [usuario.nombre, usuario.email, usuario.pais, usuario.tipo]
        .filter(Boolean)
        .some((valor) =>
          normalizeString
            ? normalizeString(valor).includes(termino)
            : String(valor).toLowerCase().includes(termino),
        )
    })
  }, [usuarios, busqueda, filtro])

  const crear = useCallback(
    async (datos) => {
      await usuariosService.crearUsuario(datos)
      await cargar()
    },
    [cargar],
  )

  const actualizar = useCallback(
    async (id, datos) => {
      const actualizado = await usuariosService.actualizarUsuario(id, datos)
      setUsuarios((actuales) =>
        actuales.map((usuario) => (usuario.id === id ? { ...usuario, ...actualizado } : usuario)),
      )
      return actualizado
    },
    [],
  )

  const eliminar = useCallback(async (id) => {
    await usuariosService.eliminarUsuario(id)
    setUsuarios((actuales) => actuales.filter((usuario) => usuario.id !== id))
  }, [])

  return {
    usuarios,
    filtrados,
    estadisticas,
    loading,
    error,
    busqueda,
    setBusqueda,
    filtro,
    setFiltro,
    cargar,
    crear,
    actualizar,
    eliminar,
    esAdmin,
  }
}

export default useUsuarios
