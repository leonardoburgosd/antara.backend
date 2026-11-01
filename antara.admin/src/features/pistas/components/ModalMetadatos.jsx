import { useState, useEffect, useCallback } from 'react'
import { formatDuration } from '../../../utils/formatters'
import { pistaService } from '../services/pistaService'
import { IconClose, IconSearch, IconDisc, IconRefresh, IconCheck, IconRadio } from '../../../components/ui/Icons'

export const ModalMetadatos = ({ pista, onCerrar, onActualizada, onToast = () => {} }) => {
  const consultaInicial = [pista.interprete, pista.nombre].filter(Boolean).join(' ').trim()
  const [cadena, setCadena] = useState(consultaInicial)
  const [proveedor, setProveedor] = useState('todos')
  const [candidatos, setCandidatos] = useState([])
  const [buscando, setBuscando] = useState(false)
  const [aplicando, setAplicando] = useState(false)
  const [seleccionado, setSeleccionado] = useState('')
  const [error, setError] = useState('')

  const buscar = useCallback(
    async (termino, provSeleccionado = proveedor) => {
      const valor = String(termino ?? '').trim()
      if (!valor) {
        setError('Ingresa el nombre de la canción o el artista.')
        return
      }

      setBuscando(true)
      setError('')
      setSeleccionado('')

      try {
        const lista = await pistaService.searchMetadata(valor, provSeleccionado || 'todos')
        setCandidatos(lista)
        if (lista.length === 0) setError('No se encontraron coincidencias para esa búsqueda.')
      } catch (err) {
        setError(err.message || 'Error al buscar coincidencias.')
        setCandidatos([])
      } finally {
        setBuscando(false)
      }
    },
    [proveedor],
  )

  useEffect(() => {
    if (consultaInicial) buscar(consultaInicial, 'todos')
  }, [consultaInicial, buscar])

  useEffect(() => {
    const alPresionar = (evento) => {
      if (evento.key === 'Escape' && !aplicando) onCerrar()
    }
    window.addEventListener('keydown', alPresionar)
    return () => window.removeEventListener('keydown', alPresionar)
  }, [aplicando, onCerrar])

  const cambiarProveedor = (nuevoProveedor) => {
    setProveedor(nuevoProveedor)
    if (cadena.trim()) {
      buscar(cadena, nuevoProveedor)
    }
  }

  const enviar = (evento) => {
    evento.preventDefault()
    buscar(cadena, proveedor)
  }

  const aplicar = async () => {
    if (!seleccionado) {
      setError('Selecciona una coincidencia para actualizar los metadatos.')
      return
    }

    const candidatoElegido = candidatos.find((candidato) => candidato.idExterno === seleccionado)

    setAplicando(true)
    setError('')

    try {
      const actualizada = await pistaService.updateMetadata(
        pista.id,
        seleccionado,
        candidatoElegido?.fuente || (proveedor !== 'todos' ? proveedor : undefined),
      )
      onActualizada(actualizada)
    } catch (err) {
      setError(err.message || 'Error al actualizar los metadatos.')
    } finally {
      setAplicando(false)
    }
  }

  const candidatoElegido = candidatos.find((candidato) => candidato.idExterno === seleccionado) || null

  return (
    <div
      className="fixed inset-0 z-[60] flex items-end justify-center bg-black/80 p-4 backdrop-blur-sm sm:items-center"
      onClick={() => !aplicando && onCerrar()}
    >
      <div
        className="max-h-[92vh] w-full max-w-2xl overflow-y-auto rounded-[2rem] border border-white/15 bg-[#0a0a0a] shadow-2xl"
        onClick={(evento) => evento.stopPropagation()}
      >
        <div className="flex items-start justify-between gap-4 border-b border-white/10 px-5 py-4 sm:px-7">
          <div className="min-w-0">
            <p className="text-[10px] uppercase tracking-[0.3em] text-white/30">Metadatos</p>
            <p className="mt-1 truncate text-lg font-semibold tracking-tight">Buscar y actualizar metadatos</p>
            <p className="mt-1 truncate text-xs text-white/40">
              {pista.nombre}
              {pista.interprete ? ` · ${pista.interprete}` : ''}
            </p>
          </div>
          <button
            type="button"
            onClick={onCerrar}
            disabled={aplicando}
            className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full border border-white/15 text-white/60 transition hover:text-white disabled:opacity-40"
            aria-label="Cerrar metadatos"
          >
            <IconClose className="h-4 w-4" />
          </button>
        </div>

        <div className="space-y-4 px-5 py-5 sm:px-7">
          <div className="flex flex-col gap-2.5">
            <div className="flex items-center gap-2 text-xs">
              <span className="text-[10px] uppercase tracking-wider text-white/40">Proveedor:</span>
              <div className="flex gap-1 rounded-full border border-white/10 bg-black/40 p-1">
                {[
                  { id: 'todos', label: 'Todos' },
                  { id: 'deezer', label: 'Deezer' },
                  { id: 'theaudiodb', label: 'TheAudioDB' },
                ].map((item) => {
                  const activo = proveedor === item.id
                  return (
                    <button
                      key={item.id}
                      type="button"
                      onClick={() => cambiarProveedor(item.id)}
                      disabled={buscando}
                      className={`rounded-full px-3 py-1 text-[11px] font-medium transition ${
                        activo
                          ? 'bg-white text-black'
                          : 'text-white/50 hover:text-white hover:bg-white/5'
                      }`}
                    >
                      {item.label}
                    </button>
                  )
                })}
              </div>
            </div>

            <form onSubmit={enviar} className="flex flex-col gap-2 sm:flex-row">
              <div className="flex flex-1 items-center gap-2 rounded-full border border-white/10 bg-black/30 px-4 py-2.5">
                <IconSearch className="h-4 w-4 shrink-0 text-white/30" />
                <input
                  type="search"
                  value={cadena}
                  onChange={(evento) => setCadena(evento.target.value)}
                  placeholder="Canción o artista..."
                  className="w-full bg-transparent text-sm text-white placeholder:text-white/25 focus:outline-none"
                  autoFocus
                />
              </div>
              <button
                type="submit"
                disabled={buscando}
                className="flex items-center justify-center gap-2 rounded-full bg-white px-5 py-2.5 text-sm font-medium text-black transition hover:bg-white/85 disabled:opacity-50"
              >
                <IconSearch className={`h-4 w-4 ${buscando ? 'animate-pulse' : ''}`} />
                {buscando ? 'Buscando...' : 'Buscar'}
              </button>
            </form>
          </div>

          <p className="text-[11px] leading-5 text-white/35">
            Busca la versión correcta en{' '}
            {proveedor === 'todos'
              ? 'Deezer y TheAudioDB'
              : proveedor === 'theaudiodb'
                ? 'TheAudioDB'
                : 'Deezer'}{' '}
            y selecciona una coincidencia. Al aplicarla se actualizarán título, artista, álbum,
            portada, año, género, duración y letra.
          </p>

          {error && (
            <div className="rounded-2xl border border-red-400/20 bg-red-400/[0.06] px-4 py-3 text-xs text-red-100">
              {error}
            </div>
          )}

          {buscando && (
            <div className="space-y-2">
              {Array.from({ length: 4 }).map((_, indice) => (
                <div
                  key={indice}
                  className="flex items-center gap-3 rounded-2xl border border-white/5 bg-white/[0.02] p-3"
                >
                  <div className="h-12 w-12 animate-pulse rounded-xl bg-white/[0.06]" />
                  <div className="flex-1 space-y-2">
                    <div className="h-2.5 w-2/5 animate-pulse rounded bg-white/[0.08]" />
                    <div className="h-2 w-1/4 animate-pulse rounded bg-white/[0.05]" />
                  </div>
                </div>
              ))}
            </div>
          )}

          {!buscando && candidatos.length > 0 && (
            <div className="max-h-[38vh] space-y-2 overflow-y-auto pr-1">
              {candidatos.map((candidato) => {
                const activo = candidato.idExterno === seleccionado
                const esTheAudioDb = candidato.fuente?.toLowerCase() === 'theaudiodb'
                return (
                  <button
                    key={`${candidato.fuente}-${candidato.idExterno}`}
                    type="button"
                    onClick={() => setSeleccionado(candidato.idExterno)}
                    className={`flex w-full items-center gap-3 rounded-2xl border p-2.5 text-left transition ${
                      activo
                        ? 'border-white/40 bg-white/[0.08]'
                        : 'border-white/10 bg-white/[0.02] hover:border-white/20'
                    }`}
                  >
                    <span className="flex h-12 w-12 shrink-0 items-center justify-center overflow-hidden rounded-xl border border-white/10 bg-white/[0.04]">
                      {candidato.portadaUrl ? (
                        <img
                          src={candidato.portadaUrl}
                          alt=""
                          className="h-full w-full object-cover"
                          loading="lazy"
                        />
                      ) : (
                        <IconDisc className="h-5 w-5 text-white/25" />
                      )}
                    </span>
                    <span className="min-w-0 flex-1">
                      <span className="block truncate text-xs font-semibold text-white">
                        {candidato.nombre}
                      </span>
                      <span className="mt-0.5 block truncate text-[11px] text-white/50">
                        {candidato.interprete || 'Sin artista'}
                        {candidato.albumNombre ? ` · ${candidato.albumNombre}` : ''}
                      </span>
                      <span className="mt-1 flex flex-wrap items-center gap-2 text-[10px] text-white/30">
                        {candidato.anoCreacion > 0 && <span>{candidato.anoCreacion}</span>}
                        {candidato.duracionSegundos > 0 && (
                          <span>{formatDuration(candidato.duracionSegundos)}</span>
                        )}
                        {candidato.generoNombre && <span>{candidato.generoNombre}</span>}
                      </span>
                    </span>
                    <span className="flex shrink-0 flex-col items-end gap-1.5 pl-2">
                      <span
                        className={`rounded-full border px-2 py-0.5 text-[8px] uppercase tracking-wider ${
                          esTheAudioDb
                            ? 'border-indigo-400/30 bg-indigo-500/10 text-indigo-300'
                            : 'border-amber-400/30 bg-amber-500/10 text-amber-300'
                        }`}
                      >
                        {candidato.fuente || 'Deezer'}
                      </span>
                      <span
                        className={`flex h-5 w-5 items-center justify-center rounded-full border ${
                          activo ? 'border-white bg-white text-black' : 'border-white/20 text-transparent'
                        }`}
                      >
                        <IconCheck className="h-3 w-3" />
                      </span>
                    </span>
                  </button>
                )
              })}
            </div>
          )}

          {candidatoElegido && (
            <div className="rounded-2xl border border-white/10 bg-black/40 p-3.5 text-xs">
              <p className="text-[10px] uppercase tracking-wider text-white/30">
                Coincidencia seleccionada
              </p>
              <div className="mt-2 flex items-center gap-3">
                <div className="flex h-10 w-10 shrink-0 items-center justify-center overflow-hidden rounded-xl border border-white/10 bg-white/[0.04]">
                  {candidatoElegido.portadaUrl ? (
                    <img src={candidatoElegido.portadaUrl} alt="" className="h-full w-full object-cover" />
                  ) : (
                    <IconDisc className="h-5 w-5 text-white/20" />
                  )}
                </div>
                <div className="min-w-0 flex-1">
                  <p className="truncate font-semibold text-white">{candidatoElegido.nombre}</p>
                  <p className="truncate text-[11px] text-white/50">
                    {candidatoElegido.interprete} · {candidatoElegido.albumNombre || 'Sin álbum'}
                  </p>
                </div>
              </div>
            </div>
          )}
        </div>

        <div className="flex flex-col-reverse gap-2 border-t border-white/10 px-5 py-4 sm:flex-row sm:justify-end sm:px-7">
          <button
            type="button"
            onClick={onCerrar}
            disabled={aplicando}
            className="rounded-full border border-white/15 px-5 py-2.5 text-sm text-white/60 transition hover:border-white/30 hover:text-white disabled:opacity-40"
          >
            Cancelar
          </button>
          <button
            type="button"
            onClick={aplicar}
            disabled={aplicando || !seleccionado}
            className="flex items-center justify-center gap-2 rounded-full bg-white px-5 py-2.5 text-sm font-medium text-black transition hover:bg-white/85 disabled:opacity-40"
          >
            {aplicando && <IconRefresh className="h-4 w-4 animate-spin" />}
            {aplicando ? 'Actualizando...' : 'Actualizar metadatos'}
          </button>
        </div>
      </div>
    </div>
  )
}

export default ModalMetadatos

