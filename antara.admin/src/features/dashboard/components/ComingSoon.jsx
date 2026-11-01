import { IconGrid } from '../../../components/ui/Icons'

export const ComingSoon = ({ item }) => {
  const Icon = item?.icon || IconGrid

  return (
    <div className="flex min-h-[55vh] flex-col items-center justify-center rounded-[2rem] border border-white/10 bg-white/[0.02] px-6 py-16 text-center">
      <div className="flex h-16 w-16 items-center justify-center rounded-full border border-white/10 bg-white/[0.04]">
        <Icon className="h-7 w-7 text-white/50" />
      </div>
      <h2 className="mt-6 text-3xl font-semibold tracking-tight">{item?.label || 'Sección'}</h2>
      <p className="mt-2 max-w-md text-sm leading-6 text-white/35">
        Esta sección está en construcción. Pronto podrás gestionar {item?.label?.toLowerCase() || 'este contenido'} desde
        aquí.
      </p>
    </div>
  )
}

export default ComingSoon

