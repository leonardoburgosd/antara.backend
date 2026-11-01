import { IconPulse, IconClose } from '../../../components/ui/Icons'

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

const StatusLine = ({ label, value }) => {
  return (
    <div className="flex items-center justify-between gap-4 py-1">
      <span className="text-white/35">{label}</span>
      <span className={`flex items-center gap-1.5 ${value ? 'text-emerald-300' : 'text-white/35'}`}>
        <span className={`h-1.5 w-1.5 rounded-full ${value ? 'bg-emerald-400' : 'bg-white/25'}`} />
        {value ? 'Sí' : 'No'}
      </span>
    </div>
  )
}

export const PruebaPanel = ({ prueba, onClose }) => {
  if (!prueba) return null

  return (
    <div className="fixed inset-0 z-50 flex items-end justify-center bg-black/70 p-4 backdrop-blur-sm sm:items-center">
      <div className="w-full max-w-md rounded-[2rem] border border-white/15 bg-[#0a0a0a] p-6 shadow-2xl">
        <div className="flex items-start justify-between gap-4">
          <div>
            <p className="text-[10px] uppercase tracking-[0.35em] text-white/35">Diagnóstico</p>
            <h3 className="mt-2 text-xl font-semibold tracking-tight">{prueba.ruta.nombre}</h3>
          </div>
          <button
            type="button"
            onClick={onClose}
            className="flex h-9 w-9 items-center justify-center rounded-full border border-white/10 text-white/40 transition hover:border-white/25 hover:text-white"
            aria-label="Cerrar"
          >
            <IconClose className="h-4 w-4" />
          </button>
        </div>

        <div
          className={`mt-4 flex items-start gap-3 rounded-2xl border px-4 py-3 text-sm ${
            prueba.data.existe
              ? 'border-emerald-400/20 bg-emerald-400/[0.06] text-emerald-100'
              : 'border-amber-400/20 bg-amber-400/[0.06] text-amber-100'
          }`}
        >
          <IconPulse className="mt-0.5 h-4 w-4 shrink-0" />
          <span>{prueba.data.mensaje}</span>
        </div>

        <div className="mt-4 space-y-2 rounded-2xl border border-white/10 bg-black/30 p-3.5 text-xs">
          <PathRow label="Ruta evaluada" value={prueba.data.rutaEvaluada} destacado />
          <StatusLine label="Existe" value={prueba.data.existe} />
          <StatusLine label="Es directorio" value={prueba.data.esDirectorio} />
          <StatusLine label="Escribible" value={prueba.data.escribible} />
          <div className="flex items-center justify-between gap-4 py-1">
            <span className="text-white/35">Archivos de audio</span>
            <span className="text-white/60">{prueba.data.archivosAudio}</span>
          </div>
          <div className="flex items-center justify-between gap-4 py-1">
            <span className="text-white/35">Subdirectorios</span>
            <span className="text-white/60">{prueba.data.subdirectorios}</span>
          </div>
          {prueba.data.rutaFisicaConfigurada && (
            <StatusLine label="Ruta física visible" value={prueba.data.rutaFisicaVisible} />
          )}
        </div>

        <button
          type="button"
          onClick={onClose}
          className="mt-5 w-full rounded-full bg-white px-5 py-2.5 text-sm font-medium text-black transition hover:bg-white/85"
        >
          Cerrar
        </button>
      </div>
    </div>
  )
}

export default PruebaPanel

