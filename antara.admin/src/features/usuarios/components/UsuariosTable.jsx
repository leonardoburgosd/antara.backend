import { Avatar } from '../../../components/ui/Avatar'
import { Badge } from '../../../components/ui/Badge'
import { formatDate } from '../../../utils/formatters'
import { IconEdit, IconPower, IconTrash } from '../../../components/ui/Icons'

const esAdmin = (tipo) => ['admin', 'administrador'].includes(String(tipo || '').toLowerCase())

const Acciones = ({ usuario, esPropio, onEditar, onAlternarEstado, onEliminar }) => (
  <div className="flex items-center justify-end gap-1.5">
    <button
      type="button"
      onClick={() => onEditar(usuario)}
      className="flex h-8 w-8 items-center justify-center rounded-full border border-white/10 text-white/50 transition hover:border-white/30 hover:text-white"
      title="Editar usuario"
      aria-label={`Editar ${usuario.nombre}`}
    >
      <IconEdit className="h-3.5 w-3.5" />
    </button>
    <button
      type="button"
      onClick={() => onAlternarEstado(usuario)}
      className="flex h-8 w-8 items-center justify-center rounded-full border border-white/10 text-white/50 transition hover:border-white/30 hover:text-white"
      title={usuario.estaActivo ? 'Desactivar cuenta' : 'Activar cuenta'}
      aria-label={usuario.estaActivo ? 'Desactivar cuenta' : 'Activar cuenta'}
    >
      <IconPower className="h-3.5 w-3.5" />
    </button>
    <button
      type="button"
      onClick={() => onEliminar(usuario)}
      disabled={esPropio}
      className="flex h-8 w-8 items-center justify-center rounded-full border border-red-500/20 text-red-300/80 transition hover:border-red-500/50 hover:bg-red-500/10 disabled:cursor-not-allowed disabled:opacity-30"
      title={esPropio ? 'No puedes eliminar tu propia cuenta' : 'Eliminar usuario'}
      aria-label={`Eliminar ${usuario.nombre}`}
    >
      <IconTrash className="h-3.5 w-3.5" />
    </button>
  </div>
)

export const UsuariosTable = ({
  usuarios = [],
  usuarioActualId,
  onEditar,
  onAlternarEstado,
  onEliminar,
}) => {
  return (
    <div className="hidden overflow-hidden rounded-[1.75rem] border border-white/10 bg-white/[0.03] md:block">
      <div className="overflow-x-auto">
        <table className="w-full min-w-[860px] text-left text-sm">
          <thead>
            <tr className="border-b border-white/10 text-[9px] uppercase tracking-[0.2em] text-white/25">
              <th className="px-5 py-3.5 font-medium">Usuario</th>
              <th className="py-3.5 pr-4 font-medium">Rol</th>
              <th className="py-3.5 pr-4 font-medium">Estado</th>
              <th className="py-3.5 pr-4 font-medium">País</th>
              <th className="py-3.5 pr-4 font-medium">Nacimiento</th>
              <th className="py-3.5 pr-4 font-medium">Registro</th>
              <th className="py-3.5 pr-5 text-right font-medium">Acciones</th>
            </tr>
          </thead>
          <tbody>
            {usuarios.map((usuario) => {
              const esPropio = usuario.id === usuarioActualId
              return (
                <tr key={usuario.id} className="border-b border-white/5 transition last:border-0 hover:bg-white/[0.03]">
                  <td className="px-5 py-3.5">
                    <div className="flex items-center gap-3">
                      <Avatar name={usuario.nombre} src={usuario.fotoPerfil} size="sm" />
                      <div className="min-w-0">
                        <p className="flex items-center gap-2 truncate font-medium">
                          {usuario.nombre}
                          {esPropio && (
                            <span className="rounded-full border border-white/15 px-1.5 py-0.5 text-[8px] uppercase tracking-[0.12em] text-white/40">
                              Tú
                            </span>
                          )}
                        </p>
                        <p className="max-w-[260px] truncate text-[11px] text-white/30">{usuario.email}</p>
                      </div>
                    </div>
                  </td>
                  <td className="py-3.5 pr-4">
                    <Badge variant={esAdmin(usuario.tipo) ? 'purple' : 'default'} size="sm">
                      {esAdmin(usuario.tipo) ? 'Admin' : 'Usuario'}
                    </Badge>
                  </td>
                  <td className="py-3.5 pr-4">
                    <Badge variant={usuario.estaActivo ? 'success' : 'danger'} size="sm">
                      {usuario.estaActivo ? 'Activo' : 'Inactivo'}
                    </Badge>
                  </td>
                  <td className="py-3.5 pr-4 text-white/45">{usuario.pais || '—'}</td>
                  <td className="py-3.5 pr-4 text-[11px] text-white/35">{formatDate(usuario.fechaNacimiento)}</td>
                  <td className="py-3.5 pr-4 text-[11px] text-white/35">{formatDate(usuario.fechaRegistro)}</td>
                  <td className="py-3.5 pr-5">
                    <Acciones
                      usuario={usuario}
                      esPropio={esPropio}
                      onEditar={onEditar}
                      onAlternarEstado={onAlternarEstado}
                      onEliminar={onEliminar}
                    />
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
      </div>
    </div>
  )
}

export const UsuariosCardsMobile = ({
  usuarios = [],
  usuarioActualId,
  onEditar,
  onAlternarEstado,
  onEliminar,
}) => {
  return (
    <div className="space-y-3 md:hidden">
      {usuarios.map((usuario) => {
        const esPropio = usuario.id === usuarioActualId
        return (
          <div key={usuario.id} className="rounded-3xl border border-white/10 bg-white/[0.03] p-4">
            <div className="flex items-start gap-3">
              <Avatar name={usuario.nombre} src={usuario.fotoPerfil} size="md" />
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm font-medium">{usuario.nombre}</p>
                <p className="truncate text-[11px] text-white/35">{usuario.email}</p>
                <div className="mt-2 flex flex-wrap gap-1.5">
                  <Badge variant={esAdmin(usuario.tipo) ? 'purple' : 'default'} size="sm">
                    {esAdmin(usuario.tipo) ? 'Admin' : 'Usuario'}
                  </Badge>
                  <Badge variant={usuario.estaActivo ? 'success' : 'danger'} size="sm">
                    {usuario.estaActivo ? 'Activo' : 'Inactivo'}
                  </Badge>
                  {esPropio && (
                    <Badge variant="info" size="sm">
                      Tú
                    </Badge>
                  )}
                </div>
              </div>
            </div>
            <div className="mt-3 flex items-center justify-between border-t border-white/5 pt-3">
              <span className="text-[10px] uppercase tracking-[0.15em] text-white/25">
                {usuario.pais || 'Sin país'} · {formatDate(usuario.fechaRegistro)}
              </span>
              <Acciones
                usuario={usuario}
                esPropio={esPropio}
                onEditar={onEditar}
                onAlternarEstado={onAlternarEstado}
                onEliminar={onEliminar}
              />
            </div>
          </div>
        )
      })}
    </div>
  )
}

export default UsuariosTable
