import { useState } from 'react'
import { useAlbumes } from '../hooks/useAlbumes'
import { usePaginacion } from '../../../hooks/usePaginacion'
import { formatNumber } from '../../../utils/formatters'

import AlbumesTable, { AlbumesCardsMobile } from '../components/AlbumesTable'
import AlbumFormModal from '../components/AlbumFormModal'

import { Input } from '../../../components/ui/Input'
import { Button } from '../../../components/ui/Button'
import { EmptyState } from '../../../components/ui/EmptyState'
import { ErrorBanner } from '../../../components/ui/ErrorBanner'
import { SkeletonTable } from '../../../components/ui/Skeleton'
import { ConfirmDialog } from '../../../components/ui/ConfirmDialog'
import { Pagination } from '../../../components/ui/Pagination'
import { IconDisc, IconPlus, IconRefresh, IconSearch, IconClose } from '../../../components/ui/Icons'

const FILTROS = [
  { id: 'todos', label: 'Todos' },
  { id: 'publicados', label: 'Publicados' },
  { id: 'borradores', label: 'Borradores' },
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

export const AlbumesPage = ({ onToast = () => {} }) => {
  const {
    albumes,
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
    alternarPublicacion,
    eliminar,
  } = useAlbumes()

  const paginacion = usePaginacion(filtrados, { resetDeps: [busqueda, filtro] })
  const [formulario, setFormulario] = useState({ abierto: false, album: null })
  const [porEliminar, setPorEliminar] = useState(null)
  const [eliminando, setEliminando] = useState(false)

  const guardar = async (payload) => {
    if (formulario.album) {
      await actualizar(formulario.album.id, payload)
      onToast('Álbum actualizado correctamente.')
    } else {
      await crear(payload)
      onToast('Álbum creado correctamente.')
    }
    setFormulario({ abierto: false, album: null })
  }

  const cambiarPublicacion = async (album) => {
    try {
      const actualizado = await alternarPublicacion(album)
      onToast(actualizado?.estaPublicado ? 'Álbum publicado.' : 'Álbum movido a borradores.')
    } catch (err) {
      onToast(err.message || 'No se pudo cambiar la publicación del álbum.')
    }
  }

  const confirmarEliminar = async () => {
    if (!porEliminar) return
    setEliminando(true)
    try {
      await eliminar(porEliminar.id)
      onToast('Álbum eliminado correctamente.')
      setPorEliminar(null)
    } catch (err) {
      onToast(err.message || 'No se pudo eliminar el álbum.')
    } finally {
      setEliminando(false)
    }
  }

  return (
    <section className="space-y-5">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <p className="text-[10px] uppercase tracking-[0.35em] text-white/35">Catálogo</p>
          <h2 className="mt-2 text-2xl font-semibold tracking-tight sm:text-3xl">Álbumes</h2>
          <p className="mt-2 max-w-2xl text-sm leading-6 text-white/40">
            Agrupa las pistas de la biblioteca, edita sus portadas y controla qué álbumes están
            publicados.
          </p>
        </div>
        <div className="flex flex-wrap gap-2">
          <Button variant="outline" icon={IconRefresh} onClick={cargar} disabled={loading}>
            Actualizar
          </Button>
          <Button icon={IconPlus} onClick={() => setFormulario({ abierto: true, album: null })}>
            Nuevo álbum
          </Button>
        </div>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <MetricCard label="Álbumes" value={estadisticas.total} hint="En el catálogo" />
        <MetricCard label="Publicados" value={estadisticas.publicados} hint="Visibles para clientes" />
        <MetricCard label="Borradores" value={estadisticas.borradores} hint="Sin publicar" />
        <MetricCard label="Inactivos" value={estadisticas.inactivos} hint="Ocultos del catálogo" />
      </div>

      <div className="flex flex-col gap-3 rounded-3xl border border-white/10 bg-white/[0.03] p-4 lg:flex-row lg:items-center">
        <Input
          icon={IconSearch}
          value={busqueda}
          onChange={(evento) => setBusqueda(evento.target.value)}
          onClear={() => setBusqueda('')}
          placeholder="Buscar por nombre, artista o descripción"
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
      ) : albumes.length === 0 ? (
        <div className="rounded-[1.75rem] border border-white/10 bg-white/[0.03]">
          <EmptyState
            icon={IconDisc}
            title="Aún no hay álbumes"
            description="Crea el primer álbum para organizar las pistas de tu biblioteca."
            action={
              <Button size="sm" icon={IconPlus} onClick={() => setFormulario({ abierto: true, album: null })}>
                Crear álbum
              </Button>
            }
          />
        </div>
      ) : filtrados.length === 0 ? (
        <div className="rounded-[1.75rem] border border-white/10 bg-white/[0.03]">
          <EmptyState
            icon={IconSearch}
            title="Sin resultados"
            description="No hay álbumes que coincidan con la búsqueda o el filtro seleccionado."
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
          <AlbumesTable
            albumes={paginacion.visibles}
            onEditar={(album) => setFormulario({ abierto: true, album })}
            onAlternarPublicacion={cambiarPublicacion}
            onEliminar={setPorEliminar}
          />
          <AlbumesCardsMobile
            albumes={paginacion.visibles}
            onEditar={(album) => setFormulario({ abierto: true, album })}
            onAlternarPublicacion={cambiarPublicacion}
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

      <AlbumFormModal
        isOpen={formulario.abierto}
        album={formulario.album}
        onClose={() => setFormulario({ abierto: false, album: null })}
        onSubmit={guardar}
      />

      <ConfirmDialog
        isOpen={Boolean(porEliminar)}
        onClose={() => setPorEliminar(null)}
        onConfirm={confirmarEliminar}
        isLoading={eliminando}
        title="Eliminar álbum"
        message={
          porEliminar
            ? `Se eliminará el álbum «${porEliminar.nombre}». Si tiene pistas asociadas, deberás desvincularlas antes.`
            : ''
        }
        confirmLabel="Eliminar"
      />
    </section>
  )
}

export default AlbumesPage
