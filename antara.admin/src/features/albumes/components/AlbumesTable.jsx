import { Badge } from '../../../components/ui/Badge'
import { formatDate } from '../../../utils/formatters'
import { IconDisc, IconEdit, IconPower, IconTrash } from '../../../components/ui/Icons'

const Portada = ({ album, size = 'md' }) => {
  const clases = size === 'lg' ? 'h-12 w-12 rounded-2xl' : 'h-10 w-10 rounded-xl'
  return (
    <span
      className={`flex ${clases} shrink-0 items-center justify-center overflow-hidden border border-white/10 bg-white/[0.04]`}
    >
      {album.portadaUrl ? (
        <img src={album.portadaUrl} alt="" className="h-full w-full object-cover" loading="lazy" />
      ) : (
        <IconDisc className="h-4 w-4 text-white/30" />
      )}
    </span>
  )
}

const Acciones = ({ album, onEditar, onAlternarPublicacion, onEliminar }) => (
  <div className="flex items-center justify-end gap-1.5">
    <button
      type="button"
      onClick={() => onEditar(album)}
      className="flex h-8 w-8 items-center justify-center rounded-full border border-white/10 text-white/50 transition hover:border-white/30 hover:text-white"
      title="Editar álbum"
      aria-label={`Editar ${album.nombre}`}
    >
      <IconEdit className="h-3.5 w-3.5" />
    </button>
    <button
      type="button"
      onClick={() => onAlternarPublicacion(album)}
      className="flex h-8 w-8 items-center justify-center rounded-full border border-white/10 text-white/50 transition hover:border-white/30 hover:text-white"
      title={album.estaPublicado ? 'Volver a borrador' : 'Publicar álbum'}
      aria-label={album.estaPublicado ? 'Despublicar álbum' : 'Publicar álbum'}
    >
      <IconPower className="h-3.5 w-3.5" />
    </button>
    <button
      type="button"
      onClick={() => onEliminar(album)}
      className="flex h-8 w-8 items-center justify-center rounded-full border border-red-500/20 text-red-300/80 transition hover:border-red-500/50 hover:bg-red-500/10"
      title="Eliminar álbum"
      aria-label={`Eliminar ${album.nombre}`}
    >
      <IconTrash className="h-3.5 w-3.5" />
    </button>
  </div>
)

export const AlbumesTable = ({ albumes = [], onEditar, onAlternarPublicacion, onEliminar }) => (
  <div className="hidden overflow-hidden rounded-[1.75rem] border border-white/10 bg-white/[0.03] md:block">
    <div className="overflow-x-auto">
      <table className="w-full min-w-[860px] text-left text-sm">
        <thead>
          <tr className="border-b border-white/10 text-[9px] uppercase tracking-[0.2em] text-white/25">
            <th className="px-5 py-3.5 font-medium">Álbum</th>
            <th className="py-3.5 pr-4 font-medium">Artista</th>
            <th className="py-3.5 pr-4 font-medium">Estado</th>
            <th className="py-3.5 pr-4 font-medium">Publicación</th>
            <th className="py-3.5 pr-4 font-medium">Visibilidad</th>
            <th className="py-3.5 pr-5 text-right font-medium">Acciones</th>
          </tr>
        </thead>
        <tbody>
          {albumes.map((album) => (
            <tr key={album.id} className="border-b border-white/5 transition last:border-0 hover:bg-white/[0.03]">
              <td className="px-5 py-3.5">
                <div className="flex items-center gap-3">
                  <Portada album={album} />
                  <div className="min-w-0">
                    <p className="max-w-[240px] truncate font-medium">{album.nombre}</p>
                    <p className="max-w-[240px] truncate text-[11px] text-white/30">
                      {album.descripcion || 'Sin descripción'}
                    </p>
                  </div>
                </div>
              </td>
              <td className="py-3.5 pr-4 text-white/50">{album.artista || '—'}</td>
              <td className="py-3.5 pr-4">
                <Badge variant={album.estaActivo ? 'success' : 'danger'} size="sm">
                  {album.estaActivo ? 'Activo' : 'Inactivo'}
                </Badge>
              </td>
              <td className="py-3.5 pr-4 text-[11px] text-white/35">
                {formatDate(album.fechaPublicacion)}
              </td>
              <td className="py-3.5 pr-4">
                <Badge variant={album.estaPublicado ? 'info' : 'warning'} size="sm">
                  {album.estaPublicado ? 'Publicado' : 'Borrador'}
                </Badge>
              </td>
              <td className="py-3.5 pr-5">
                <Acciones
                  album={album}
                  onEditar={onEditar}
                  onAlternarPublicacion={onAlternarPublicacion}
                  onEliminar={onEliminar}
                />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  </div>
)

export const AlbumesCardsMobile = ({ albumes = [], onEditar, onAlternarPublicacion, onEliminar }) => (
  <div className="space-y-3 md:hidden">
    {albumes.map((album) => (
      <div key={album.id} className="rounded-3xl border border-white/10 bg-white/[0.03] p-4">
        <div className="flex items-start gap-3">
          <Portada album={album} size="lg" />
          <div className="min-w-0 flex-1">
            <p className="truncate text-sm font-medium">{album.nombre}</p>
            <p className="truncate text-[11px] text-white/35">{album.artista || 'Sin artista'}</p>
            <div className="mt-2 flex flex-wrap gap-1.5">
              <Badge variant={album.estaPublicado ? 'info' : 'warning'} size="sm">
                {album.estaPublicado ? 'Publicado' : 'Borrador'}
              </Badge>
              <Badge variant={album.estaActivo ? 'success' : 'danger'} size="sm">
                {album.estaActivo ? 'Activo' : 'Inactivo'}
              </Badge>
            </div>
          </div>
        </div>
        <div className="mt-3 flex items-center justify-between border-t border-white/5 pt-3">
          <span className="text-[10px] uppercase tracking-[0.15em] text-white/25">
            {formatDate(album.fechaPublicacion)}
          </span>
          <Acciones
            album={album}
            onEditar={onEditar}
            onAlternarPublicacion={onAlternarPublicacion}
            onEliminar={onEliminar}
          />
        </div>
      </div>
    ))}
  </div>
)

export default AlbumesTable
