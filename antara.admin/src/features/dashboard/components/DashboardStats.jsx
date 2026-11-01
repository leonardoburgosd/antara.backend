import { useMemo } from 'react'
import { formatNumber } from '../../../utils/formatters'
import { IconUsers, IconDisc, IconMusic, IconList } from '../../../components/ui/Icons'

export const DashboardStats = ({ dashboard, loading }) => {
  const stats = useMemo(
    () => [
      {
        label: 'Usuarios',
        tag: 'Comunidad',
        value: dashboard?.usuariosTotales,
        hint: `${formatNumber(dashboard?.usuariosActivos)} activos`,
        icon: IconUsers,
      },
      {
        label: 'Álbumes',
        tag: 'Catálogo',
        value: dashboard?.albumesTotales,
        hint: `${formatNumber(dashboard?.albumesRecientes?.length)} recientes`,
        icon: IconDisc,
      },
      {
        label: 'Pistas',
        tag: 'Música',
        value: dashboard?.pistasTotales,
        hint: `${formatNumber(dashboard?.reproduccionesTotales)} reproducciones`,
        icon: IconMusic,
      },
      {
        label: 'Playlists',
        tag: 'Curaduría',
        value: dashboard?.playlistsTotales,
        hint: 'Colecciones activas',
        icon: IconList,
      },
    ],
    [dashboard],
  )

  return (
    <section className="mt-5 grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
      {stats.map((stat) => (
        <article
          key={stat.label}
          className="group relative overflow-hidden rounded-[1.75rem] border border-white/10 bg-white/[0.03] p-6 transition duration-300 hover:-translate-y-1 hover:border-white/25 hover:bg-white/[0.05]"
        >
          <div className="flex items-start justify-between">
            <div className="flex h-11 w-11 items-center justify-center rounded-2xl border border-white/10 bg-white/[0.04]">
              <stat.icon className="h-5 w-5 text-white/70" />
            </div>
            <span className="rounded-full border border-white/10 px-3 py-1 text-[9px] uppercase tracking-[0.2em] text-white/30">
              {stat.tag}
            </span>
          </div>
          <p className="mt-6 text-xs uppercase tracking-[0.2em] text-white/35">{stat.label}</p>
          {loading ? (
            <div className="mt-2 h-10 w-24 animate-pulse rounded-lg bg-white/10" />
          ) : (
            <p className="mt-1 text-4xl font-semibold tracking-tight">{formatNumber(stat.value)}</p>
          )}
          <p className="mt-3 text-xs text-white/35">{stat.hint}</p>
          <div className="mt-5 h-px w-full bg-gradient-to-r from-white/20 via-white/5 to-transparent" />
        </article>
      ))}
    </section>
  )
}

export default DashboardStats

