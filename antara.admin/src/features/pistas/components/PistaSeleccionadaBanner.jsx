import { IconSparkles, IconPlay, IconPause, IconClose } from '../../../components/ui/Icons'

export const PistaSeleccionadaBanner = ({
  pista,
  isPlaying,
  sonando,
  onMetadatos,
  onAlternarReproduccion,
  onDetalle,
  onDeseleccionar,
}) => {
  if (!pista) return null

  return (
    <div className="flex flex-wrap items-center justify-between gap-3 rounded-2xl border border-white/20 bg-[#121212]/95 px-5 py-3 shadow-2xl backdrop-blur transition animate-in fade-in">
      <div className="flex items-center gap-3 min-w-0">
        <span className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-white text-[11px] font-bold text-black">
          ✓
        </span>
        <div className="min-w-0">
          <p className="truncate text-xs font-semibold text-white">{pista.nombre}</p>
          <p className="truncate text-[10px] text-white/45">
            {pista.interprete || 'Sin artista'}
            {pista.albumNombre ? ` · ${pista.albumNombre}` : ''}
          </p>
        </div>
      </div>
      <div className="flex flex-wrap items-center gap-2 shrink-0">
        <button
          type="button"
          onClick={onMetadatos}
          className="flex items-center gap-1.5 rounded-full bg-white px-3.5 py-1.5 text-xs font-semibold text-black transition hover:bg-white/85 shadow"
        >
          <IconSparkles className="h-3.5 w-3.5" />
          Buscar y actualizar metadatos
        </button>
        <button
          type="button"
          onClick={onAlternarReproduccion}
          className="flex items-center gap-1.5 rounded-full border border-white/20 px-3 py-1.5 text-xs text-white transition hover:bg-white/10"
        >
          {isPlaying && sonando ? <IconPause className="h-3.5 w-3.5" /> : <IconPlay className="h-3.5 w-3.5" />}
          {isPlaying && sonando ? 'Pausar' : 'Reproducir'}
        </button>
        <button
          type="button"
          onClick={onDetalle}
          className="flex items-center gap-1.5 rounded-full border border-white/20 px-3 py-1.5 text-xs text-white/70 transition hover:bg-white/10 hover:text-white"
        >
          Detalle
        </button>
        <button
          type="button"
          onClick={onDeseleccionar}
          className="flex h-7 w-7 items-center justify-center rounded-full text-white/40 transition hover:bg-white/10 hover:text-white"
          title="Deseleccionar"
        >
          <IconClose className="h-3.5 w-3.5" />
        </button>
      </div>
    </div>
  )
}

export default PistaSeleccionadaBanner

