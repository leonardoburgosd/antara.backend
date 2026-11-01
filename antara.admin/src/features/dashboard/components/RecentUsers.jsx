import { formatNumber, relativeTime } from '../../../utils/formatters'
import { IconUsers } from '../../../components/ui/Icons'
import { EmptyState } from '../../../components/ui/EmptyState'
import { Avatar } from '../../../components/ui/Avatar'

export const RecentUsers = ({ recentUsers = [], total = 0, loading = false }) => {
  return (
    <section className="rounded-[2rem] border border-white/10 bg-white/[0.03] p-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h3 className="text-lg font-semibold">Usuarios recientes</h3>
          <p className="mt-1 text-xs text-white/35">Últimos registros en la plataforma.</p>
        </div>
        <span className="rounded-full border border-white/10 px-3 py-1 text-[9px] uppercase tracking-[0.2em] text-white/30">
          {formatNumber(total)} total
        </span>
      </div>
      <div className="mt-4 space-y-1">
        {loading ? (
          <div className="space-y-2.5">
            {Array.from({ length: 4 }).map((_, i) => (
              <div key={i} className="flex items-center gap-3 rounded-2xl border border-white/5 bg-white/[0.02] p-3">
                <div className="h-9 w-9 animate-pulse rounded-full bg-white/[0.06]" />
                <div className="flex-1 space-y-2">
                  <div className="h-2.5 w-2/5 animate-pulse rounded bg-white/[0.08]" />
                  <div className="h-2 w-1/4 animate-pulse rounded bg-white/[0.05]" />
                </div>
              </div>
            ))}
          </div>
        ) : recentUsers.length === 0 ? (
          <EmptyState
            icon={IconUsers}
            title="Aún no hay usuarios"
            description="Los nuevos registros aparecerán en esta lista."
          />
        ) : (
          recentUsers.map((item) => (
            <div
              key={item.id}
              className="flex items-center gap-3 rounded-2xl border border-transparent px-2 py-2.5 transition hover:border-white/10 hover:bg-white/[0.02]"
            >
              <Avatar name={item.nombre} size="sm" />
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm font-medium">{item.nombre}</p>
                <p className="truncate text-[11px] text-white/30">{item.email}</p>
              </div>
              <div className="shrink-0 text-right">
                <span className="rounded-full border border-white/10 px-2 py-0.5 text-[8px] uppercase tracking-[0.15em] text-white/35">
                  {item.tipo || 'user'}
                </span>
                <p className="mt-1 text-[10px] text-white/25">{relativeTime(item.fechaRegistro)}</p>
              </div>
            </div>
          ))
        )}
      </div>
    </section>
  )
}

export default RecentUsers

