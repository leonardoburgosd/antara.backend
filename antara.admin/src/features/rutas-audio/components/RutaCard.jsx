import { formatDate } from '../../../utils/formatters'
import {
  IconFolder,
  IconPulse,
  IconPencil,
  IconPower,
  IconStar,
  IconTrash,
} from '../../../components/ui/Icons'

const PathRow = ({ label, value, destacado = false }) => {
  return (
    <div className="flex items-start justify-between gap-4">
      <span className="shrink-0 text-[10px] uppercase tracking-[0.15em] text-white/30">{label}</span>
      <span
        className={`truncate font-mono text-[11px] ${destacado ? 'text-white/70' : 'text-white/45'}`}
        title={value || ''}
      >
        {value || '—'}
      </span>
    </div>
  )
}

const rutaVisible = (ruta) => ruta.rutaEfectiva || ruta.rutaContenedor || ruta.rutaFisica || '—'

export const RutaCard = ({
  ruta,
  procesando = false,
  probando = false,
  onProbar,
  onEditar,
  onAlternarActiva,
  onMarcarPredeterminada,
  onEliminar,
}) => {
  return (
    <article className="flex flex-col rounded-[1.75rem] border border-white/10 bg-white/[0.03] p-5 transition hover:border-white/20">
      <div className="flex items-start justify-between gap-3">
        <div className="flex min-w-0 items-center gap-3">
          <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-2xl border border-white/10 bg-white/[0.04]">
            <IconFolder className="h-5 w-5 text-white/60" />
          </div>
          <div className="min-w-0">
            <p className="truncate text-sm font-semibold">{ruta.nombre}</p>
            <p className="truncate text-[10px] uppercase tracking-[0.2em] text-white/30">
              Registrada el {formatDate(ruta.fechaRegistro)}
            </p>
          </div>
        </div>
        <div className="flex shrink-0 flex-col items-end gap-1.5">
          <span
            className={`rounded-full border px-2.5 py-0.5 text-[8px] uppercase tracking-[0.18em] ${
              ruta.esPredeterminada
                ? 'border-white/30 bg-white text-black'
                : 'border-white/10 text-white/35'
            }`}
          >
            {ruta.esPredeterminada ? 'Predeterminada' : 'Secundaria'}
          </span>
          <span
            className={`flex items-center gap-1.5 rounded-full border px-2.5 py-0.5 text-[8px] uppercase tracking-[0.18em] ${
              ruta.estaActiva
                ? 'border-emerald-400/20 text-emerald-300/80'
                : 'border-white/10 text-white/30'
            }`}
          >
            <span
              className={`h-1 w-1 rounded-full ${
                ruta.estaActiva ? 'bg-emerald-400' : 'bg-white/30'
              }`}
            />
            {ruta.estaActiva ? 'Activa' : 'Inactiva'}
          </span>
        </div>
      </div>

      <div className="mt-4 space-y-2.5 rounded-2xl border border-white/10 bg-black/30 p-3.5">
        <PathRow label="Ruta física" value={ruta.rutaFisica} />
        <PathRow label="Contenedor" value={ruta.rutaContenedor} />
        <PathRow label="En uso por la API" value={rutaVisible(ruta)} destacado />
      </div>

      {ruta.descripcion && (
        <p className="mt-3 line-clamp-2 text-xs leading-5 text-white/35">{ruta.descripcion}</p>
      )}

      <div className="mt-4 flex flex-wrap items-center gap-2 border-t border-white/5 pt-4">
        <button
          type="button"
          onClick={() => onProbar?.(ruta)}
          disabled={probando}
          className="flex items-center gap-1.5 rounded-full border border-white/10 px-3 py-1.5 text-[11px] text-white/50 transition hover:border-white/25 hover:text-white disabled:opacity-40"
        >
          <IconPulse className={`h-3.5 w-3.5 ${probando ? 'animate-pulse' : ''}`} />
          {probando ? 'Probando...' : 'Probar ruta'}
        </button>

        <button
          type="button"
          onClick={() => onEditar?.(ruta)}
          className="flex items-center gap-1.5 rounded-full border border-white/10 px-3 py-1.5 text-[11px] text-white/50 transition hover:border-white/25 hover:text-white"
        >
          <IconPencil className="h-3.5 w-3.5" />
          Editar
        </button>

        <button
          type="button"
          onClick={() => onAlternarActiva?.(ruta)}
          disabled={procesando}
          className="flex items-center gap-1.5 rounded-full border border-white/10 px-3 py-1.5 text-[11px] text-white/50 transition hover:border-white/25 hover:text-white disabled:opacity-40"
        >
          <IconPower className="h-3.5 w-3.5" />
          {ruta.estaActiva ? 'Desactivar' : 'Activar'}
        </button>

        {!ruta.esPredeterminada && (
          <button
            type="button"
            onClick={() => onMarcarPredeterminada?.(ruta)}
            disabled={procesando}
            className="flex items-center gap-1.5 rounded-full border border-white/10 px-3 py-1.5 text-[11px] text-white/50 transition hover:border-white/25 hover:text-white disabled:opacity-40"
          >
            <IconStar className="h-3.5 w-3.5" />
            Predeterminar
          </button>
        )}

        <button
          type="button"
          onClick={() => onEliminar?.(ruta)}
          className="ml-auto flex items-center gap-1.5 rounded-full border border-red-400/15 px-3 py-1.5 text-[11px] text-red-300/70 transition hover:border-red-400/40 hover:text-red-200"
        >
          <IconTrash className="h-3.5 w-3.5" />
          Eliminar
        </button>
      </div>
    </article>
  )
}

export default RutaCard

