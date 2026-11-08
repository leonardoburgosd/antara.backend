import { useState } from 'react'
import { useUsuarios } from '../hooks/useUsuarios'
import { usePaginacion } from '../../../hooks/usePaginacion'
import { storage } from '../../../services/storage/localStorage'
import { formatNumber } from '../../../utils/formatters'

import UsuariosTable, { UsuariosCardsMobile } from '../components/UsuariosTable'
import UsuarioFormModal from '../components/UsuarioFormModal'

import { Input } from '../../../components/ui/Input'
import { Button } from '../../../components/ui/Button'
import { EmptyState } from '../../../components/ui/EmptyState'
import { Pagination } from '../../../components/ui/Pagination'
import { ErrorBanner } from '../../../components/ui/ErrorBanner'
import { SkeletonTable } from '../../../components/ui/Skeleton'
import { ConfirmDialog } from '../../../components/ui/ConfirmDialog'
import { IconUsers, IconUserPlus, IconRefresh, IconSearch, IconClose } from '../../../components/ui/Icons'

const FILTROS = [
  { id: 'todos', label: 'Todos' },
  { id: 'admin', label: 'Administradores' },
  { id: 'usuario', label: 'Usuarios' },
  { id: 'activos', label: 'Activos' },
  { id: 'inactivos', label: 'Inactivos' },
]

const MetricCard = ({ label, value, hint }) => (
  <div className="rounded-3xl border border-white/10 bg-white/[0.03] p-5">
    <p className="text-[10px] uppercase tracking-[0.25em] text-white/30">{label}</p>
    <p className="mt-3 text-3xl font-semibold tracking-tight">{formatNumber(value)}</p>
    {hint && <p className="mt-1 text-[11px] text-white/30">{hint}</p>}
  </div>
)

export const UsuariosPage = ({ onToast = () => {} }) => {
  const {
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
  } = useUsuarios()

  const usuarioActualId = storage.getUser()?.id
  const paginacion = usePaginacion(filtrados, { resetDeps: [busqueda, filtro] })
  const [formulario, setFormulario] = useState({ abierto: false, usuario: null })
  const [porEliminar, setPorEliminar] = useState(null)
  const [eliminando, setEliminando] = useState(false)

  const guardar = async (payload) => {
    if (formulario.usuario) {
      await actualizar(formulario.usuario.id, payload)
      onToast('Usuario actualizado correctamente.')
    } else {
      await crear(payload)
      onToast('Usuario creado correctamente.')
    }
    setFormulario({ abierto: false, usuario: null })
  }

  const alternarEstado = async (usuario) => {
    if (usuario.id === usuarioActualId) {
      onToast('No puedes desactivar tu propia cuenta.')
      return
    }

    try {
      await actualizar(usuario.id, { estaActivo: !usuario.estaActivo })
      onToast(usuario.estaActivo ? 'Cuenta desactivada.' : 'Cuenta activada.')
    } catch (err) {
      onToast(err.message || 'No se pudo cambiar el estado de la cuenta.')
    }
  }

  const confirmarEliminar = async () => {
    if (!porEliminar) return
    setEliminando(true)
    try {
      await eliminar(porEliminar.id)
      onToast('Usuario eliminado correctamente.')
      setPorEliminar(null)
    } catch (err) {
      onToast(err.message || 'No se pudo eliminar el usuario.')
    } finally {
      setEliminando(false)
    }
  }

  return (
    <section className="space-y-5">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <p className="text-[10px] uppercase tracking-[0.35em] text-white/35">Administración</p>
          <h2 className="mt-2 text-2xl font-semibold tracking-tight sm:text-3xl">Usuarios</h2>
          <p className="mt-2 max-w-2xl text-sm leading-6 text-white/40">
            Crea cuentas, asigna roles y controla quién puede acceder a Antara.
          </p>
        </div>
        <div className="flex flex-wrap gap-2">
          <Button variant="outline" icon={IconRefresh} onClick={cargar} disabled={loading}>
            Actualizar
          </Button>
          <Button
            icon={IconUserPlus}
            onClick={() => setFormulario({ abierto: true, usuario: null })}
          >
            Nuevo usuario
          </Button>
        </div>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <MetricCard label="Usuarios" value={estadisticas.total} hint="Cuentas registradas" />
        <MetricCard label="Administradores" value={estadisticas.admins} hint="Con acceso al panel" />
        <MetricCard label="Activos" value={estadisticas.activos} hint="Pueden iniciar sesión" />
        <MetricCard label="Inactivos" value={estadisticas.inactivos} hint="Cuentas deshabilitadas" />
      </div>

      <div className="flex flex-col gap-3 rounded-3xl border border-white/10 bg-white/[0.03] p-4 lg:flex-row lg:items-center">
        <Input
          icon={IconSearch}
          value={busqueda}
          onChange={(evento) => setBusqueda(evento.target.value)}
          onClear={() => setBusqueda('')}
          placeholder="Buscar por nombre, correo o país"
          containerClassName="w-full lg:max-w-sm"
        />
        <div className="flex flex-wrap gap-2 lg:ml-auto">
          {FILTROS.map((item) => (
            <button
              key={item.id}
              type="button"
              onClick={() => setFiltro(item.id)}
              className={`rounded-full border px-3 py-1.5 text-xs transition ${
                filtro === item.id
                  ? 'border-white bg-white text-black'
                  : 'border-white/10 text-white/45 hover:border-white/30 hover:text-white'
              }`}
            >
              {item.label}
            </button>
          ))}
        </div>
      </div>

      {error && <ErrorBanner message={error} onRetry={cargar} />}

      {loading ? (
        <div className="rounded-[1.75rem] border border-white/10 bg-white/[0.03] p-6">
          <SkeletonTable rows={5} cols={5} />
        </div>
      ) : usuarios.length === 0 ? (
        <div className="rounded-[1.75rem] border border-white/10 bg-white/[0.03]">
          <EmptyState
            icon={IconUsers}
            title="Aún no hay usuarios"
            description="Crea la primera cuenta para empezar a gestionar el acceso a la plataforma."
            action={
              <Button size="sm" icon={IconUserPlus} onClick={() => setFormulario({ abierto: true, usuario: null })}>
                Crear usuario
              </Button>
            }
          />
        </div>
      ) : filtrados.length === 0 ? (
        <div className="rounded-[1.75rem] border border-white/10 bg-white/[0.03]">
          <EmptyState
            icon={IconSearch}
            title="Sin resultados"
            description="No hay cuentas que coincidan con la búsqueda o el filtro seleccionado."
            action={
              <Button
                size="sm"
                variant="outline"
                icon={IconClose}
                onClick={() => {
                  setBusqueda('')
                  setFiltro('todos')
                }}
              >
                Limpiar filtros
              </Button>
            }
          />
        </div>
      ) : (
        <>
          <UsuariosTable
            usuarios={paginacion.visibles}
            usuarioActualId={usuarioActualId}
            onEditar={(usuario) => setFormulario({ abierto: true, usuario })}
            onAlternarEstado={alternarEstado}
            onEliminar={setPorEliminar}
          />
          <UsuariosCardsMobile
            usuarios={paginacion.visibles}
            usuarioActualId={usuarioActualId}
            onEditar={(usuario) => setFormulario({ abierto: true, usuario })}
            onAlternarEstado={alternarEstado}
            onEliminar={setPorEliminar}
          />
          <Pagination
            pagina={paginacion.pagina}
            totalPaginas={paginacion.totalPaginas}
            desde={paginacion.desde}
            hasta={paginacion.hasta}
            total={paginacion.total}
            tamano={paginacion.tamano}
            onPagina={paginacion.setPagina}
            onTamano={paginacion.cambiarTamano}
          />
        </>
      )}

      <UsuarioFormModal
        isOpen={formulario.abierto}
        usuario={formulario.usuario}
        onClose={() => setFormulario({ abierto: false, usuario: null })}
        onSubmit={guardar}
      />

      <ConfirmDialog
        isOpen={Boolean(porEliminar)}
        onClose={() => setPorEliminar(null)}
        onConfirm={confirmarEliminar}
        isLoading={eliminando}
        title="Eliminar usuario"
        message={
          porEliminar
            ? `Se eliminará la cuenta de ${porEliminar.nombre} (${porEliminar.email}). Esta acción no se puede deshacer.`
            : ''
        }
        confirmLabel="Eliminar"
      />
    </section>
  )
}

export default UsuariosPage
