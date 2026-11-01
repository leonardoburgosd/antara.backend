import {
  IconScan,
  IconUpload,
  IconDisc,
  IconUserPlus,
  IconPlaylist,
  IconArrowRight,
} from '../../../components/ui/Icons'

const DEFAULT_ACTIONS = [
  {
    id: 'escanear',
    label: 'Escanear biblioteca',
    description: 'Busca pistas nuevas y sus metadatos',
    icon: IconScan,
  },
  {
    label: 'Subir pista',
    description: 'Agrega audio al catálogo',
    icon: IconUpload,
    message: 'La carga de pistas estará disponible pronto.',
  },
  {
    label: 'Crear álbum',
    description: 'Organiza nuevas colecciones',
    icon: IconDisc,
    message: 'La creación de álbumes estará disponible pronto.',
  },
  {
    label: 'Nuevo usuario',
    description: 'Invita a un nuevo miembro',
    icon: IconUserPlus,
    message: 'La creación de usuarios estará disponible pronto.',
  },
  {
    label: 'Nueva playlist',
    description: 'Cura una selección musical',
    icon: IconPlaylist,
    message: 'La creación de playlists estará disponible pronto.',
  },
]

export const QuickActions = ({ actions = DEFAULT_ACTIONS, onAction }) => {
  return (
    <section className="rounded-[2rem] border border-white/10 bg-white/[0.03] p-6">
      <div>
        <h3 className="text-lg font-semibold">Acciones rápidas</h3>
        <p className="mt-1 text-xs text-white/35">Atajos para gestionar el catálogo.</p>
      </div>
      <div className="mt-4 grid gap-2">
        {actions.map((action) => (
          <button
            key={action.label}
            type="button"
            onClick={() => onAction?.(action)}
            className="group flex items-center justify-between gap-3 rounded-2xl border border-white/10 bg-white/[0.02] px-3 py-3 text-left transition hover:border-white/25 hover:bg-white/[0.05]"
          >
            <span className="flex min-w-0 items-center gap-3">
              <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-xl border border-white/10 bg-white/[0.04]">
                <action.icon className="h-4 w-4 text-white/50" />
              </span>
              <span className="min-w-0">
                <span className="block truncate text-sm font-medium">{action.label}</span>
                <span className="block truncate text-[10px] text-white/30">{action.description}</span>
              </span>
            </span>
            <IconArrowRight className="h-4 w-4 shrink-0 text-white/15 transition group-hover:translate-x-0.5 group-hover:text-white/60" />
          </button>
        ))}
      </div>
    </section>
  )
}

export default QuickActions

