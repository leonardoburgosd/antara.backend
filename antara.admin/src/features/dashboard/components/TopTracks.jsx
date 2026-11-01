import { formatNumber } from '../../../utils/formatters'
import { IconTrend } from '../../../components/ui/Icons'
import { EmptyState } from '../../../components/ui/EmptyState'

export const TopTracks = ({ topTracks = [], loading = false }) => {
  return (
    <section className="rounded-[2rem] border border-white/10 bg-white/[0.03] p-6 sm:p-7">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h3 className="text-lg font-semibold">Pistas más reproducidas</h3>
          <p className="mt-1 text-xs text-white/35">El catálogo que más está sonando.</p>
        </div>
        <span className="rounded-full border border-white/10 px-3 py-1 text-[9px] uppercase tracking-[0.2em] text-white/30">
          Top 5
        </span>
      </div>
      <div className="mt-6">
        {loading ? (
          <div className="space-y-3">
            {Array.from({ length: 5 }).map((_, i) => (
              <div key={i} className="h-10 animate-pulse rounded-xl bg-white/[0.04]" />
            ))}
          </div>
        ) : topTracks.length === 0 ? (
          <EmptyState
            icon={IconTrend}
            title="Aún no hay reproducciones"
            description="Cuando las pistas empiecen a sonar, verás aquí el ranking."
          />
        ) : (
          <div className="space-y-4">
            {topTracks.map((track, index) => {
              const max = Math.max(
                ...topTracks.map((item) => Number(item.reproducciones) || 0),
                1,
              )
              const width = Math.max(6, ((Number(track.reproducciones) || 0) / max) * 100)

              return (
                <div key={track.id}>
                  <div className="flex items-center justify-between gap-4">
                    <div className="flex min-w-0 items-center gap-3">
                      <span className="w-5 shrink-0 text-[10px] text-white/25">
                        {String(index + 1).padStart(2, '0')}
                      </span>
                      <div className="min-w-0">
                        <p className="truncate text-sm font-medium">{track.nombre}</p>
                        <p className="truncate text-[11px] text-white/35">
                          {track.interprete || 'Artista desconocido'} · {track.albumNombre || 'Sin álbum'}
                        </p>
                      </div>
                    </div>
                    <span className="shrink-0 text-xs text-white/40">
                      {formatNumber(track.reproducciones)}
                    </span>
                  </div>
                  <div className="mt-2 h-1.5 overflow-hidden rounded-full bg-white/[0.06]">
                    <div
                      className="h-full rounded-full bg-white/40 transition-all duration-700"
                      style={{ width: `${width}%` }}
                    />
                  </div>
                </div>
              )
            })}
          </div>
        )}
      </div>
    </section>
  )
}

export default TopTracks

