import { formatNumber, relativeTime } from '../../../utils/formatters'
import { IconRefresh, IconDatabase } from '../../../components/ui/Icons'

const StatusRow = ({ label, value, dot }) => {
  return (
    <div className="flex items-center justify-between gap-4 rounded-xl px-1 py-1.5">
      <span className="text-xs text-white/35">{label}</span>
      <span className="flex items-center gap-2 text-xs text-white/60">
        {dot && <span className={`h-1.5 w-1.5 rounded-full ${dot}`} />}
        {value}
      </span>
    </div>
  )
}

export const SystemStatus = ({ health, dashboard, loading, onRefresh }) => {
  return (
    <section className="rounded-[2rem] border border-white/10 bg-white/[0.03] p-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h3 className="text-lg font-semibold">Estado del sistema</h3>
          <p className="mt-1 text-xs text-white/35">Conexión con la API de Antara.</p>
        </div>
        <button
          type="button"
          onClick={onRefresh}
          className="flex h-9 w-9 items-center justify-center rounded-full border border-white/10 text-white/35 transition hover:border-white/25 hover:text-white"
          title="Actualizar"
          aria-label="Actualizar"
        >
          <IconRefresh className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
        </button>
      </div>
      <div className="mt-5 space-y-2.5">
        <StatusRow
          label="Estado"
          value={health?.activo ? 'Operativo' : 'Sin conexión'}
          dot={health?.activo ? 'bg-emerald-400' : 'bg-red-400'}
        />
        <StatusRow label="Versión" value={health?.version || '—'} />
        <StatusRow
          label="Administradores"
          value={health?.adminsRegistrados ? 'Registrados' : 'Pendientes'}
        />
        <StatusRow label="Usuarios activos" value={formatNumber(dashboard?.usuariosActivos)} />
        <StatusRow
          label="Última actualización"
          value={relativeTime(health?.timestamp || dashboard?.timestamp)}
        />
      </div>
      <div className="mt-5 flex items-center gap-3 rounded-2xl border border-white/10 bg-black/30 p-4">
        <div className="flex h-9 w-9 items-center justify-center rounded-xl border border-white/10 bg-white/[0.04]">
          <IconDatabase className="h-4 w-4 text-white/45" />
        </div>
        <div>
          <p className="text-[10px] uppercase tracking-[0.2em] text-white/25">Base de datos</p>
          <p className="mt-0.5 text-sm text-white/60">PostgreSQL · conectada</p>
        </div>
      </div>
    </section>
  )
}

export default SystemStatus

