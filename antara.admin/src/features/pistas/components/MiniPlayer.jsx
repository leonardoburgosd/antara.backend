import { formatDuration } from '../../../utils/formatters'
import {
  IconMusic,
  IconPlay,
  IconPause,
  IconClose,
  IconSparkles,
  IconVolume,
  IconVolumeLow,
  IconVolumeX,
} from '../../../components/ui/Icons'

export const MiniPlayer = ({
  pista,
  sonando,
  progreso,
  duracion,
  volumen,
  silenciado,
  onAlternar,
  onCerrar,
  onBuscar,
  onVolumen,
  onSilenciar,
  onAbrir,
  onMetadatos,
}) => {
  if (!pista) return null

  const porcentaje = duracion > 0 ? Math.min(100, (progreso / duracion) * 100) : 0
  const nivelVolumen = silenciado ? 0 : Math.round((volumen || 0) * 100)

  return (
    <div className="fixed bottom-4 left-1/2 z-40 w-[calc(100%-2rem)] max-w-xl -translate-x-1/2 rounded-[1.5rem] border border-white/15 bg-[#0a0a0a]/95 p-3.5 shadow-2xl backdrop-blur sm:left-auto sm:right-6 sm:translate-x-0">
      <div className="flex items-center gap-3">
        <button
          type="button"
          onClick={onAbrir}
          className="flex h-11 w-11 shrink-0 items-center justify-center overflow-hidden rounded-xl border border-white/10 bg-white/[0.04]"
          title="Ver detalle"
        >
          {pista.portadaUrl ? (
            <img src={pista.portadaUrl} alt="" className="h-full w-full object-cover" />
          ) : (
            <IconMusic className="h-5 w-5 text-white/30" />
          )}
        </button>

        <div className="min-w-0 flex-1">
          <button
            type="button"
            onClick={onAbrir}
            className="block max-w-full truncate text-left text-xs font-medium text-white hover:underline"
          >
            {pista.nombre}
          </button>
          <p className="truncate text-[10px] text-white/40">
            {pista.interprete || 'Sin artista'}
            {pista.albumNombre ? ` · ${pista.albumNombre}` : ''}
          </p>
        </div>

        <button
          type="button"
          onClick={onMetadatos}
          className="flex h-8 items-center gap-1.5 rounded-full border border-white/15 px-2.5 text-[11px] text-white/60 transition hover:border-white/30 hover:bg-white/10 hover:text-white shrink-0"
          title="Actualizar metadatos"
        >
          <IconSparkles className="h-3 w-3 text-amber-300/80" />
          <span className="hidden sm:inline">Metadatos</span>
        </button>

        <button
          type="button"
          onClick={onAlternar}
          className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-white text-black transition hover:bg-white/85"
          aria-label={sonando ? 'Pausar' : 'Reproducir'}
        >
          {sonando ? <IconPause className="h-4 w-4" /> : <IconPlay className="h-4 w-4" />}
        </button>

        <button
          type="button"
          onClick={onCerrar}
          className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full text-white/35 transition hover:bg-white/10 hover:text-white"
          aria-label="Cerrar reproductor"
        >
          <IconClose className="h-3.5 w-3.5" />
        </button>
      </div>

      <div className="mt-2.5 flex items-center gap-2">
        <span className="w-8 shrink-0 text-right font-mono text-[9px] text-white/40">
          {formatDuration(progreso)}
        </span>
        <div className="relative flex-1 flex items-center">
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
            disabled={duracion <= 0}
            className="relative z-10 h-4 w-full cursor-pointer appearance-none bg-transparent accent-white opacity-0"
            aria-label="Progreso de la canción"
          />
        </div>
        <span className="w-8 shrink-0 font-mono text-[9px] text-white/40">{formatDuration(duracion)}</span>
      </div>

      <div className="mt-2.5 flex items-center gap-2.5 border-t border-white/5 pt-2">
        <button
          type="button"
          onClick={onSilenciar}
          className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full text-white/60 transition hover:bg-white/10 hover:text-white"
          aria-label={silenciado || nivelVolumen === 0 ? 'Activar sonido' : 'Silenciar'}
          title={silenciado || nivelVolumen === 0 ? 'Activar sonido' : 'Silenciar'}
        >
          {silenciado || nivelVolumen === 0 ? (
            <IconVolumeX className="h-4 w-4 text-red-400/80" />
          ) : nivelVolumen < 50 ? (
            <IconVolumeLow className="h-4 w-4" />
          ) : (
            <IconVolume className="h-4 w-4" />
          )}
        </button>
        <div className="relative flex-1 flex items-center">
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
            className="h-1.5 w-full cursor-pointer appearance-none rounded-full accent-white"
            aria-label="Control de volumen"
            title={`Volumen: ${nivelVolumen}%`}
          />
        </div>
        <span className="w-8 shrink-0 text-right font-mono text-[9px] text-white/45">{nivelVolumen}%</span>
      </div>
    </div>
  )
}

export default MiniPlayer

