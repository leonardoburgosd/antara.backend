import { formatNumber, formatDuration } from '../../../utils/formatters'

const MetricaPista = ({ label, value, hint }) => (
  <div className="rounded-[1.5rem] border border-white/10 bg-white/[0.03] p-4">
    <p className="text-[10px] uppercase tracking-[0.2em] text-white/35">{label}</p>
    <p className="mt-2 text-2xl font-semibold tracking-tight">{value}</p>
    {hint && <p className="mt-1 text-xs text-white/35">{hint}</p>}
  </div>
)

export const PistasMetrics = ({ estadisticas }) => {
  return (
    <div className="grid grid-cols-2 gap-3 lg:grid-cols-4">
      <MetricaPista
        label="Canciones"
        value={formatNumber(estadisticas.total)}
        hint="En la biblioteca"
      />
      <MetricaPista
        label="Con letra"
        value={formatNumber(estadisticas.conLetra)}
        hint="Letra disponible"
      />
      <MetricaPista
        label="Álbumes"
        value={formatNumber(estadisticas.albumes)}
        hint="Distintos"
      />
      <MetricaPista
        label="Duración total"
        value={formatDuration(estadisticas.duracion)}
        hint={`${formatNumber(estadisticas.reproducciones)} reproducciones`}
      />
    </div>
  )
}

export default PistasMetrics

