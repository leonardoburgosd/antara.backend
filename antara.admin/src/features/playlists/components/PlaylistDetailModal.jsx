import { useEffect, useMemo, useState } from 'react'
import { Modal } from '../../../components/ui/Modal'
import { Input } from '../../../components/ui/Input'
import { Button } from '../../../components/ui/Button'
import { IconAlertCircle, IconMusic, IconPlus, IconPlaylist, IconRefresh, IconSearch, IconTrash } from '../../../components/ui/Icons'
import { formatDuration, normalizeText } from '../../../utils/formatters'

const AvatarCancion = ({ cancion }) => (
  <span className="flex h-10 w-10 shrink-0 items-center justify-center overflow-hidden rounded-xl border border-white/10 bg-white/[0.04]">
    {cancion.portadaUrl || cancion.imagenArtista ? (
      <img
        src={cancion.portadaUrl || cancion.imagenArtista}
        alt=""
        loading="lazy"
        className="h-full w-full object-cover"
      />
    ) : (
      <IconMusic className="h-4 w-4 text-white/25" />
    )}
  </span>
)

const ChipArtista = ({ nombre }) => (
  <span className="rounded-full border border-white/10 bg-white/[0.04] px-2.5 py-1 text-[10px] text-white/55">
    {nombre}
  </span>
)

export const PlaylistDetailModal = ({
  isOpen,
  playlist,
  canciones = [],
  biblioteca = [],
  cargando = false,
  onClose,
  onAgregar,
  onQuitar,
}) => {
  const [busqueda, setBusqueda] = useState('')
  const [pendientes, setPendientes] = useState([])
  const [error, setError] = useState('')

  useEffect(() => {
    if (!isOpen) return
    setBusqueda('')
    setPendientes([])
    setError('')
  }, [isOpen, playlist?.id])

  const enPlaylist = useMemo(() => new Set(canciones.map((cancion) => cancion.id)), [canciones])

  const artistas = useMemo(() => {
    const nombres = new Map()
    canciones.forEach((cancion) => {
      const nombre = cancion.interprete?.trim()
      if (nombre) nombres.set(normalizeText(nombre), nombre)
    })
    return [...nombres.values()]
  }, [canciones])

  const duracionTotal = useMemo(
    () => canciones.reduce((total, cancion) => total + (Number(cancion.duracionSegundos) || 0), 0),
    [canciones],
  )

  const disponibles = useMemo(() => {
    const termino = normalizeText(busqueda)
    return biblioteca
      .filter((pista) => !enPlaylist.has(pista.id))
      .filter((pista) => {
        if (!termino) return true
        return [pista.nombre, pista.interprete, pista.albumNombre]
          .filter(Boolean)
          .some((valor) => normalizeText(valor).includes(termino))
      })
  }, [biblioteca, enPlaylist, busqueda])

  const estaPendiente = (id) => pendientes.includes(id)

  const ejecutar = async (id, accion) => {
    if (!playlist?.id || estaPendiente(id)) return
    setPendientes((actuales) => [...actuales, id])
    setError('')
    try {
      await accion()
    } catch (err) {
      setError(err.message || 'No se pudo actualizar la playlist.')
    } finally {
      setPendientes((actuales) => actuales.filter((valor) => valor !== id))
    }
  }

  const agregar = (pista) => ejecutar(pista.id, () => onAgregar(pista.id))
  const quitar = (cancion) => ejecutar(cancion.id, () => onQuitar(cancion.id))

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={playlist?.nombre || 'Playlist'}
      subtitle="Las canciones pueden venir de distintos artistas y álbumes."
      size="2xl"
    >
      <div className="space-y-5">
        <div className="flex flex-col gap-4 rounded-3xl border border-white/10 bg-white/[0.03] p-4 sm:flex-row sm:items-center">
          <div className="flex h-20 w-20 shrink-0 items-center justify-center overflow-hidden rounded-2xl border border-white/10 bg-white/[0.04]">
            {playlist?.portadaUrl ? (
              <img src={playlist.portadaUrl} alt="" className="h-full w-full object-cover" />
            ) : (
              <IconPlaylist className="h-8 w-8 text-white/25" />
            )}
          </div>
          <div className="min-w-0 flex-1">
            {playlist?.descripcion ? (
              <p className="text-sm leading-6 text-white/50">{playlist.descripcion}</p>
            ) : (
              <p className="text-sm text-white/30">Sin descripción.</p>
            )}
            <div className="mt-3 flex flex-wrap items-center gap-2 text-[10px] uppercase tracking-[0.18em] text-white/35">
              <span className="rounded-full border border-white/10 px-2.5 py-1">
                {canciones.length} {canciones.length === 1 ? 'canción' : 'canciones'}
              </span>
              <span className="rounded-full border border-white/10 px-2.5 py-1">
                {artistas.length} {artistas.length === 1 ? 'artista' : 'artistas'}
              </span>
              {duracionTotal > 0 && (
                <span className="rounded-full border border-white/10 px-2.5 py-1">
                  {formatDuration(duracionTotal)}
                </span>
              )}
            </div>
            {artistas.length > 0 && (
              <div className="mt-3 flex flex-wrap gap-1.5">
                {artistas.slice(0, 8).map((nombre) => (
                  <ChipArtista key={nombre} nombre={nombre} />
                ))}
                {artistas.length > 8 && (
                  <span className="px-2 py-1 text-[10px] text-white/35">+{artistas.length - 8} más</span>
                )}
              </div>
            )}
          </div>
        </div>

        {error && (
          <div className="flex items-start gap-2 rounded-2xl border border-red-500/30 bg-red-500/10 px-4 py-3 text-xs text-red-200">
            <IconAlertCircle className="mt-0.5 h-4 w-4 shrink-0" />
            <span>{error}</span>
          </div>
        )}

        <div className="grid gap-5 lg:grid-cols-2">
          <section className="rounded-3xl border border-white/10 bg-white/[0.03] p-4">
            <header className="mb-3 flex items-center justify-between">
              <h3 className="text-xs uppercase tracking-[0.22em] text-white/40">Canciones</h3>
              {cargando && <IconRefresh className="h-4 w-4 animate-spin text-white/30" />}
            </header>

            {cargando && canciones.length === 0 ? (
              <p className="py-8 text-center text-xs text-white/30">Cargando canciones…</p>
            ) : canciones.length === 0 ? (
              <p className="py-8 text-center text-xs text-white/30">
                La playlist está vacía. Agrega canciones desde la lista de la derecha.
              </p>
            ) : (
              <ul className="max-h-[360px] space-y-1.5 overflow-y-auto pr-1">
                {canciones.map((cancion, indice) => (
                  <li
                    key={cancion.id}
                    className="flex items-center gap-3 rounded-2xl border border-transparent px-2 py-2 transition hover:border-white/10 hover:bg-white/[0.03]"
                  >
                    <span className="w-5 shrink-0 text-right text-[10px] text-white/25">
                      {String(indice + 1).padStart(2, '0')}
                    </span>
                    <AvatarCancion cancion={cancion} />
                    <div className="min-w-0 flex-1">
                      <p className="truncate text-sm text-white/85">{cancion.nombre}</p>
                      <p className="truncate text-[11px] text-white/35">
                        {[cancion.interprete || 'Artista desconocido', cancion.albumNombre]
                          .filter(Boolean)
                          .join(' · ')}
                      </p>
                    </div>
                    <span className="shrink-0 text-[10px] text-white/25">
                      {formatDuration(cancion.duracionSegundos)}
                    </span>
                    <button
                      type="button"
                      onClick={() => quitar(cancion)}
                      disabled={estaPendiente(cancion.id)}
                      className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full text-white/30 transition hover:bg-red-500/15 hover:text-red-200 disabled:opacity-40"
                      title="Quitar de la playlist"
                      aria-label={`Quitar ${cancion.nombre}`}
                    >
                      {estaPendiente(cancion.id) ? (
                        <IconRefresh className="h-4 w-4 animate-spin" />
                      ) : (
                        <IconTrash className="h-4 w-4" />
                      )}
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </section>

          <section className="rounded-3xl border border-white/10 bg-white/[0.03] p-4">
            <header className="mb-3 flex items-center justify-between gap-2">
              <h3 className="text-xs uppercase tracking-[0.22em] text-white/40">Agregar canciones</h3>
              <span className="text-[10px] text-white/25">Cualquier artista</span>
            </header>

            <Input
              icon={IconSearch}
              value={busqueda}
              onChange={(evento) => setBusqueda(evento.target.value)}
              onClear={() => setBusqueda('')}
              placeholder="Buscar por canción, artista o álbum"
            />

            {biblioteca.length === 0 ? (
              <p className="py-8 text-center text-xs text-white/30">
                La biblioteca está vacía. Sube pistas o escanea una ruta para poder agregarlas.
              </p>
            ) : disponibles.length === 0 ? (
              <p className="py-8 text-center text-xs text-white/30">
                {busqueda
                  ? 'No hay canciones que coincidan con la búsqueda.'
                  : 'Todas las canciones de la biblioteca ya están en esta playlist.'}
              </p>
            ) : (
              <>
                <ul className="mt-3 max-h-[300px] space-y-1.5 overflow-y-auto pr-1">
                  {disponibles.slice(0, 80).map((pista) => (
                    <li
                      key={pista.id}
                      className="flex items-center gap-3 rounded-2xl border border-transparent px-2 py-2 transition hover:border-white/10 hover:bg-white/[0.03]"
                    >
                      <AvatarCancion cancion={pista} />
                      <div className="min-w-0 flex-1">
                        <p className="truncate text-sm text-white/85">{pista.nombre}</p>
                        <p className="truncate text-[11px] text-white/35">
                          {[pista.interprete || 'Artista desconocido', pista.albumNombre]
                            .filter(Boolean)
                            .join(' · ')}
                        </p>
                      </div>
                      <span className="shrink-0 text-[10px] text-white/25">
                        {formatDuration(pista.duracionSegundos)}
                      </span>
                      <button
                        type="button"
                        onClick={() => agregar(pista)}
                        disabled={estaPendiente(pista.id)}
                        className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full border border-white/10 text-white/60 transition hover:border-white/30 hover:bg-white/10 hover:text-white disabled:opacity-40"
                        title="Agregar a la playlist"
                        aria-label={`Agregar ${pista.nombre}`}
                      >
                        {estaPendiente(pista.id) ? (
                          <IconRefresh className="h-4 w-4 animate-spin" />
                        ) : (
                          <IconPlus className="h-4 w-4" />
                        )}
                      </button>
                    </li>
                  ))}
                </ul>
                {disponibles.length > 80 && (
                  <p className="mt-2 px-2 text-[10px] text-white/25">
                    Mostrando 80 de {disponibles.length}. Usa la búsqueda para afinar.
                  </p>
                )}
              </>
            )}
          </section>
        </div>

        <div className="flex justify-end">
          <Button variant="outline" onClick={onClose}>
            Listo
          </Button>
        </div>
      </div>
    </Modal>
  )
}

export default PlaylistDetailModal
