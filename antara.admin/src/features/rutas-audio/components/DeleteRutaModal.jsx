import { IconTrash, IconRefresh } from '../../../components/ui/Icons'

export const DeleteRutaModal = ({
  ruta,
  onClose,
  onConfirm,
  eliminando,
}) => {
  if (!ruta) return null

  return (
    <div className="fixed inset-0 z-50 flex items-end justify-center bg-black/70 p-4 backdrop-blur-sm sm:items-center">
      <div className="w-full max-w-sm rounded-[2rem] border border-white/15 bg-[#0a0a0a] p-6 shadow-2xl">
        <div className="flex h-11 w-11 items-center justify-center rounded-full border border-red-400/20 bg-red-400/[0.08]">
          <IconTrash className="h-5 w-5 text-red-300" />
        </div>
        <h3 className="mt-4 text-lg font-semibold tracking-tight">Eliminar ruta</h3>
        <p className="mt-2 text-sm leading-6 text-white/40">
          ¿Seguro que quieres eliminar <span className="text-white/70">{ruta.nombre}</span>? Esta
          acción no borra los archivos de audio, solo el registro de la ruta.
        </p>
        <div className="mt-5 flex flex-col-reverse gap-2 sm:flex-row sm:justify-end">
          <button
            type="button"
            onClick={onClose}
            disabled={eliminando}
            className="rounded-full border border-white/15 px-5 py-2.5 text-sm text-white/60 transition hover:border-white/30 hover:text-white disabled:opacity-40"
          >
            Cancelar
          </button>
          <button
            type="button"
            onClick={onConfirm}
            disabled={eliminando}
            className="flex items-center justify-center gap-2 rounded-full bg-red-500 px-5 py-2.5 text-sm font-medium text-white transition hover:bg-red-400 disabled:opacity-50"
          >
            {eliminando && <IconRefresh className="h-4 w-4 animate-spin" />}
            {eliminando ? 'Eliminando...' : 'Eliminar'}
          </button>
        </div>
      </div>
    </div>
  )
}

export default DeleteRutaModal

