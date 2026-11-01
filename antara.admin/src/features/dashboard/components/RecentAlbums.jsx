import { formatDate } from '../../../utils/formatters'
import { IconDisc } from '../../../components/ui/Icons'
import { EmptyState } from '../../../components/ui/EmptyState'

export const RecentAlbums = ({
  recentAlbums = [],
  loading = false,
  onViewAll,
}) => {
  return (
    <section className="mt-5">
      <div className="flex items-end justify-between gap-4">
        <div>
          <h3 className="text-lg font-semibold">Álbumes recientes</h3>
          <p className="mt-1 text-xs text-white/35">Lo último que se sumó a la biblioteca.</p>
        </div>
        {onViewAll && (
          <button
            type="button"
            onClick={onViewAll}
            className="text-xs text-white/35 underline-offset-4 transition hover:text-white hover:underline"
          >
            Ver todos
          </button>
        )}
      </div>
      <div className="mt-4 grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {loading ? (
          Array.from({ length: 4 }).map((_, index) => (
            <div
              key={index}
              className="overflow-hidden rounded-[1.5rem] border border-white/10 bg-white/[0.03]"
            >
              <div className="aspect-square animate-pulse bg-white/[0.05]" />
              <div className="space-y-2 p-4">
                <div className="h-3 w-2/3 animate-pulse rounded bg-white/10" />
                <div className="h-2 w-1/3 animate-pulse rounded bg-white/[0.07]" />
              </div>
            </div>
          ))
        ) : recentAlbums.length === 0 ? (
          <div className="col-span-full">
            <EmptyState
              icon={IconDisc}
              title="Aún no hay álbumes"
              description="Crea un álbum para empezar a organizar el catálogo."
            />
          </div>
        ) : (
          recentAlbums.map((album) => (
            <article
              key={album.id}
              className="group overflow-hidden rounded-[1.5rem] border border-white/10 bg-white/[0.03] transition duration-300 hover:-translate-y-1 hover:border-white/25"
            >
              <div className="relative aspect-square overflow-hidden bg-gradient-to-br from-white/[0.12] via-white/[0.04] to-transparent">
                {album.portadaUrl ? (
                  <img
                    src={album.portadaUrl}
                    alt={album.nombre}
                    className="h-full w-full object-cover transition duration-500 group-hover:scale-105"
                  />
                ) : (
                  <div className="flex h-full items-center justify-center">
                    <IconDisc className="h-10 w-10 text-white/20" />
                  </div>
                )}
                <span className="absolute left-3 top-3 rounded-full border border-white/20 bg-black/60 px-2.5 py-1 text-[8px] uppercase tracking-[0.2em] text-white/70 backdrop-blur">
                  {album.estaPublicado ? 'Publicado' : 'Borrador'}
                </span>
              </div>
              <div className="p-4">
                <p className="truncate text-sm font-medium">{album.nombre}</p>
                <p className="mt-1 text-[10px] text-white/30">{formatDate(album.fechaPublicacion)}</p>
              </div>
            </article>
          ))
        )}
      </div>
    </section>
  )
}

export default RecentAlbums

