import { useEffect, useRef, useState } from 'react'
import { Modal } from '../../../components/ui/Modal'
import { Input } from '../../../components/ui/Input'
import { Select } from '../../../components/ui/Select'
import { Button } from '../../../components/ui/Button'
import { pistaService } from '../services/pistaService'
import {
  IconAlertCircle,
  IconFolder,
  IconMusic,
  IconUpload,
  IconLink,
} from '../../../components/ui/Icons'

// Mismas extensiones que acepta la API: solo audio.
const EXTENSIONES_AUDIO = ['mp3', 'flac', 'wav', 'm4a', 'aac', 'ogg', 'opus', 'wma', 'aiff', 'alac']
const TAMANO_MAXIMO = 2 * 1024 * 1024 * 1024
const PATRON_YOUTUBE = /^https?:\/\/(www\.|m\.|music\.)?(youtube\.com|youtu\.be)\//i

const esUrlYouTube = (valor) => PATRON_YOUTUBE.test((valor || '').trim())

const extension = (nombre = '') => {
  const punto = nombre.lastIndexOf('.')
  return punto < 0 ? '' : nombre.slice(punto + 1).toLowerCase()
}

const esAudio = (archivo) => Boolean(archivo) && EXTENSIONES_AUDIO.includes(extension(archivo.name))

const tamanoLegible = (bytes = 0) => {
  if (bytes >= 1024 * 1024 * 1024) return `${(bytes / (1024 * 1024 * 1024)).toFixed(2)} GB`
  if (bytes >= 1024 * 1024) return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
  return `${Math.max(1, Math.round(bytes / 1024))} KB`
}

const MODOS = [
  { id: 'archivo', label: 'Archivo de audio' },
  { id: 'youtube', label: 'Desde YouTube' },
]

export const SubirPistaModal = ({ isOpen, onClose, onSubida }) => {
  const inputRef = useRef(null)
  const [rutas, setRutas] = useState([])
  const [cargandoRutas, setCargandoRutas] = useState(false)
  const [rutaId, setRutaId] = useState('')
  const [modo, setModo] = useState('archivo')
  const [subcarpeta, setSubcarpeta] = useState('')
  const [archivo, setArchivo] = useState(null)
  const [url, setUrl] = useState('')
  const [error, setError] = useState('')
  const [subiendo, setSubiendo] = useState(false)

  useEffect(() => {
    if (!isOpen) return

    setArchivo(null)
    setUrl('')
    setModo('archivo')
    setSubcarpeta('')
    setError('')
    setSubiendo(false)
    setCargandoRutas(true)

    pistaService
      .getRutasSubida()
      .then((lista) => {
        setRutas(lista)
        const preferida =
          lista.find((ruta) => ruta.esPredeterminada && ruta.escribible) ||
          lista.find((ruta) => ruta.escribible) ||
          lista[0]
        setRutaId(preferida?.id ?? '')
      })
      .catch((err) => setError(err.message || 'No se pudieron cargar las rutas de audio.'))
      .finally(() => setCargandoRutas(false))
  }, [isOpen])

  const rutaSeleccionada = rutas.find((ruta) => ruta.id === rutaId) || null
  const sinRutas = !cargandoRutas && rutas.length === 0
  const ytValido = esUrlYouTube(url)

  const elegirArchivo = (evento) => {
    const seleccionado = evento.target.files?.[0] ?? null
    setError('')

    if (!seleccionado) {
      setArchivo(null)
      return
    }

    if (!esAudio(seleccionado)) {
      setArchivo(null)
      if (inputRef.current) inputRef.current.value = ''
      setError(`Solo se permiten archivos de audio (${EXTENSIONES_AUDIO.join(', ')}).`)
      return
    }

    if (seleccionado.size > TAMANO_MAXIMO) {
      setArchivo(null)
      if (inputRef.current) inputRef.current.value = ''
      setError('El archivo supera el límite de 2 GB.')
      return
    }

    setArchivo(seleccionado)
  }

  const cambiarModo = (nuevo) => {
    setModo(nuevo)
    setError('')
  }

  const subir = async () => {
    if (!rutaSeleccionada) {
      setError('Selecciona la ruta de destino.')
      return
    }

    if (!rutaSeleccionada.escribible) {
      setError(
        `La ruta «${rutaSeleccionada.nombre}» no admite escritura desde la API. Revisa el montaje de la unidad.`,
      )
      return
    }

    if (modo === 'youtube') {
      if (!url.trim()) {
        setError('Pega el enlace de YouTube de la canción.')
        return
      }
      if (!ytValido) {
        setError('El enlace debe ser de YouTube (youtube.com o youtu.be).')
        return
      }
    } else if (!archivo) {
      setError('Selecciona el archivo de audio que quieres subir.')
      return
    }

    setSubiendo(true)
    setError('')

    try {
      const resultado =
        modo === 'youtube'
          ? await pistaService.subirDesdeUrl({ url, rutaId: rutaSeleccionada.id, subcarpeta })
          : await pistaService.subirPista({ archivo, rutaId: rutaSeleccionada.id, subcarpeta })

      onSubida?.(resultado)
      onClose?.()
    } catch (err) {
      setError(err.message || 'No se pudo subir la canción.')
    } finally {
      setSubiendo(false)
    }
  }

  const etiquetaBoton =
    modo === 'youtube' ? (subiendo ? 'Descargando…' : 'Descargar y subir') : subiendo ? 'Subiendo…' : 'Subir canción'

  return (
    <Modal
      isOpen={isOpen}
      onClose={subiendo ? undefined : onClose}
      closeOnBackdrop={!subiendo}
      title="Subir canción"
      subtitle="Guarda un archivo de audio propio o descarga el audio de un enlace de YouTube en la ruta elegida."
      size="lg"
    >
      <div className="space-y-4">
        {error && (
          <div className="flex items-start gap-2 rounded-2xl border border-red-500/30 bg-red-500/10 px-4 py-3 text-xs text-red-200">
            <IconAlertCircle className="mt-0.5 h-4 w-4 shrink-0" />
            <span>{error}</span>
          </div>
        )}

        <div className="flex flex-wrap gap-2">
          {MODOS.map((item) => (
            <button
              key={item.id}
              type="button"
              onClick={() => cambiarModo(item.id)}
              disabled={subiendo}
              className={`flex items-center gap-2 rounded-full border px-3.5 py-2 text-xs transition disabled:opacity-40 ${
                modo === item.id
                  ? 'border-white bg-white text-black'
                  : 'border-white/10 text-white/45 hover:border-white/30 hover:text-white'
              }`}
            >
              {item.id === 'youtube' ? <IconLink className="h-3.5 w-3.5" /> : <IconMusic className="h-3.5 w-3.5" />}
              {item.label}
            </button>
          ))}
        </div>

        {sinRutas ? (
          <div className="rounded-2xl border border-amber-500/30 bg-amber-500/10 px-4 py-3 text-xs text-amber-100">
            No hay rutas de audio activas. Crea una ruta en «Rutas de audio» antes de subir archivos.
          </div>
        ) : (
          <>
            <Select
              label="Ruta de destino"
              value={rutaId}
              onChange={(evento) => setRutaId(evento.target.value)}
              disabled={cargandoRutas || subiendo}
            >
              {cargandoRutas && <option value="">Cargando rutas…</option>}
              {!cargandoRutas &&
                rutas.map((ruta) => (
                  <option key={ruta.id} value={ruta.id} className="bg-[#121212]">
                    {ruta.nombre}
                    {ruta.esPredeterminada ? ' (predeterminada)' : ''}
                    {ruta.escribible ? ` — ${ruta.rutaDestino}` : ' — no escribible'}
                  </option>
                ))}
            </Select>

            {rutaSeleccionada && (
              <div
                className={`flex items-start gap-2 rounded-2xl border px-4 py-3 text-[11px] ${
                  rutaSeleccionada.escribible
                    ? 'border-white/10 bg-white/[0.02] text-white/45'
                    : 'border-amber-500/30 bg-amber-500/10 text-amber-100'
                }`}
              >
                <IconFolder className="mt-0.5 h-3.5 w-3.5 shrink-0" />
                <span className="break-all">
                  {rutaSeleccionada.escribible
                    ? `Se guardará en ${rutaSeleccionada.rutaDestino}`
                    : 'La carpeta de esta ruta no existe o el contenedor no puede escribir en ella.'}
                </span>
              </div>
            )}

            <Input
              label="Subcarpeta (opcional)"
              value={subcarpeta}
              onChange={(evento) => setSubcarpeta(evento.target.value)}
              placeholder="Ej. Rock/2026"
              icon={IconFolder}
              disabled={subiendo}
            />

            {modo === 'archivo' ? (
              <div className="space-y-2">
                <label className="block text-[10px] uppercase tracking-[0.2em] text-white/40 font-medium">
                  Archivo de audio
                </label>
                <input
                  ref={inputRef}
                  type="file"
                  accept=".mp3,.flac,.wav,.m4a,.aac,.ogg,.opus,.wma,.aiff,.alac,audio/*"
                  onChange={elegirArchivo}
                  disabled={subiendo}
                  className="block w-full cursor-pointer rounded-2xl border border-dashed border-white/15 bg-black/30 px-4 py-6 text-xs text-white/50 file:mr-4 file:rounded-full file:border-0 file:bg-white file:px-4 file:py-2 file:text-xs file:font-semibold file:text-black hover:border-white/30 disabled:cursor-not-allowed disabled:opacity-50"
                />
                <p className="px-1 text-[10px] text-white/25">
                  Formatos permitidos: {EXTENSIONES_AUDIO.join(', ')}. Tamaño máximo: 2 GB.
                </p>
              </div>
            ) : (
              <div className="space-y-2">
                <Input
                  label="Enlace de YouTube"
                  value={url}
                  onChange={(evento) => setUrl(evento.target.value)}
                  placeholder="https://www.youtube.com/watch?v=..."
                  icon={IconLink}
                  disabled={subiendo}
                />
                <p className="px-1 text-[10px] text-white/25">
                  Se descarga solo el audio y se convierte a MP3 antes de guardarlo en la ruta.
                </p>
              </div>
            )}

            {modo === 'archivo' && archivo && (
              <div className="flex items-center gap-3 rounded-2xl border border-white/10 bg-white/[0.03] px-4 py-3">
                <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-xl border border-white/10 bg-white/[0.04]">
                  <IconMusic className="h-4 w-4 text-white/40" />
                </span>
                <div className="min-w-0">
                  <p className="truncate text-xs font-medium text-white/80">{archivo.name}</p>
                  <p className="text-[10px] text-white/30">{tamanoLegible(archivo.size)}</p>
                </div>
              </div>
            )}

            {modo === 'youtube' && url.trim() && (
              <div
                className={`flex items-start gap-2 rounded-2xl border px-4 py-3 text-[11px] ${
                  ytValido
                    ? 'border-white/10 bg-white/[0.02] text-white/45'
                    : 'border-amber-500/30 bg-amber-500/10 text-amber-100'
                }`}
              >
                <IconLink className="mt-0.5 h-3.5 w-3.5 shrink-0" />
                <span className="break-all">
                  {ytValido
                    ? 'Enlace válido: se descargará el audio y se registrará la pista con sus metadatos.'
                    : 'El enlace no es de YouTube. Solo se admiten youtube.com y youtu.be.'}
                </span>
              </div>
            )}
          </>
        )}

        <div className="flex justify-end gap-3 pt-2">
          <Button variant="ghost" onClick={onClose} disabled={subiendo}>
            Cancelar
          </Button>
          <Button
            icon={modo === 'youtube' ? IconLink : IconUpload}
            onClick={subir}
            loading={subiendo}
            disabled={sinRutas || cargandoRutas || (modo === 'archivo' ? !archivo : !url.trim())}
          >
            {etiquetaBoton}
          </Button>
        </div>
      </div>
    </Modal>
  )
}

export default SubirPistaModal
