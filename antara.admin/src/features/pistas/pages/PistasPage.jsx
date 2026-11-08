import { useState, useMemo } from 'react'
import { usePistas } from '../hooks/usePistas'
import { useAudioPlayer } from '../hooks/useAudioPlayer'
import { formatNumber } from '../../../utils/formatters'

import PistasMetrics from '../components/PistasMetrics'
import PistasFilters from '../components/PistasFilters'
import PistaSeleccionadaBanner from '../components/PistaSeleccionadaBanner'
import PistasTable from '../components/PistasTable'
import PistaCardsMobile from '../components/PistaCardsMobile'
import MiniPlayer from '../components/MiniPlayer'
import ModalDetalle from '../components/ModalDetalle'
import ModalMetadatos from '../components/ModalMetadatos'
import SubirPistaModal from '../components/SubirPistaModal'

import { EmptyState } from '../../../components/ui/EmptyState'
import { SkeletonTable } from '../../../components/ui/Skeleton'
import { ErrorBanner } from '../../../components/ui/ErrorBanner'
import {
  IconScan,
  IconRefresh,
  IconMusic,
  IconSearch,
  IconFolder,
  IconClose,
  IconUpload,
} from '../../../components/ui/Icons'

export const PistasPage = ({ onToast = () => {}, onEscanear }) => {
  const {
    pistas,
    filtradas,
    conteos,
    estadisticas,
    loading,
    error,
    busqueda,
    setBusqueda,
    filtro,
    setFiltro,
    cargar,
    actualizarPistaLocal,
  } = usePistas()

  const {
    audioRef,
    reproduciendoId,
    sonando,
    progreso,
    duracionAudio,
    volumen,
    silenciado,
    alternarReproduccion,
    detener,
    buscarTiempo,
    cambiarVolumen,
    alternarSilencio,
    setSonando,
    setProgreso,
    setDuracionAudio,
  } = useAudioPlayer({ onToast })

  const [pistaSeleccionadaId, setPistaSeleccionadaId] = useState(null)
  const [detalle, setDetalle] = useState(null)
  const [pistaMetadatos, setPistaMetadatos] = useState(null)
  const [subirAbierto, setSubirAbierto] = useState(false)

  const pistaActual = useMemo(
    () => pistas.find((p) => p.id === reproduciendoId) || null,
    [pistas, reproduciendoId],
  )

  const pistaSeleccionada = useMemo(
    () => pistas.find((p) => p.id === pistaSeleccionadaId) || null,
    [pistas, pistaSeleccionadaId],
  )

  const alternarSeleccion = (pistaId, evento) => {
    if (evento) evento.stopPropagation()
    setPistaSeleccionadaId((actual) => (actual === pistaId ? null : pistaId))
  }

  const aplicarSubida = async (resultado) => {
    if (!resultado) return
    setSubirAbierto(false)
    onToast(
      resultado.conMetadatos
        ? `«${resultado.nombre}» se subió y se completaron sus metadatos.`
        : `«${resultado.nombre}» se subió a ${resultado.rutaDestino}.`,
    )
    await cargar()
  }

  const aplicarMetadatosActualizados = (actualizada) => {
    if (!actualizada?.id) return
    actualizarPistaLocal(actualizada)
    setDetalle((actual) => (actual && actual.id === actualizada.id ? actualizada : actual))
    setPistaMetadatos(null)
    onToast('Metadatos actualizados correctamente.')
  }

  return (
    <section className="space-y-5 pb-24">
      <audio
        ref={audioRef}
        className="hidden"
        onPlay={() => setSonando(true)}
        onPause={() => setSonando(false)}
        onTimeUpdate={(evento) => setProgreso(evento.currentTarget.currentTime || 0)}
        onLoadedMetadata={(evento) => {
          const valor = evento.currentTarget.duration
          setDuracionAudio(Number.isFinite(valor) ? valor : 0)
        }}
        onEnded={() => {
          setSonando(false)
          setProgreso(0)
        }}
      />

      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <p className="text-[10px] uppercase tracking-[0.35em] text-white/35">Biblioteca</p>
          <h2 className="mt-2 text-2xl font-semibold tracking-tight sm:text-3xl">Pistas</h2>
          <p className="mt-2 max-w-2xl text-sm leading-6 text-white/40">
            Canciones encontradas en tus rutas de audio, con artista, álbum, portada, año, género y
            letra obtenidos desde Deezer o TheAudioDB.
          </p>
        </div>
        <div className="flex flex-wrap gap-2">
          <button
            type="button"
            onClick={() => setSubirAbierto(true)}
            className="flex items-center gap-2 rounded-full border border-white/15 px-4 py-2.5 text-sm text-white/70 transition hover:border-white/30 hover:text-white"
          >
            <IconUpload className="h-4 w-4" />
            Subir canción
          </button>
          {onEscanear && (
            <button
              type="button"
              onClick={onEscanear}
              className="flex items-center gap-2 rounded-full bg-white px-4 py-2.5 text-sm font-medium text-black transition hover:bg-white/85"
            >
              <IconScan className="h-4 w-4" />
              Escanear rutas
            </button>
          )}
          <button
            type="button"
            onClick={cargar}
            disabled={loading}
            className="flex items-center gap-2 rounded-full border border-white/15 px-4 py-2.5 text-sm text-white/60 transition hover:border-white/30 hover:text-white disabled:opacity-40"
          >
            <IconRefresh className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
            Actualizar
          </button>
        </div>
      </div>

      <PistasMetrics estadisticas={estadisticas} />

      <PistasFilters
        busqueda={busqueda}
        onBusquedaChange={setBusqueda}
        onLimpiarBusqueda={() => setBusqueda('')}
        filtro={filtro}
        onFiltroChange={setFiltro}
        conteos={conteos}
      />

      <PistaSeleccionadaBanner
        pista={pistaSeleccionada}
        isPlaying={reproduciendoId === pistaSeleccionada?.id}
        sonando={sonando}
        onMetadatos={() => setPistaMetadatos(pistaSeleccionada)}
        onAlternarReproduccion={() => alternarReproduccion(pistaSeleccionada)}
        onDetalle={() => setDetalle(pistaSeleccionada)}
        onDeseleccionar={() => setPistaSeleccionadaId(null)}
      />

      {error && <ErrorBanner message={error} onRetry={cargar} />}

      {loading ? (
        <div className="rounded-[1.75rem] border border-white/10 bg-white/[0.03] p-6">
          <SkeletonTable rows={6} cols={5} />
        </div>
      ) : pistas.length === 0 ? (
        <EmptyState
          icon={IconMusic}
          title="Aún no hay pistas en la biblioteca"
          description="Escanea tus rutas de audio para descubrir canciones y completar sus metadatos automáticamente."
          action={
            onEscanear ? (
              <button
                type="button"
                onClick={onEscanear}
                className="mt-3 flex items-center gap-2 rounded-full bg-white px-4 py-2 text-xs font-semibold text-black"
              >
                <IconFolder className="h-3.5 w-3.5" />
                Ir a rutas de audio
              </button>
            ) : null
          }
        />
      ) : filtradas.length === 0 ? (
        <EmptyState
          icon={IconSearch}
          title="Sin resultados"
          description="No encontramos canciones que coincidan con la búsqueda o el filtro seleccionado."
          action={
            <button
              type="button"
              onClick={() => {
                setBusqueda('')
                setFiltro('todas')
              }}
              className="mt-3 flex items-center gap-2 rounded-full border border-white/15 px-4 py-2 text-xs text-white/70 hover:text-white"
            >
              <IconClose className="h-3.5 w-3.5" />
              Limpiar filtros
            </button>
          }
        />
      ) : (
        <>
          <PistasTable
            pistas={filtradas}
            pistaSeleccionadaId={pistaSeleccionadaId}
            reproduciendoId={reproduciendoId}
            sonando={sonando}
            onSeleccionar={alternarSeleccion}
            onAlternarReproduccion={alternarReproduccion}
            onAbrirDetalle={setDetalle}
            onAbrirMetadatos={setPistaMetadatos}
          />

          <PistaCardsMobile
            pistas={filtradas}
            pistaSeleccionadaId={pistaSeleccionadaId}
            reproduciendoId={reproduciendoId}
            sonando={sonando}
            onSeleccionar={alternarSeleccion}
            onAlternarReproduccion={alternarReproduccion}
            onAbrirDetalle={setDetalle}
            onAbrirMetadatos={setPistaMetadatos}
          />

          <p className="text-[10px] uppercase tracking-[0.2em] text-white/25">
            Mostrando {formatNumber(filtradas.length)} de {formatNumber(pistas.length)} canciones
          </p>
        </>
      )}

      {pistaActual && (
        <MiniPlayer
          pista={pistaActual}
          sonando={sonando}
          progreso={progreso}
          duracion={duracionAudio}
          onAlternar={() => alternarReproduccion(pistaActual)}
          onBuscar={buscarTiempo}
          onCerrar={detener}
          onAbrir={() => setDetalle(pistaActual)}
          volumen={volumen}
          silenciado={silenciado}
          onVolumen={cambiarVolumen}
          onSilenciar={alternarSilencio}
          onMetadatos={() => setPistaMetadatos(pistaActual)}
        />
      )}

      {detalle && (
        <ModalDetalle
          pista={detalle}
          activa={reproduciendoId === detalle.id}
          sonando={sonando}
          progreso={reproduciendoId === detalle.id ? progreso : 0}
          duracion={reproduciendoId === detalle.id ? duracionAudio : detalle.duracionSegundos}
          volumen={volumen}
          silenciado={silenciado}
          onVolumen={cambiarVolumen}
          onSilenciar={alternarSilencio}
          onBuscar={buscarTiempo}
          onCerrar={() => setDetalle(null)}
          onReproducir={() => alternarReproduccion(detalle)}
          onEditarMetadatos={() => setPistaMetadatos(detalle)}
        />
      )}

      <SubirPistaModal
        isOpen={subirAbierto}
        onClose={() => setSubirAbierto(false)}
        onSubida={aplicarSubida}
      />

      {pistaMetadatos && (
        <ModalMetadatos
          pista={pistaMetadatos}
          onCerrar={() => setPistaMetadatos(null)}
          onActualizada={aplicarMetadatosActualizados}
          onToast={onToast}
        />
      )}
    </section>
  )
}

export default PistasPage

