import { formatDuration } from '../../../utils/formatters'
import { IconDisc, IconPlay, IconPause, IconSparkles } from '../../../components/ui/Icons'

const BotonPortada = ({ pista, activa, sonando, onClick }) => {
  return (
    <button
      type="button"
      onClick={onClick}
      className="group relative flex h-11 w-11 shrink-0 items-center justify-center overflow-hidden rounded-xl border border-white/10 bg-white/[0.04]"
      aria-label={activa && sonando ? 'Pausar' : 'Reproducir'}
      title={activa && sonando ? 'Pausar' : 'Reproducir'}
    >
      {pista.portadaUrl ? (
        <img src={pista.portadaUrl} alt="" className="h-full w-full object-cover" loading="lazy" />
      ) : (
        <IconDisc className="h-5 w-5 text-white/30" />
      )}
      <span
        className={`absolute inset-0 flex items-center justify-center bg-black/55 text-white transition ${
          activa ? 'opacity-100' : 'opacity-0 group-hover:opacity-100'
        }`}
      >
        {activa && sonando ? <IconPause className="h-4 w-4" /> : <IconPlay className="h-4 w-4" />}
      </span>
    </button>
  )
}

export const PistaCardsMobile = ({
  pistas = [],
  pistaSeleccionadaId,
  reproduciendoId,
  sonando,
  onSeleccionar,
  onAlternarReproduccion,
  onAbrirDetalle,
  onAbrirMetadatos,
}) => {
  return (
    <div className="space-y-2.5 md:hidden">
      {pistas.map((pista) => {
        const isSelected = pistaSeleccionadaId === pista.id
        const isPlaying = reproduciendoId === pista.id
        return (
          <article
            key={pista.id}
            onClick={() => onAbrirDetalle(pista)}
            className={`flex flex-col gap-2 rounded-2xl border p-3 transition ${
              isPlaying
                ? 'border-white/25 bg-white/[0.06]'
                : isSelected
                  ? 'border-white/40 bg-white/[0.05]'
                  : 'border-white/10 bg-white/[0.03] hover:border-white/20'
            }`}
          >
            <div className="flex items-center gap-3">
              <input
                type="checkbox"
                checked={isSelected}
                onClick={(e) => e.stopPropagation()}
                onChange={() => onSeleccionar(pista.id)}
                className="h-4 w-4 rounded border-white/30 bg-transparent text-white accent-white cursor-pointer shrink-0"
                aria-label={`Seleccionar ${pista.nombre}`}
              />
              <BotonPortada
                pista={pista}
                activa={isPlaying}
                sonando={sonando}
                onClick={(evento) => {
                  evento.stopPropagation()
                  onAlternarReproduccion(pista)
                }}
              />
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm font-medium">{pista.nombre}</p>
                <p className="truncate text-[11px] text-white/35">
                  {pista.interprete || 'Sin artista'}
                  {pista.albumNombre ? ` · ${pista.albumNombre}` : ''}
                </p>
                <div className="mt-1 flex flex-wrap items-center gap-1.5 text-[9px] text-white/30">
                  {pista.anoCreacion > 0 && <span>{pista.anoCreacion}</span>}
                  {pista.duracionSegundos > 0 && (
                    <>
                      <span className="h-0.5 w-0.5 rounded-full bg-white/20" />
                      <span className="font-mono">{formatDuration(pista.duracionSegundos)}</span>
                    </>
                  )}
                  {pista.tieneLetra && (
                    <>
                      <span className="h-0.5 w-0.5 rounded-full bg-white/20" />
                      <span className="text-emerald-300/70">Con letra</span>
                    </>
                  )}
                </div>
              </div>
              <button
                type="button"
                onClick={(e) => {
                  e.stopPropagation()
                  onAbrirMetadatos(pista)
                }}
                className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full border border-white/15 text-white/60 transition hover:bg-white/10 hover:text-white"
                title="Actualizar metadatos"
              >
                <IconSparkles className="h-4 w-4 text-amber-300/80" />
              </button>
            </div>
          </article>
        )
      })}
    </div>
  )
}

export default PistaCardsMobile

