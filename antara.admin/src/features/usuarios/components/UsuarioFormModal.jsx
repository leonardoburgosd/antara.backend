import { useEffect, useState } from 'react'
import { Modal } from '../../../components/ui/Modal'
import { Input } from '../../../components/ui/Input'
import { Select } from '../../../components/ui/Select'
import { Button } from '../../../components/ui/Button'
import { IconAlertCircle } from '../../../components/ui/Icons'

const GENEROS = [
  { value: 'M', label: 'Masculino' },
  { value: 'F', label: 'Femenino' },
  { value: 'O', label: 'Otro' },
]

const ESTADOS = [
  { value: 'true', label: 'Activo' },
  { value: 'false', label: 'Inactivo' },
]

const estadoInicial = (usuario) => ({
  nombre: usuario?.nombre ?? '',
  email: usuario?.email ?? '',
  tipo: ['admin', 'administrador'].includes(String(usuario?.tipo || '').toLowerCase()) ? 'admin' : 'usuario',
  estaActivo: usuario ? Boolean(usuario.estaActivo) : true,
  fechaNacimiento: (usuario?.fechaNacimiento ?? '').slice(0, 10),
  genero: usuario?.genero || 'M',
  pais: usuario?.pais ?? '',
  fotoPerfil: usuario?.fotoPerfil ?? '',
  password: '',
})

/** Convierte "1998-04-12" al ISO que espera la API. */
const aIso = (fecha) => {
  if (!fecha) return null
  const valor = new Date(`${fecha}T00:00:00`)
  return Number.isNaN(valor.getTime()) ? null : valor.toISOString()
}

export const UsuarioFormModal = ({ isOpen, usuario, onClose, onSubmit }) => {
  const [valores, setValores] = useState(() => estadoInicial(usuario))
  const [error, setError] = useState('')
  const [guardando, setGuardando] = useState(false)

  useEffect(() => {
    if (!isOpen) return
    setValores(estadoInicial(usuario))
    setError('')
  }, [isOpen, usuario])

  const esEdicion = Boolean(usuario?.id)

  const setCampo = (campo) => (evento) =>
    setValores((actuales) => ({ ...actuales, [campo]: evento.target.value }))

  const validar = () => {
    if (!valores.nombre.trim()) return 'El nombre no puede estar vacío.'
    if (!valores.email.includes('@')) return 'Ingresa un correo electrónico válido.'
    if (!esEdicion && valores.password.length < 6)
      return 'La contraseña debe tener al menos 6 caracteres.'
    if (esEdicion && valores.password && valores.password.length < 6)
      return 'La nueva contraseña debe tener al menos 6 caracteres.'
    return ''
  }

  const handleGuardar = async () => {
    const mensaje = validar()
    if (mensaje) {
      setError(mensaje)
      return
    }

    const payload = {
      nombre: valores.nombre.trim(),
      email: valores.email.trim(),
      tipo: valores.tipo,
      estaActivo: valores.estaActivo,
      genero: valores.genero,
      pais: valores.pais?.trim() || null,
      fotoPerfil: valores.fotoPerfil?.trim() || null,
      fechaNacimiento: aIso(valores.fechaNacimiento) || new Date().toISOString(),
    }

    if (valores.password) payload.password = valores.password

    setGuardando(true)
    setError('')
    try {
      await onSubmit(payload)
    } catch (err) {
      setError(err.message || 'No se pudo guardar el usuario.')
    } finally {
      setGuardando(false)
    }
  }

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={esEdicion ? 'Editar usuario' : 'Nuevo usuario'}
      subtitle={
        esEdicion
          ? 'Actualiza los datos, el rol o el estado de la cuenta.'
          : 'La cuenta podrá iniciar sesión con el correo y la contraseña indicados.'
      }
      size="lg"
    >
      <div className="space-y-4">
        {error && (
          <div className="flex items-start gap-2 rounded-2xl border border-red-500/30 bg-red-500/10 px-4 py-3 text-xs text-red-200">
            <IconAlertCircle className="mt-0.5 h-4 w-4 shrink-0" />
            <span>{error}</span>
          </div>
        )}

        <Input label="Nombre completo" value={valores.nombre} onChange={setCampo('nombre')} placeholder="María Fernanda López" />
        <Input
          label="Correo electrónico"
          type="email"
          value={valores.email}
          onChange={setCampo('email')}
          placeholder="usuario@antara.app"
        />

        <div className="grid gap-4 sm:grid-cols-2">
          <Select label="Rol" value={valores.tipo} onChange={setCampo('tipo')}>
            <option value="usuario" className="bg-[#121212]">Usuario</option>
            <option value="admin" className="bg-[#121212]">Administrador</option>
          </Select>
          <Select
            label="Estado"
            value={String(valores.estaActivo)}
            onChange={(evento) =>
              setValores((actuales) => ({ ...actuales, estaActivo: evento.target.value === 'true' }))
            }
            options={ESTADOS}
          />
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <Input
            label="Fecha de nacimiento"
            type="date"
            value={valores.fechaNacimiento}
            onChange={setCampo('fechaNacimiento')}
          />
          <Select label="Género" value={valores.genero} onChange={setCampo('genero')} options={GENEROS} />
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <Input label="País" value={valores.pais} onChange={setCampo('pais')} placeholder="Perú" />
          <Input
            label="Foto de perfil (URL)"
            value={valores.fotoPerfil}
            onChange={setCampo('fotoPerfil')}
            placeholder="https://..."
          />
        </div>

        <Input
          label={esEdicion ? 'Nueva contraseña (opcional)' : 'Contraseña'}
          type="password"
          value={valores.password}
          onChange={setCampo('password')}
          placeholder={esEdicion ? 'Déjala vacía para conservar la actual' : 'Mínimo 6 caracteres'}
          autoComplete="new-password"
        />

        <div className="flex justify-end gap-3 pt-2">
          <Button variant="ghost" onClick={onClose} disabled={guardando}>
            Cancelar
          </Button>
          <Button onClick={handleGuardar} loading={guardando}>
            {esEdicion ? 'Guardar cambios' : 'Crear usuario'}
          </Button>
        </div>
      </div>
    </Modal>
  )
}

export default UsuarioFormModal
