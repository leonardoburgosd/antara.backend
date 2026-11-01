import { useState } from 'react'
import { useRutasAudio } from '../hooks/useRutasAudio'
import { useEscaneoAudio } from '../hooks/useEscaneoAudio'

import EscaneoPanel from '../components/EscaneoPanel'
import PruebaPanel from '../components/PruebaPanel'
import RutaCard from '../components/RutaCard'
import RutaFormModal from '../components/RutaFormModal'
import DeleteRutaModal from '../components/DeleteRutaModal'
import { ErrorBanner } from '../../../components/ui/ErrorBanner'

import { IconFolder, IconPlus, IconRefresh } from '../../../components/ui/Icons'

const FORM_VACIO = {
  nombre: '',
  rutaFisica: '',
  rutaContenedor: '',
  descripcion: '',
  estaActiva: true,
  esPredeterminada: false,
}

export const RutasAudioPage = ({ onToast = () => {} }) => {
  const {
    rutas,
    loading,
    error,
    cargar,
    guardar,
    alternarActiva,
    marcarPredeterminada,
    probar,
    eliminar,
    prueba,
    setPrueba,
    procesandoId,
    probandoId,
    guardando,
    eliminando,
  } = useRutasAudio({ onToast })

  const {
    escaneo,
    iniciando: iniciandoEscaneo,
    cancelando: cancelandoEscaneo,
    error: escaneoError,
    iniciarEscaneo,
    cancelarEscaneo,
  } = useEscaneoAudio({ onToast })

  const [modalAbierto, setModalAbierto] = useState(false)
  const [rutaEditando, setRutaEditando] = useState(null)
  const [form, setForm] = useState(FORM_VACIO)
  const [formError, setFormError] = useState('')

  const [rutaAEliminar, setRutaAEliminar] = useState(null)

  const abrirCrear = () => {
    setRutaEditando(null)
    setForm(FORM_VACIO)
    setFormError('')
    setModalAbierto(true)
  }

  const abrirEditar = (ruta) => {
    setRutaEditando(ruta)
    setForm({
      nombre: ruta.nombre || '',
      rutaFisica: ruta.rutaFisica || '',
      rutaContenedor: ruta.rutaContenedor || '',
      descripcion: ruta.descripcion || '',
      estaActiva: Boolean(ruta.estaActiva),
      esPredeterminada: Boolean(ruta.esPredeterminada),
    })
    setFormError('')
    setModalAbierto(true)
  }

  const cerrarModal = () => {
    if (guardando) return
    setModalAbierto(false)
    setFormError('')
  }

  const cambiarCampo = (evento) => {
    const { name, value, type, checked } = evento.target
    setForm((actual) => ({ ...actual, [name]: type === 'checkbox' ? checked : value }))
  }

  const handleGuardar = async (evento) => {
    evento.preventDefault()
    setFormError('')

    if (!form.nombre.trim()) {
      setFormError('Ingresa un nombre para la ruta.')
      return
    }

    if (!form.rutaFisica.trim() && !form.rutaContenedor.trim()) {
      setFormError('Ingresa la ruta física, la ruta del contenedor o ambas.')
      return
    }

    try {
      await guardar(form, rutaEditando)
      setModalAbierto(false)
    } catch (err) {
      setFormError(err.message || 'Error al guardar la ruta.')
    }
  }

  const handleEliminar = async () => {
    if (!rutaAEliminar) return
    try {
      await eliminar(rutaAEliminar)
      setRutaAEliminar(null)
    } catch {
      // Ignored
    }
  }

  return (
    <section className="space-y-5">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <p className="text-[10px] uppercase tracking-[0.35em] text-white/35">Biblioteca</p>
          <h2 className="mt-2 text-2xl font-semibold tracking-tight sm:text-3xl">Rutas de audio</h2>
          <p className="mt-2 max-w-2xl text-sm leading-6 text-white/40">
            Administra las carpetas donde la API busca y guarda los archivos de audio. Solo los
            administradores con token válido pueden modificarlas.
          </p>
        </div>
        <div className="flex flex-wrap gap-2">
          <button
            type="button"
            onClick={cargar}
            disabled={loading}
            className="flex items-center gap-2 rounded-full border border-white/15 px-4 py-2.5 text-sm text-white/60 transition hover:border-white/30 hover:text-white disabled:opacity-40"
          >
            <IconRefresh className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
            Actualizar
          </button>
          <button
            type="button"
            onClick={abrirCrear}
            className="flex items-center gap-2 rounded-full bg-white px-4 py-2.5 text-sm font-medium text-black transition hover:bg-white/85"
          >
            <IconPlus className="h-4 w-4" />
            Nueva ruta
          </button>
        </div>
      </div>

      <div className="rounded-2xl border border-white/10 bg-white/[0.02] px-4 py-3 text-xs leading-5 text-white/40">
        <p>
          <span className="text-white/65">Ruta física:</span> carpeta real en el equipo host (por
          ejemplo <span className="text-white/60">C:/Users/TuUsuario/Music</span>). Las unidades
          montadas se traducen automáticamente dentro del contenedor:
          <span className="text-white/60"> C:/ → /host/c</span>.
        </p>
        <p>
          <span className="text-white/65">Ruta en el contenedor:</span> es opcional. Solo úsala si
          quieres apuntar a un montaje propio (por ejemplo <span className="text-white/60">/music</span>);
          si la dejas vacía, la API detecta la carpeta a partir de la ruta física.
        </p>
      </div>

      <EscaneoPanel
        escaneo={escaneo}
        iniciando={iniciandoEscaneo}
        cancelando={cancelandoEscaneo}
        error={escaneoError}
        onIniciar={iniciarEscaneo}
        onCancelar={cancelarEscaneo}
      />

      {error && <ErrorBanner message={error} onRetry={cargar} />}

      {loading ? (
        <div className="grid gap-4 xl:grid-cols-2">
          {Array.from({ length: 4 }).map((_, indice) => (
            <div key={indice} className="rounded-[1.75rem] border border-white/10 bg-white/[0.03] p-5">
              <div className="flex items-center gap-3">
                <div className="h-10 w-10 animate-pulse rounded-2xl bg-white/[0.06]" />
                <div className="space-y-2">
                  <div className="h-3 w-32 animate-pulse rounded bg-white/10" />
                  <div className="h-2 w-20 animate-pulse rounded bg-white/[0.06]" />
                </div>
              </div>
              <div className="mt-5 space-y-3">
                <div className="h-2.5 w-full animate-pulse rounded bg-white/[0.06]" />
                <div className="h-2.5 w-4/5 animate-pulse rounded bg-white/[0.05]" />
              </div>
            </div>
          ))}
        </div>
      ) : rutas.length === 0 ? (
        <div className="flex flex-col items-center justify-center rounded-[2rem] border border-dashed border-white/10 bg-white/[0.01] px-6 py-16 text-center">
          <div className="flex h-14 w-14 items-center justify-center rounded-full border border-white/10 bg-white/[0.03]">
            <IconFolder className="h-6 w-6 text-white/35" />
          </div>
          <p className="mt-4 text-sm font-medium text-white/60">Aún no hay rutas registradas</p>
          <p className="mt-1 max-w-sm text-xs leading-5 text-white/30">
            Crea la primera ruta para indicarle a la API dónde están tus archivos de audio.
          </p>
          <button
            type="button"
            onClick={abrirCrear}
            className="mt-5 flex items-center gap-2 rounded-full bg-white px-4 py-2.5 text-sm font-medium text-black transition hover:bg-white/85"
          >
            <IconPlus className="h-4 w-4" />
            Nueva ruta
          </button>
        </div>
      ) : (
        <div className="grid gap-4 xl:grid-cols-2">
          {rutas.map((ruta) => (
            <RutaCard
              key={ruta.id}
              ruta={ruta}
              procesando={procesandoId === ruta.id}
              probando={probandoId === ruta.id}
              onProbar={probar}
              onEditar={abrirEditar}
              onAlternarActiva={alternarActiva}
              onMarcarPredeterminada={marcarPredeterminada}
              onEliminar={setRutaAEliminar}
            />
          ))}
        </div>
      )}

      <RutaFormModal
        isOpen={modalAbierto}
        onClose={cerrarModal}
        rutaEditando={rutaEditando}
        form={form}
        onChange={cambiarCampo}
        onSubmit={handleGuardar}
        formError={formError}
        guardando={guardando}
      />

      <PruebaPanel prueba={prueba} onClose={() => setPrueba(null)} />

      <DeleteRutaModal
        ruta={rutaAEliminar}
        onClose={() => setRutaAEliminar(null)}
        onConfirm={handleEliminar}
        eliminando={eliminando}
      />
    </section>
  )
}

export default RutasAudioPage

