import { IconClose, IconRefresh } from '../../../components/ui/Icons'

const Campo = ({ etiqueta, ayuda, obligatorio = false, children }) => {
  return (
    <label className="block">
      <span className="mb-1.5 flex items-center gap-1 text-[10px] uppercase tracking-[0.2em] text-white/35">
        {etiqueta}
        {obligatorio && <span className="text-white/50">*</span>}
      </span>
      {children}
      {ayuda && <span className="mt-1 block text-[10px] leading-4 text-white/25">{ayuda}</span>}
    </label>
  )
}

export const RutaFormModal = ({
  isOpen,
  onClose,
  rutaEditando,
  form,
  onChange,
  onSubmit,
  formError,
  guardando,
}) => {
  if (!isOpen) return null

  return (
    <div className="fixed inset-0 z-50 flex items-end justify-center bg-black/70 p-4 backdrop-blur-sm sm:items-center">
      <div className="w-full max-w-lg rounded-[2rem] border border-white/15 bg-[#0a0a0a] p-6 shadow-2xl">
        <div className="flex items-start justify-between gap-4">
          <div>
            <p className="text-[10px] uppercase tracking-[0.35em] text-white/35">
              {rutaEditando ? 'Edición' : 'Nueva ruta'}
            </p>
            <h3 className="mt-2 text-xl font-semibold tracking-tight">
              {rutaEditando ? 'Editar ruta de audio' : 'Agregar ruta de audio'}
            </h3>
          </div>
          <button
            type="button"
            onClick={onClose}
            disabled={guardando}
            className="flex h-9 w-9 items-center justify-center rounded-full border border-white/10 text-white/40 transition hover:border-white/25 hover:text-white"
            aria-label="Cerrar"
          >
            <IconClose className="h-4 w-4" />
          </button>
        </div>

        <form onSubmit={onSubmit} className="mt-5 space-y-4">
          <Campo etiqueta="Nombre" obligatorio>
            <input
              type="text"
              name="nombre"
              value={form.nombre}
              onChange={onChange}
              placeholder="Colección personal"
              maxLength={100}
              required
              className="w-full rounded-2xl border border-white/10 bg-white/[0.03] px-4 py-3 text-sm text-white placeholder:text-white/25 focus:border-white/30 focus:outline-none"
            />
          </Campo>

          <Campo
            etiqueta="Ruta física (host)"
            ayuda="Ruta real en la máquina anfitriona; por ejemplo C:/Users/TuUsuario/Music"
          >
            <input
              type="text"
              name="rutaFisica"
              value={form.rutaFisica}
              onChange={onChange}
              placeholder="C:/Users/TuUsuario/Music"
              maxLength={500}
              className="w-full rounded-2xl border border-white/10 bg-white/[0.03] px-4 py-3 font-mono text-sm text-white placeholder:text-white/25 focus:border-white/30 focus:outline-none"
            />
          </Campo>

          <Campo
            etiqueta="Ruta en el contenedor (opcional)"
            ayuda="Déjala vacía para detectarla desde la ruta física; por ejemplo /host/c/Users/TuUsuario/Music"
          >
            <input
              type="text"
              name="rutaContenedor"
              value={form.rutaContenedor}
              onChange={onChange}
              placeholder="/music"
              maxLength={500}
              className="w-full rounded-2xl border border-white/10 bg-white/[0.03] px-4 py-3 font-mono text-sm text-white placeholder:text-white/25 focus:border-white/30 focus:outline-none"
            />
          </Campo>

          <Campo etiqueta="Descripción">
            <textarea
              name="descripcion"
              value={form.descripcion}
              onChange={onChange}
              rows={2}
              maxLength={300}
              placeholder="Biblioteca principal de MP3"
              className="w-full resize-none rounded-2xl border border-white/10 bg-white/[0.03] px-4 py-3 text-sm text-white placeholder:text-white/25 focus:border-white/30 focus:outline-none"
            />
          </Campo>

          <div className="flex flex-col gap-2 rounded-2xl border border-white/10 bg-white/[0.02] p-3.5">
            <label className="flex cursor-pointer items-center justify-between gap-3 text-sm text-white/60">
              <span>Ruta activa</span>
              <input
                type="checkbox"
                name="estaActiva"
                checked={form.estaActiva}
                onChange={onChange}
                className="h-4 w-4 accent-white"
              />
            </label>
            <label className="flex cursor-pointer items-center justify-between gap-3 text-sm text-white/60">
              <span>Usar como predeterminada</span>
              <input
                type="checkbox"
                name="esPredeterminada"
                checked={form.esPredeterminada}
                onChange={onChange}
                className="h-4 w-4 accent-white"
              />
            </label>
          </div>

          {formError && (
            <div className="rounded-2xl border border-red-400/20 bg-red-400/[0.06] px-4 py-3 text-xs text-red-100">
              {formError}
            </div>
          )}

          <div className="flex flex-col-reverse gap-2 pt-2 sm:flex-row sm:justify-end">
            <button
              type="button"
              onClick={onClose}
              disabled={guardando}
              className="rounded-full border border-white/15 px-5 py-2.5 text-sm text-white/60 transition hover:border-white/30 hover:text-white disabled:opacity-40"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={guardando}
              className="flex items-center justify-center gap-2 rounded-full bg-white px-5 py-2.5 text-sm font-medium text-black transition hover:bg-white/85 disabled:opacity-50"
            >
              {guardando && <IconRefresh className="h-4 w-4 animate-spin" />}
              {guardando ? 'Guardando...' : rutaEditando ? 'Guardar cambios' : 'Crear ruta'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

export default RutaFormModal

