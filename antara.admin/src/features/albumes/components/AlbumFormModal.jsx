import { useEffect, useState } from 'react'
import { Modal } from '../../../components/ui/Modal'
import { Input } from '../../../components/ui/Input'
import { Select } from '../../../components/ui/Select'
import { Button } from '../../../components/ui/Button'
import { IconAlertCircle, IconDisc } from '../../../components/ui/Icons'

const estadoInicial = (album) => ({
  nombre: album?.nombre ?? '',
  artista: album?.artista ?? '',
  descripcion: album?.descripcion ?? '',
  portadaUrl: album?.portadaUrl ?? '',
  estaPublicado: album ? Boolean(album.estaPublicado) : false,
  estaActivo: album ? Boolean(album.estaActivo) : true,
})

export const AlbumFormModal = ({ isOpen, album, onClose, onSubmit }) => {
  const [valores, setValores] = useState(() => estadoInicial(album))
  const [error, setError] = useState('')
  const [guardando, setGuardando] = useState(false)
  const [portadaError, setPortadaError] = useState(false)

  useEffect(() => {
    if (!isOpen) return
    setValores(estadoInicial(album))
    setError('')
  }, [isOpen, album])

  useEffect(() => {
    setPortadaError(false)
  }, [valores.portadaUrl])

  const esEdicion = Boolean(album?.id)

  const setCampo = (campo) => (evento) =>
    setValores((actuales) => ({ ...actuales, [campo]: evento.target.value }))

  const setBooleano = (campo, valor) =>
    setValores((actuales) => ({ ...actuales, [campo]: valor === 'true' }))

  const handleGuardar = async () => {
    if (!valores.nombre.trim()) {
      setError('El nombre del álbum es obligatorio.')
      return
    }

    const payload = {
      nombre: valores.nombre.trim(),
      artista: valores.artista?.trim() || null,
      descripcion: valores.descripcion?.trim() || null,
      portadaUrl: valores.portadaUrl?.trim() || null,
      estaPublicado: valores.estaPublicado,
    }

    setGuardando(true)
    setError('')
    try {
      await onSubmit(esEdicion ? { ...payload, estaActivo: valores.estaActivo } : payload)
    } catch (err) {
      setError(err.message || 'No se pudo guardar el álbum.')
    } finally {
      setGuardando(false)
    }
  }

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={esEdicion ? 'Editar álbum' : 'Nuevo álbum'}
      subtitle={
        esEdicion
          ? 'Actualiza los datos, la portada o el estado de publicación.'
          : 'Agrupa tus pistas en un álbum y decide si se publica.'
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

        <Input label="Nombre del álbum" value={valores.nombre} onChange={setCampo('nombre')} />
        <Input
          label="Artista"
          value={valores.artista}
          onChange={setCampo('artista')}
          placeholder="Artista o banda"
        />

        <div className="space-y-1.5">
          <label className="block text-[10px] uppercase tracking-[0.2em] text-white/40 font-medium">
            Descripción
          </label>
          <textarea
            value={valores.descripcion}
            onChange={setCampo('descripcion')}
            rows={3}
            placeholder="Descripción opcional del álbum"
            className="w-full rounded-2xl border border-white/10 bg-black/40 px-5 py-3 text-sm text-white placeholder:text-white/25 transition focus:border-white focus:outline-none"
          />
        </div>

        <Input
          label="Portada (URL)"
          value={valores.portadaUrl}
          onChange={setCampo('portadaUrl')}
          placeholder="https://..."
        />

        {valores.portadaUrl?.trim() && (
          <div className="flex items-center gap-3 rounded-2xl border border-white/10 bg-white/[0.02] p-3">
            <div className="flex h-16 w-16 shrink-0 items-center justify-center overflow-hidden rounded-xl border border-white/10 bg-black/40">
              {portadaError ? (
                <IconDisc className="h-6 w-6 text-white/25" />
              ) : (
                <img
                  src={valores.portadaUrl}
                  alt="Portada del álbum"
                  className="h-full w-full object-cover"
                  onError={() => setPortadaError(true)}
                />
              )}
            </div>
            <div className="min-w-0">
              <p className="text-[10px] uppercase tracking-[0.2em] text-white/30">Portada actual</p>
              <p className="mt-1 text-xs text-white/45">
                Así se muestra en las canciones del álbum y en el reproductor.
              </p>
            </div>
          </div>
        )}

        <div className="grid gap-4 sm:grid-cols-2">
          <Select
            label="Publicación"
            value={String(valores.estaPublicado)}
            onChange={(evento) => setBooleano('estaPublicado', evento.target.value)}
          >
            <option value="false" className="bg-[#121212]">Borrador</option>
            <option value="true" className="bg-[#121212]">Publicado</option>
          </Select>

          {esEdicion && (
            <Select
              label="Estado"
              value={String(valores.estaActivo)}
              onChange={(evento) => setBooleano('estaActivo', evento.target.value)}
            >
              <option value="true" className="bg-[#121212]">Activo</option>
              <option value="false" className="bg-[#121212]">Inactivo</option>
            </Select>
          )}
        </div>

        <div className="flex justify-end gap-3 pt-2">
          <Button variant="ghost" onClick={onClose} disabled={guardando}>
            Cancelar
          </Button>
          <Button onClick={handleGuardar} loading={guardando}>
            {esEdicion ? 'Guardar cambios' : 'Crear álbum'}
          </Button>
        </div>
      </div>
    </Modal>
  )
}

export default AlbumFormModal
