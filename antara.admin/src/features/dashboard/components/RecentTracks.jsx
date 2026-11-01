import { formatNumber, formatDate } from '../../../utils/formatters'
import { IconMusic } from '../../../components/ui/Icons'
import { EmptyState } from '../../../components/ui/EmptyState'

export const RecentTracks = ({
  recentTracks = [],
  loading = false,
  onViewAll,
}) => {
  return (
    <section className="rounded-[2rem] border border-white/10 bg-white/[0.03] p-6 sm:p-7">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h3 className="text-lg font-semibold">Pistas recientes</h3>
          <p className="mt-1 text-xs text-white/35">Últimas incorporaciones al catálogo.</p>
        </div>
        {onViewAll && (
          <button
            type="button"
            onClick={onViewAll}
            className="text-xs text-white/35 underline-offset-4 transition hover:text-white hover:underline"
          >
            Ver todas
          </button>
        )}
      </div>
      <div className="mt-5 overflow-x-auto">
        {loading ? (
          <div className="space-y-3">
            {Array.from({ length: 5 }).map((_, i) => (
              <div key={i} className="h-12 animate-pulse rounded-xl bg-white/[0.04]" />
            ))}
          </div>
        ) : recentTracks.length === 0 ? (
          <EmptyState
            icon={IconMusic}
            title="Aún no hay pistas"
            description="Sube tu primera pista para verla reflejada aquí."
          />
        ) : (
          <table className="w-full min-w-[560px] text-left text-sm">
            <thead>
              <tr className="border-b border-white/10 text-[9px] uppercase tracking-[0.2em] text-white/25">
                <th className="pb-3 font-medium">Pista</th>
                <th className="pb-3 font-medium">Intérprete</th>
                <th className="pb-3 font-medium">Reproducciones</th>
                <th className="pb-3 text-right font-medium">Fecha</th>
              </tr>
            </thead>
            <tbody>
              {recentTracks.map((track) => (
                <tr
                  key={track.id}
                  className="border-b border-white/5 transition last:border-0 hover:bg-white/[0.02]"
                >
                  <td className="py-4 pr-4">
                    <div className="flex items-center gap-3">
                      <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-xl border border-white/10 bg-white/[0.04]">
                        <IconMusic className="h-4 w-4 text-white/45" />
                      </div>
                      <div className="min-w-0">
                        <p className="truncate font-medium">{track.nombre}</p>
                        <p className="truncate text-[11px] text-white/30">
                          {track.albumNombre || 'Sin álbum'}
                        </p>
                      </div>
                    </div>
                  </td>
                  <td className="py-4 pr-4 text-white/45">{track.interprete || '—'}</td>
                  <td className="py-4 pr-4 text-white/45">{formatNumber(track.reproducciones)}</td>
                  <td className="py-4 text-right text-[11px] text-white/30">
                    {formatDate(track.fechaRegistro)}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </section>
  )
}

export default RecentTracks

