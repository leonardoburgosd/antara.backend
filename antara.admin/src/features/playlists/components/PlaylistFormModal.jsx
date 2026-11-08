import { useEffect, useState } from 'react'
import { Modal } from '../../../components/ui/Modal'
import { Input } from '../../../components/ui/Input'
import { Button } from '../../../components/ui/Button'
import { IconAlertCircle, IconPlaylist, IconClose } from '../../../components/ui/Icons'

const MAX_PORTADA_MB = 10

const estadoInicial = (playlist) => ({
  nombre: playlist?.nombre ?? '',
  descripcion: playlist?.descripcion ?? '',
  portadaUrl: playlist?.portadaUrl ?? '',
  archivo: null,
})

export const PlaylistFormModal = ({ isOpen, playlist, onClose, onSubmit }) => {
  const [valores, setValores] = useState(() => estadoInicial(playlist))
  const [error, setError] = useState('')
  const [guardando, setGuardando] = useState(false)
  const [portadaLocal, setPortadaLocal] = useState('')

  useEffect(() => {
    if (!isOpen) return
    setValores(estadoInicial(playlist))
    setError('')
  }, [isOpen, playlist])

  // Vista previa del archivo elegido; se libera al cambiarlo o cerrar el modal.
  useEffect(() => {
    if (!valores.archivo) {
      setPortadaLocal('')
      return undefined
    }
    const url = URL.createObjectURL(valores.archivo)
    setPortadaLocal(url)
    return () => URL.revokeObjectURL(url)
  }, [valores.archivo])

  const esEdicion = Boolean(playlist?.id)
  const portadaVisible = portadaLocal || valores.portadaUrl

  const setCampo = (campo) => (evento) =>
    setValores((actuales) => ({ ...actuales, [campo]: evento.target.value }))

  const elegirPortada = (evento) => {
    const archivo = evento.target.files?.[0]
    if (!archivo) return

    if (!archivo.type.startsWith('image/')) {
      setError('La portada debe ser una imagen.')
      return
    }
    if (archivo.size > MAX_PORTADA_MB * 1024 * 1024) {
      setError(`La portada no puede superar los ${MAX_PORTADA_MB} MB.`)
      return
    }

    setError('')
    setValores((actuales) => ({ ...actuales, archivo }))
  }

  const quitarPortada = () =>
    setValores((actuales) => ({ ...actuales, archivo: null, portadaUrl: '' }))

  const handleGuardar = async () => {
    if (!valores.nombre.trim()) {
      setError('El nombre de la playlist es obligatorio.')
      return
    }

    setGuardando(true)
    setError('')
    try {
      await onSubmit({
        nombre: valores.nombre,
        descripcion: valores.descripcion,
        portada: valores.archivo,
        portadaUrl: valores.archivo ? null : valores.portadaUrl?.trim() || null,
        quitarPortada: esEdicion && !valores.archivo && !valores.portadaUrl?.trim(),
      })
    } catch (err) {
      setError(err.message || 'No se pudo guardar la playlist.')
    } finally {
      setGuardando(false)
    }
  }

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={esEdicion ? 'Editar playlist' : 'Nueva playlist'}
      subtitle={
        esEdicion
          ? 'Actualiza el nombre, la descripción o la portada.'
          : 'Crea una lista y agrégale canciones de cualquier artista.'
      }
      size="lg"
    >
      <div className="space-y-4">
        {error && (
          <div className="flex items-start gap-2 rounded-2xl border border-red-500/30 bg-red-500/10 px-4 py-3 text-xs text-red-200">
            <IconAlertCircle className="mt-0.5 h-4 w-4 shrink-0" />
            <span>{error}</span>
          </div>
        )}

        <Input label="Nombre de la playlist" value={valores.nombre} onChange={setCampo('nombre')} />

        <div className="space-y-1.5">
          <label className="block text-[10px] uppercase tracking-[0.2em] text-white/40 font-medium">
            Descripción
          </label>
          <textarea
            value={valores.descripcion}
            onChange={setCampo('descripcion')}
            rows={3}
            placeholder="Para qué momento es esta playlist (opcional)"
            className="w-full rounded-2xl border border-white/10 bg-black/40 px-5 py-3 text-sm text-white placeholder:text-white/25 transition focus:border-white focus:outline-none"
          />
        </div>

        <div className="space-y-2">
          <label className="block text-[10px] uppercase tracking-[0.2em] text-white/40 font-medium">
            Portada
          </label>
          <div className="flex items-center gap-4 rounded-2xl border border-white/10 bg-black/30 p-3">
            <div className="flex h-16 w-16 shrink-0 items-center justify-center overflow-hidden rounded-2xl border border-white/10 bg-white/[0.04]">
              {portadaVisible ? (
                <img src={portadaVisible} alt="Portada de la playlist" className="h-full w-full object-cover" />
              ) : (
                <IconPlaylist className="h-6 w-6 text-white/25" />
              )}
            </div>
            <div className="min-w-0 flex-1 space-y-2">
              <input
                type="file"
                accept="image/*"
                onChange={elegirPortada}
                className="block w-full text-xs text-white/50 file:mr-3 file:rounded-full file:border-0 file:bg-white/10 file:px-3 file:py-1.5 file:text-xs file:text-white/80 hover:file:bg-white/20"
              />
              <p className="text-[10px] text-white/30">
                {esEdicion
                  ? 'Si no eliges una imagen se conserva la portada actual.'
                  : `Opcional, hasta ${MAX_PORTADA_MB} MB.`}
              </p>
            </div>
            {portadaVisible && (
              <button
                type="button"
                onClick={quitarPortada}
                className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full text-white/35 transition hover:bg-white/10 hover:text-white"
                title="Quitar portada"
                aria-label="Quitar portada"
              >
                <IconClose className="h-4 w-4" />
              </button>
            )}
          </div>

          <Input
            label="o usa una URL"
            value={valores.archivo ? '' : valores.portadaUrl}
            onChange={setCampo('portadaUrl')}
            placeholder="https://..."
            disabled={Boolean(valores.archivo)}
          />
        </div>

        <div className="flex justify-end gap-3 pt-2">
          <Button variant="ghost" onClick={onClose} disabled={guardando}>
            Cancelar
          </Button>
          <Button onClick={handleGuardar} loading={guardando}>
            {esEdicion ? 'Guardar cambios' : 'Crear playlist'}
          </Button>
        </div>
      </div>
    </Modal>
  )
}

export default PlaylistFormModal
