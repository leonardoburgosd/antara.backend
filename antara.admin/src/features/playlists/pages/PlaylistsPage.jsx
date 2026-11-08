import { useState } from 'react'
import { usePlaylists } from '../hooks/usePlaylists'
import PlaylistFormModal from '../components/PlaylistFormModal'
import PlaylistDetailModal from '../components/PlaylistDetailModal'
import { formatNumber, getInitials } from '../../../utils/formatters'

import { Input } from '../../../components/ui/Input'
import { Button } from '../../../components/ui/Button'
import { EmptyState } from '../../../components/ui/EmptyState'
import { ErrorBanner } from '../../../components/ui/ErrorBanner'
import { SkeletonTable } from '../../../components/ui/Skeleton'
import { ConfirmDialog } from '../../../components/ui/ConfirmDialog'
import {
  IconClose,
  IconEdit,
  IconMusic,
  IconPlaylist,
  IconPlus,
  IconRefresh,
  IconSearch,
  IconTrash,
} from '../../../components/ui/Icons'

const MetricCard = ({ label, value, hint }) => (
  <div className="rounded-3xl border border-white/10 bg-white/[0.03] p-5">
    <p className="text-[10px] uppercase tracking-[0.25em] text-white/30">{label}</p>
    <p className="mt-3 text-3xl font-semibold tracking-tight">{formatNumber(value)}</p>
    {hint && <p className="mt-1 text-[11px] text-white/30">{hint}</p>}
  </div>
)

const PlaylistCard = ({ playlist, onAbrir, onEditar, onEliminar }) => {
  const artistas = String(playlist.artistas || '')
    .split(',')
    .map((nombre) => nombre.trim())
    .filter(Boolean)

  return (
    <article
      onClick={() => onAbrir(playlist)}
      className="group flex cursor-pointer flex-col rounded-[1.75rem] border border-white/10 bg-white/[0.03] p-5 transition hover:border-white/25 hover:bg-white/[0.05]"
    >
      <div className="flex items-start gap-4">
        <div className="flex h-16 w-16 shrink-0 items-center justify-center overflow-hidden rounded-2xl border border-white/10 bg-white/[0.04]">
          {playlist.portadaUrl ? (
            <img src={playlist.portadaUrl} alt="" loading="lazy" className="h-full w-full object-cover" />
          ) : (
            <span className="text-sm font-semibold text-white/60">
              {getInitials(playlist.nombre, 'PL')}
            </span>
          )}
        </div>

        <div className="min-w-0 flex-1">
          <h3 className="truncate text-base font-medium text-white/90">{playlist.nombre}</h3>
          <p className="mt-1 line-clamp-2 text-xs leading-5 text-white/35">
            {playlist.descripcion || 'Sin descripción.'}
          </p>
        </div>

        <div className="flex shrink-0 items-center gap-1 opacity-80 transition group-hover:opacity-100">
          <button
            type="button"
            onClick={(evento) => {
              evento.stopPropagation()
              onEditar(playlist)
            }}
            className="flex h-8 w-8 items-center justify-center rounded-full text-white/40 transition hover:bg-white/10 hover:text-white"
            title="Editar playlist"
            aria-label={`Editar ${playlist.nombre}`}
          >
            <IconEdit className="h-4 w-4" />
          </button>
          <button
            type="button"
            onClick={(evento) => {
              evento.stopPropagation()
              onEliminar(playlist)
            }}
            className="flex h-8 w-8 items-center justify-center rounded-full text-white/40 transition hover:bg-red-500/15 hover:text-red-200"
            title="Eliminar playlist"
            aria-label={`Eliminar ${playlist.nombre}`}
          >
            <IconTrash className="h-4 w-4" />
          </button>
        </div>
      </div>

      <div className="mt-5 flex flex-wrap items-center gap-2 text-[10px] uppercase tracking-[0.16em] text-white/35">
        <span className="inline-flex items-center gap-1.5 rounded-full border border-white/10 px-2.5 py-1">
          <IconMusic className="h-3 w-3" />
          {formatNumber(playlist.totalPistas)} {playlist.totalPistas === 1 ? 'canción' : 'canciones'}
        </span>
        <span className="rounded-full border border-white/10 px-2.5 py-1">
          {formatNumber(playlist.totalArtistas)} {playlist.totalArtistas === 1 ? 'artista' : 'artistas'}
        </span>
        {playlist.usuarioNombre && (
          <span className="truncate px-1 normal-case tracking-normal text-white/25">
            {playlist.usuarioNombre}
          </span>
        )}
      </div>

      <p className="mt-3 truncate text-[11px] text-white/30">
        {artistas.length > 0 ? artistas.join(' · ') : 'Agrega canciones de cualquier artista'}
      </p>
    </article>
  )
}

export const PlaylistsPage = ({ onToast = () => {} }) => {
  const {
    playlists,
    filtradas,
    biblioteca,
    estadisticas,
    loading,
    error,
    busqueda,
    setBusqueda,
    cargar,
    crear,
    actualizar,
    eliminar,
    obtenerPistas,
    agregarPista,
    quitarPista,
  } = usePlaylists()

  const [formulario, setFormulario] = useState({ abierto: false, playlist: null })
  const [detalle, setDetalle] = useState(null)
  const [canciones, setCanciones] = useState([])
  const [cargandoCanciones, setCargandoCanciones] = useState(false)
  const [porEliminar, setPorEliminar] = useState(null)
  const [eliminando, setEliminando] = useState(false)

  // El resumen se refresca al agregar o quitar canciones; se usa el de la lista.
  const playlistAbierta = detalle
    ? playlists.find((playlist) => playlist.id === detalle.id) || detalle
    : null

  const abrirDetalle = async (playlist) => {
    setDetalle(playlist)
    setCanciones([])
    setCargandoCanciones(true)
    try {
      setCanciones(await obtenerPistas(playlist.id))
    } catch (err) {
      onToast(err.message || 'No se pudieron cargar las canciones de la playlist.')
    } finally {
      setCargandoCanciones(false)
    }
  }

  const cerrarDetalle = () => {
    setDetalle(null)
    setCanciones([])
    setCargandoCanciones(false)
  }

  const guardar = async (datos) => {
    if (formulario.playlist) {
      await actualizar(formulario.playlist.id, datos)
      onToast('Playlist actualizada correctamente.')
    } else {
      const creada = await crear(datos)
      onToast('Playlist creada. Agrégale canciones de distintos artistas.')
      if (creada?.id) {
        setFormulario({ abierto: false, playlist: null })
        await abrirDetalle(creada)
        return
      }
    }
    setFormulario({ abierto: false, playlist: null })
  }

  const agregar = async (pistaId) => {
    if (!playlistAbierta) return
    setCanciones(await agregarPista(playlistAbierta.id, pistaId))
  }

  const quitar = async (pistaId) => {
    if (!playlistAbierta) return
    setCanciones(await quitarPista(playlistAbierta.id, pistaId))
  }

  const confirmarEliminar = async () => {
    if (!porEliminar) return
    setEliminando(true)
    try {
      await eliminar(porEliminar.id)
      if (detalle?.id === porEliminar.id) cerrarDetalle()
      onToast('Playlist eliminada. Las canciones siguen en la biblioteca.')
      setPorEliminar(null)
    } catch (err) {
      onToast(err.message || 'No se pudo eliminar la playlist.')
    } finally {
      setEliminando(false)
    }
  }

  return (
    <section className="space-y-5">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <p className="text-[10px] uppercase tracking-[0.35em] text-white/35">Colecciones</p>
          <h2 className="mt-2 text-2xl font-semibold tracking-tight sm:text-3xl">Playlists</h2>
          <p className="mt-2 max-w-2xl text-sm leading-6 text-white/40">
            Crea listas con canciones de distintos artistas y álbumes, como una selección para
            cualquier momento.
          </p>
        </div>
        <div className="flex flex-wrap gap-2">
          <Button variant="outline" icon={IconRefresh} onClick={cargar} disabled={loading}>
            Actualizar
          </Button>
          <Button icon={IconPlus} onClick={() => setFormulario({ abierto: true, playlist: null })}>
            Nueva playlist
          </Button>
        </div>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <MetricCard label="Playlists" value={estadisticas.total} hint="Creadas en el catálogo" />
        <MetricCard label="Canciones" value={estadisticas.canciones} hint="Asignadas a playlists" />
        <MetricCard label="Artistas" value={estadisticas.artistas} hint="Distintos en todas las listas" />
        <MetricCard label="Vacías" value={estadisticas.vacias} hint="Sin canciones todavía" />
      </div>

      <div className="flex flex-col gap-3 rounded-3xl border border-white/10 bg-white/[0.03] p-4 sm:flex-row sm:items-center">
        <Input
          icon={IconSearch}
          value={busqueda}
          onChange={(evento) => setBusqueda(evento.target.value)}
          onClear={() => setBusqueda('')}
          placeholder="Buscar por nombre, descripción o artista"
          containerClassName="w-full sm:max-w-md"
        />
        <p className="text-xs text-white/30 sm:ml-auto">
          {formatNumber(filtradas.length)} de {formatNumber(playlists.length)} playlists
        </p>
      </div>

      {error && <ErrorBanner message={error} onRetry={cargar} />}

      {loading ? (
        <div className="rounded-[1.75rem] border border-white/10 bg-white/[0.03] p-6">
          <SkeletonTable rows={4} cols={4} />
        </div>
      ) : playlists.length === 0 ? (
        <div className="rounded-[1.75rem] border border-white/10 bg-white/[0.03]">
          <EmptyState
            icon={IconPlaylist}
            title="Aún no hay playlists"
            description="Crea la primera lista y agrégale canciones de cualquier artista."
            action={
              <Button
                size="sm"
                icon={IconPlus}
                onClick={() => setFormulario({ abierto: true, playlist: null })}
              >
                Crear playlist
              </Button>
            }
          />
        </div>
      ) : filtradas.length === 0 ? (
        <div className="rounded-[1.75rem] border border-white/10 bg-white/[0.03]">
          <EmptyState
            icon={IconSearch}
            title="Sin resultados"
            description="No hay playlists que coincidan con la búsqueda."
            action={
              <Button size="sm" variant="outline" icon={IconClose} onClick={() => setBusqueda('')}>
                Limpiar búsqueda
              </Button>
            }
          />
        </div>
      ) : (
        <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
          {filtradas.map((playlist) => (
            <PlaylistCard
              key={playlist.id}
              playlist={playlist}
              onAbrir={abrirDetalle}
              onEditar={(item) => setFormulario({ abierto: true, playlist: item })}
              onEliminar={setPorEliminar}
            />
          ))}
        </div>
      )}

      <PlaylistFormModal
        isOpen={formulario.abierto}
        playlist={formulario.playlist}
        onClose={() => setFormulario({ abierto: false, playlist: null })}
        onSubmit={guardar}
      />

      <PlaylistDetailModal
        isOpen={Boolean(detalle)}
        playlist={playlistAbierta}
        canciones={canciones}
        biblioteca={biblioteca}
        cargando={cargandoCanciones}
        onClose={cerrarDetalle}
        onAgregar={agregar}
        onQuitar={quitar}
      />

      <ConfirmDialog
        isOpen={Boolean(porEliminar)}
        onClose={() => setPorEliminar(null)}
        onConfirm={confirmarEliminar}
        isLoading={eliminando}
        title="Eliminar playlist"
        message={
          porEliminar
            ? `Se eliminará la playlist «${porEliminar.nombre}». Las canciones seguirán disponibles en la biblioteca.`
            : ''
        }
        confirmLabel="Eliminar"
      />
    </section>
  )
}

export default PlaylistsPage
