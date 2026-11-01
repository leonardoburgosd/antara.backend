import { useState } from 'react'
import { formatNumber, formatDate, formatDuration, getInitials } from '../../../utils/formatters'
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

const AvatarArtista = ({ nombre, src }) => {
  const [fallo, setFallo] = useState(false)

  return (
    <span className="flex h-7 w-7 shrink-0 items-center justify-center overflow-hidden rounded-full border border-white/10 bg-white/[0.06] text-[9px] font-semibold text-white/60">
      {src && !fallo ? (
        <img
          src={src}
          alt={nombre || 'Artista'}
          className="h-full w-full object-cover"
          onError={() => setFallo(true)}
        />
      ) : (
        getInitials(nombre, '?')
      )}
    </span>
  )
}

const BadgeLetra = ({ tieneLetra }) => (
  <span
    className={`inline-flex items-center gap-1 rounded-full border px-2 py-0.5 text-[8px] uppercase tracking-[0.12em] ${
      tieneLetra
        ? 'border-emerald-400/20 text-emerald-300/80'
        : 'border-white/10 text-white/30'
    }`}
  >
    <span className={`h-1 w-1 rounded-full ${tieneLetra ? 'bg-emerald-400' : 'bg-white/25'}`} />
    {tieneLetra ? 'Con letra' : 'Sin letra'}
  </span>
)

export const PistasTable = ({
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
    <div className="hidden overflow-hidden rounded-[1.75rem] border border-white/10 bg-white/[0.03] md:block">
      <div className="overflow-x-auto">
        <table className="w-full min-w-[1040px] text-left text-sm">
          <thead>
            <tr className="border-b border-white/10 text-[9px] uppercase tracking-[0.2em] text-white/25">
              <th className="px-4 py-3.5 w-10 text-center">Sel.</th>
              <th className="px-3 py-3.5 font-medium">#</th>
              <th className="py-3.5 pr-4 font-medium">Canción</th>
              <th className="py-3.5 pr-4 font-medium">Artista</th>
              <th className="py-3.5 pr-4 font-medium">Año</th>
              <th className="py-3.5 pr-4 font-medium">Duración</th>
              <th className="py-3.5 pr-4 font-medium">Género</th>
              <th className="py-3.5 pr-4 font-medium">Letra</th>
              <th className="py-3.5 pr-4 font-medium">Reprod.</th>
              <th className="py-3.5 pr-4 text-right font-medium">Registrada</th>
              <th className="py-3.5 pr-5 text-right font-medium">Acciones</th>
            </tr>
          </thead>
          <tbody>
            {pistas.map((pista, indice) => {
              const isSelected = pistaSeleccionadaId === pista.id
              const isPlaying = reproduciendoId === pista.id
              return (
                <tr
                  key={pista.id}
                  onClick={() => onAbrirDetalle(pista)}
                  className={`cursor-pointer border-b border-white/5 transition last:border-0 hover:bg-white/[0.03] ${
                    isPlaying ? 'bg-white/[0.04]' : ''
                  } ${isSelected ? 'border-l-2 border-l-white bg-white/[0.05]' : ''}`}
                >
                  <td
                    className="px-4 py-3.5 text-center"
                    onClick={(e) => onSeleccionar(pista.id, e)}
                  >
                    <input
                      type="checkbox"
                      checked={isSelected}
                      onChange={() => {}}
                      className="h-4 w-4 rounded border-white/30 bg-transparent text-white accent-white cursor-pointer"
                      aria-label={`Seleccionar pista ${pista.nombre}`}
                    />
                  </td>
                  <td className="px-3 py-3.5 text-[10px] text-white/25">
                    {String(indice + 1).padStart(2, '0')}
                  </td>
                  <td className="py-3.5 pr-4">
                    <div className="flex items-center gap-3">
                      <BotonPortada
                        pista={pista}
                        activa={isPlaying}
                        sonando={sonando}
                        onClick={(evento) => {
                          evento.stopPropagation()
                          onAlternarReproduccion(pista)
                        }}
                      />
                      <div className="min-w-0">
                        <p className="max-w-[240px] truncate font-medium">{pista.nombre}</p>
                        <p className="max-w-[240px] truncate text-[11px] text-white/30">
                          {pista.albumNombre || 'Sin álbum'}
                        </p>
                      </div>
                    </div>
                  </td>
                  <td className="py-3.5 pr-4">
                    <div className="flex items-center gap-2">
                      <AvatarArtista nombre={pista.interprete} src={pista.imagenArtista} />
                      <span className="max-w-[150px] truncate text-white/55">
                        {pista.interprete || '—'}
                      </span>
                    </div>
                  </td>
                  <td className="py-3.5 pr-4 text-white/45">
                    {pista.anoCreacion > 0 ? pista.anoCreacion : '—'}
                  </td>
                  <td className="py-3.5 pr-4 font-mono text-[11px] text-white/45">
                    {formatDuration(pista.duracionSegundos)}
                  </td>
                  <td className="py-3.5 pr-4 text-white/45">{pista.generoNombre || '—'}</td>
                  <td className="py-3.5 pr-4">
                    <BadgeLetra tieneLetra={pista.tieneLetra} />
                  </td>
                  <td className="py-3.5 pr-4 text-white/45">{formatNumber(pista.reproducciones)}</td>
                  <td className="py-3.5 pr-4 text-right text-[11px] text-white/30">
                    {formatDate(pista.fechaRegistro)}
                  </td>
                  <td className="py-3.5 pr-5 text-right" onClick={(e) => e.stopPropagation()}>
                    <div className="flex items-center justify-end gap-1.5">
                      <button
                        type="button"
                        onClick={() => onAbrirMetadatos(pista)}
                        className="flex items-center gap-1.5 rounded-full border border-white/15 px-2.5 py-1 text-xs text-white/60 transition hover:border-white/35 hover:bg-white/10 hover:text-white"
                        title="Buscar y actualizar metadatos"
                      >
                        <IconSparkles className="h-3 w-3 text-amber-300/80" />
                        <span className="hidden xl:inline">Metadatos</span>
                      </button>
                    </div>
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
      </div>
    </div>
  )
}

export default PistasTable

