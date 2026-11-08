import { useEffect, useState } from 'react'
import { usePerfil } from '../hooks/usePerfil'
import { formatDate } from '../../../utils/formatters'

import { Avatar } from '../../../components/ui/Avatar'
import { Badge } from '../../../components/ui/Badge'
import { Button } from '../../../components/ui/Button'
import { Input } from '../../../components/ui/Input'
import { Select } from '../../../components/ui/Select'
import { ErrorBanner } from '../../../components/ui/ErrorBanner'
import { Spinner } from '../../../components/ui/Spinner'
import {
  IconSettings,
  IconCheck,
  IconAlertCircle,
  IconRefresh,
  IconInfo,
} from '../../../components/ui/Icons'

const GENEROS = [
  { value: 'M', label: 'Masculino' },
  { value: 'F', label: 'Femenino' },
  { value: 'O', label: 'Otro' },
]

const aIso = (fecha) => {
  if (!fecha) return null
  const valor = new Date(`${fecha}T00:00:00`)
  return Number.isNaN(valor.getTime()) ? null : valor.toISOString()
}

const esAdmin = (tipo) => ['admin', 'administrador'].includes(String(tipo || '').toLowerCase())

const DatoCuenta = ({ label, children }) => (
  <div className="flex items-center justify-between gap-3 border-b border-white/5 py-2.5 last:border-0">
    <span className="text-[10px] uppercase tracking-[0.2em] text-white/30">{label}</span>
    <span className="text-xs text-white/70">{children}</span>
  </div>
)

export const ConfiguracionPage = ({ onToast = () => {}, onPerfilActualizado }) => {
  const { perfil, loading, error, cargar, guardarPerfil, cambiarPassword } = usePerfil()

  const [datos, setDatos] = useState(null)
  const [guardando, setGuardando] = useState(false)
  const [errorDatos, setErrorDatos] = useState('')

  const [claves, setClaves] = useState({ actual: '', nueva: '', repetir: '' })
  const [guardandoClave, setGuardandoClave] = useState(false)
  const [errorClave, setErrorClave] = useState('')

  useEffect(() => {
    if (!perfil) return
    setDatos({
      nombre: perfil.nombre ?? '',
      email: perfil.email ?? '',
      fechaNacimiento: (perfil.fechaNacimiento ?? '').slice(0, 10),
      genero: perfil.genero || 'M',
      pais: perfil.pais ?? '',
      fotoPerfil: perfil.fotoPerfil ?? '',
    })
  }, [perfil])

  const setCampo = (campo) => (evento) =>
    setDatos((actuales) => ({ ...actuales, [campo]: evento.target.value }))

  const handleGuardarDatos = async () => {
    if (!datos) return
    if (!datos.nombre.trim()) {
      setErrorDatos('El nombre no puede estar vacío.')
      return
    }
    if (!datos.email.includes('@')) {
      setErrorDatos('Ingresa un correo electrónico válido.')
      return
    }

    setGuardando(true)
    setErrorDatos('')
    try {
      const actualizado = await guardarPerfil({
        nombre: datos.nombre.trim(),
        email: datos.email.trim(),
        fechaNacimiento: aIso(datos.fechaNacimiento),
        genero: datos.genero,
        pais: datos.pais?.trim() || null,
        fotoPerfil: datos.fotoPerfil?.trim() || null,
      })
      onPerfilActualizado?.(actualizado)
      onToast('Tus datos se actualizaron correctamente.')
    } catch (err) {
      setErrorDatos(err.message || 'No se pudieron guardar tus datos.')
    } finally {
      setGuardando(false)
    }
  }

  const handleCambiarPassword = async () => {
    if (!claves.actual) {
      setErrorClave('Ingresa tu contraseña actual.')
      return
    }
    if (claves.nueva.length < 6) {
      setErrorClave('La nueva contraseña debe tener al menos 6 caracteres.')
      return
    }
    if (claves.nueva !== claves.repetir) {
      setErrorClave('Las contraseñas nuevas no coinciden.')
      return
    }

    setGuardandoClave(true)
    setErrorClave('')
    try {
      await cambiarPassword(claves.actual, claves.nueva)
      setClaves({ actual: '', nueva: '', repetir: '' })
      onToast('Contraseña actualizada correctamente.')
    } catch (err) {
      setErrorClave(err.message || 'No se pudo cambiar la contraseña.')
    } finally {
      setGuardandoClave(false)
    }
  }

  if (loading && !perfil) {
    return (
      <div className="flex items-center justify-center rounded-[2rem] border border-white/10 bg-white/[0.03] py-24">
        <Spinner />
      </div>
    )
  }

  return (
    <section className="space-y-5 pb-10">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <p className="text-[10px] uppercase tracking-[0.35em] text-white/35">Cuenta</p>
          <h2 className="mt-2 text-2xl font-semibold tracking-tight sm:text-3xl">Configuración</h2>
          <p className="mt-2 max-w-2xl text-sm leading-6 text-white/40">
            Actualiza tus datos personales y la contraseña con la que accedes al panel.
          </p>
        </div>
        <Button variant="outline" icon={IconRefresh} onClick={cargar} disabled={loading}>
          Recargar
        </Button>
      </div>

      {error && <ErrorBanner message={error} onRetry={cargar} />}

      <div className="grid gap-5 xl:grid-cols-[1.45fr_1fr]">
        <div className="rounded-[2rem] border border-white/10 bg-white/[0.03] p-6 sm:p-7">
          <div className="flex flex-wrap items-center gap-4">
            <Avatar name={datos?.nombre || perfil?.nombre} src={perfil?.fotoPerfil} size="xl" />
            <div className="min-w-0">
              <h3 className="text-lg font-semibold">Datos personales</h3>
              <p className="mt-1 text-xs text-white/35">
                Así apareces en el panel y en los registros de la plataforma.
              </p>
              <div className="mt-2 flex flex-wrap gap-1.5">
                <Badge variant={esAdmin(perfil?.tipo) ? 'purple' : 'default'} size="sm">
                  {esAdmin(perfil?.tipo) ? 'Administrador' : 'Usuario'}
                </Badge>
                <Badge variant={perfil?.estaActivo ? 'success' : 'danger'} size="sm">
                  {perfil?.estaActivo ? 'Cuenta activa' : 'Cuenta inactiva'}
                </Badge>
              </div>
            </div>
          </div>

          {errorDatos && (
            <div className="mt-5 flex items-start gap-2 rounded-2xl border border-red-500/30 bg-red-500/10 px-4 py-3 text-xs text-red-200">
              <IconAlertCircle className="mt-0.5 h-4 w-4 shrink-0" />
              <span>{errorDatos}</span>
            </div>
          )}

          {datos && (
            <div className="mt-6 space-y-4">
              <Input label="Nombre completo" value={datos.nombre} onChange={setCampo('nombre')} />
              <Input label="Correo electrónico" type="email" value={datos.email} onChange={setCampo('email')} />

              <div className="grid gap-4 sm:grid-cols-2">
                <Input
                  label="Fecha de nacimiento"
                  type="date"
                  value={datos.fechaNacimiento}
                  onChange={setCampo('fechaNacimiento')}
                />
                <Select label="Género" value={datos.genero} onChange={setCampo('genero')} options={GENEROS} />
              </div>

              <div className="grid gap-4 sm:grid-cols-2">
                <Input label="País" value={datos.pais} onChange={setCampo('pais')} placeholder="Perú" />
                <Input
                  label="Foto de perfil (URL)"
                  value={datos.fotoPerfil}
                  onChange={setCampo('fotoPerfil')}
                  placeholder="https://..."
                />
              </div>

              <div className="flex justify-end pt-1">
                <Button icon={IconCheck} onClick={handleGuardarDatos} loading={guardando}>
                  Guardar cambios
                </Button>
              </div>
            </div>
          )}
        </div>

        <div className="space-y-5">
          <div className="rounded-[2rem] border border-white/10 bg-white/[0.03] p-6">
            <h3 className="text-base font-semibold">Información de la cuenta</h3>
            <div className="mt-4">
              <DatoCuenta label="Rol">{esAdmin(perfil?.tipo) ? 'Administrador' : 'Usuario'}</DatoCuenta>
              <DatoCuenta label="Estado">{perfil?.estaActivo ? 'Activo' : 'Inactivo'}</DatoCuenta>
              <DatoCuenta label="Miembro desde">{formatDate(perfil?.fechaRegistro)}</DatoCuenta>
              <DatoCuenta label="Identificador">
                <span className="font-mono text-[10px] text-white/40">{perfil?.id || '—'}</span>
              </DatoCuenta>
            </div>
            <p className="mt-3 flex items-start gap-2 text-[11px] leading-5 text-white/30">
              <IconInfo className="mt-0.5 h-3.5 w-3.5 shrink-0" />
              El rol y el estado de la cuenta los administra otro administrador desde Usuarios.
            </p>
          </div>

          <div className="rounded-[2rem] border border-white/10 bg-white/[0.03] p-6">
            <div className="flex items-center gap-2">
              <IconSettings className="h-4 w-4 text-white/40" />
              <h3 className="text-base font-semibold">Seguridad</h3>
            </div>
            <p className="mt-1 text-xs text-white/35">Cambia la contraseña de tu cuenta.</p>

            {errorClave && (
              <div className="mt-4 flex items-start gap-2 rounded-2xl border border-red-500/30 bg-red-500/10 px-4 py-3 text-xs text-red-200">
                <IconAlertCircle className="mt-0.5 h-4 w-4 shrink-0" />
                <span>{errorClave}</span>
              </div>
            )}

            <div className="mt-4 space-y-3">
              <Input
                label="Contraseña actual"
                type="password"
                value={claves.actual}
                onChange={(evento) => setClaves((actual) => ({ ...actual, actual: evento.target.value }))}
                autoComplete="current-password"
              />
              <Input
                label="Nueva contraseña"
                type="password"
                value={claves.nueva}
                onChange={(evento) => setClaves((actual) => ({ ...actual, nueva: evento.target.value }))}
                placeholder="Mínimo 6 caracteres"
                autoComplete="new-password"
              />
              <Input
                label="Repetir nueva contraseña"
                type="password"
                value={claves.repetir}
                onChange={(evento) => setClaves((actual) => ({ ...actual, repetir: evento.target.value }))}
                autoComplete="new-password"
              />
              <Button
                variant="secondary"
                className="w-full"
                onClick={handleCambiarPassword}
                loading={guardandoClave}
              >
                Actualizar contraseña
              </Button>
            </div>
          </div>
        </div>
      </div>
    </section>
  )
}

export default ConfiguracionPage
