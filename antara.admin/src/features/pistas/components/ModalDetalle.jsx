import { formatDuration, formatNumber, formatDate } from '../../../utils/formatters'
import {
  IconDisc,
  IconClose,
  IconPlay,
  IconPause,
  IconSparkles,
  IconVolume,
  IconVolumeLow,
  IconVolumeX,
} from '../../../components/ui/Icons'

const BadgeLetra = ({ tieneLetra }) => (
  <span
    className={`inline-flex items-center gap-1.5 rounded-full border px-2.5 py-0.5 text-[9px] uppercase tracking-[0.15em] ${
      tieneLetra
        ? 'border-emerald-400/25 bg-emerald-400/[0.08] text-emerald-200'
        : 'border-white/10 text-white/35'
    }`}
  >
    <span className={`h-1 w-1 rounded-full ${tieneLetra ? 'bg-emerald-400' : 'bg-white/30'}`} />
    {tieneLetra ? 'Con letra' : 'Sin letra'}
  </span>
)

const DatoDetalle = ({ label, value }) => (
  <div className="rounded-2xl border border-white/5 bg-black/20 p-3">
    <p className="text-[9px] uppercase tracking-[0.15em] text-white/30">{label}</p>
    <p className="mt-1 truncate text-sm font-semibold">{value}</p>
  </div>
)

const DatoFila = ({ label, value, mono = false }) => (
  <div className="flex items-start justify-between gap-4 py-1">
    <span className="shrink-0 text-white/35">{label}</span>
    <span
      className={`truncate text-right ${mono ? 'font-mono text-[11px] text-white/50' : 'text-white/60'}`}
      title={value || ''}
    >
      {value}
    </span>
  </div>
)

export const ModalDetalle = ({
  pista,
  activa,
  sonando,
  progreso,
  duracion,
  volumen,
  silenciado,
  onVolumen,
  onSilenciar,
  onBuscar,
  onCerrar,
  onReproducir,
  onEditarMetadatos,
}) => {
  if (!pista) return null

  const nivelVolumen = silenciado ? 0 : Math.round((volumen || 0) * 100)
  const porcentaje = duracion > 0 ? Math.min(100, (progreso / duracion) * 100) : 0

  return (
    <div
      className="fixed inset-0 z-50 flex items-end justify-center bg-black/70 p-4 backdrop-blur-sm sm:items-center"
      onClick={onCerrar}
    >
      <div
        className="max-h-[90vh] w-full max-w-2xl overflow-y-auto rounded-[2rem] border border-white/15 bg-[#0a0a0a] shadow-2xl"
        onClick={(evento) => evento.stopPropagation()}
      >
        <div className="relative">
          <div className="relative aspect-[16/7] overflow-hidden bg-gradient-to-br from-white/[0.12] via-white/[0.04] to-transparent">
            {pista.portadaUrl ? (
              <img
                src={pista.portadaUrl}
                alt={pista.albumNombre || pista.nombre}
                className="h-full w-full object-cover"
              />
            ) : (
              <div className="flex h-full items-center justify-center">
                <IconDisc className="h-14 w-14 text-white/15" />
              </div>
            )}
            <div className="absolute inset-0 bg-gradient-to-t from-[#0a0a0a] via-[#0a0a0a]/40 to-transparent" />
            <button
              type="button"
              onClick={onCerrar}
              className="absolute right-4 top-4 flex h-9 w-9 items-center justify-center rounded-full border border-white/20 bg-black/60 text-white/70 backdrop-blur transition hover:text-white"
              aria-label="Cerrar detalle"
            >
              <IconClose className="h-4 w-4" />
            </button>
          </div>

          <div className="px-5 pb-2 sm:px-7">
            <div className="-mt-10 flex items-end gap-4">
              <div className="h-20 w-20 shrink-0 overflow-hidden rounded-2xl border border-white/15 bg-white/[0.06] shadow-xl">
                {pista.portadaUrl ? (
                  <img src={pista.portadaUrl} alt="" className="h-full w-full object-cover" />
                ) : (
                  <div className="flex h-full items-center justify-center">
                    <IconDisc className="h-7 w-7 text-white/25" />
                  </div>
                )}
              </div>
              <div className="min-w-0 pb-1">
                <p className="truncate text-xl font-semibold tracking-tight">{pista.nombre}</p>
                <p className="mt-0.5 truncate text-sm text-white/45">
                  {pista.interprete || 'Artista desconocido'}
                  {pista.albumNombre ? ` · ${pista.albumNombre}` : ''}
                </p>
              </div>
            </div>
          </div>
        </div>

        <div className="space-y-4 px-5 pb-6 pt-4 sm:px-7">
          <div className="flex flex-wrap items-center gap-2">
            <button
              type="button"
              onClick={onReproducir}
              className="flex items-center gap-2 rounded-full bg-white px-4 py-2 text-sm font-medium text-black transition hover:bg-white/85"
            >
              {activa && sonando ? <IconPause className="h-4 w-4" /> : <IconPlay className="h-4 w-4" />}
              {activa && sonando ? 'Pausar' : 'Reproducir'}
            </button>
            <BadgeLetra tieneLetra={pista.tieneLetra} />
            {pista.fuenteMetadatos && (
              <span className="rounded-full border border-white/10 px-2.5 py-1 text-[8px] uppercase tracking-[0.15em] text-white/40">
                Fuente: {pista.fuenteMetadatos}
              </span>
            )}
            {onEditarMetadatos && (
              <button
                type="button"
                onClick={onEditarMetadatos}
                className="flex items-center gap-2 rounded-full border border-white/15 px-3.5 py-2 text-xs text-white/70 transition hover:border-white/30 hover:bg-white/5 hover:text-white"
              >
                <IconSparkles className="h-3.5 w-3.5 text-amber-300/80" />
                Actualizar metadatos
              </button>
            )}
          </div>

          {activa && (
            <div className="rounded-2xl border border-white/10 bg-white/[0.03] p-3.5 space-y-2">
              <div className="flex items-center justify-between text-[10px] text-white/40">
                <span className="font-mono">{formatDuration(progreso)}</span>
                <span className="font-mono">{formatDuration(duracion)}</span>
              </div>
              <div className="relative flex items-center">
                <div className="pointer-events-none absolute inset-y-0 left-0 flex w-full items-center">
                  <div className="h-1.5 w-full overflow-hidden rounded-full bg-white/10">
                    <div className="h-full rounded-full bg-white" style={{ width: `${porcentaje}%` }} />
                  </div>
                </div>
                <input
                  type="range"
                  min="0"
                  max={duracion > 0 ? duracion : 0}
                  step="0.1"
                  value={Math.min(progreso, duracion || 0)}
                  onChange={onBuscar}
                  className="relative z-10 h-4 w-full cursor-pointer appearance-none bg-transparent accent-white opacity-0"
                />
              </div>
              <div className="flex items-center gap-2 pt-1 border-t border-white/5">
                <button
                  type="button"
                  onClick={onSilenciar}
                  className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full text-white/60 transition hover:bg-white/10 hover:text-white"
                >
                  {silenciado || nivelVolumen === 0 ? (
                    <IconVolumeX className="h-3.5 w-3.5 text-red-400" />
                  ) : nivelVolumen < 50 ? (
                    <IconVolumeLow className="h-3.5 w-3.5" />
                  ) : (
                    <IconVolume className="h-3.5 w-3.5" />
                  )}
                </button>
                <input
                  type="range"
                  min="0"
                  max="100"
                  step="1"
                  value={nivelVolumen}
                  onChange={onVolumen}
                  style={{
                    background: `linear-gradient(to right, #ffffff ${nivelVolumen}%, rgba(255,255,255,0.15) ${nivelVolumen}%)`,
                  }}
                  className="h-1.5 flex-1 cursor-pointer appearance-none rounded-full accent-white"
                  title={`Volumen ${nivelVolumen}%`}
                />
                <span className="w-8 shrink-0 text-right font-mono text-[9px] text-white/40">{nivelVolumen}%</span>
              </div>
            </div>
          )}

          <div className="grid grid-cols-2 gap-2 sm:grid-cols-4">
            <DatoDetalle label="Año" value={pista.anoCreacion > 0 ? pista.anoCreacion : '—'} />
            <DatoDetalle label="Duración" value={formatDuration(pista.duracionSegundos)} />
            <DatoDetalle label="Género" value={pista.generoNombre || '—'} />
            <DatoDetalle label="Reproducciones" value={formatNumber(pista.reproducciones)} />
          </div>

          <div className="space-y-2 rounded-2xl border border-white/10 bg-black/30 p-4 text-xs">
            <DatoFila label="Álbum" value={pista.albumNombre || '—'} />
            <DatoFila label="Registrada" value={formatDate(pista.fechaRegistro)} />
            <DatoFila label="Archivo" value={pista.url || '—'} mono />
          </div>

          <div>
            <div className="flex items-center justify-between gap-3">
              <p className="text-[10px] uppercase tracking-[0.25em] text-white/30">Letra</p>
              {pista.tieneLetra && (
                <span className="text-[9px] text-white/25">Obtenida desde lyrics.ovh</span>
              )}
            </div>
            {pista.tieneLetra && pista.letra ? (
              <div className="mt-2 max-h-64 overflow-y-auto whitespace-pre-line rounded-2xl border border-white/10 bg-white/[0.02] p-4 text-xs leading-6 text-white/60">
                {pista.letra}
              </div>
            ) : (
              <div className="mt-2 rounded-2xl border border-dashed border-white/10 bg-white/[0.01] px-4 py-6 text-center text-xs text-white/30">
                Esta pista no tiene letra registrada.
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}

export default ModalDetalle

