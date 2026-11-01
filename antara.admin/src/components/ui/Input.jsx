import { IconClose } from './Icons'

export const Input = ({
  label,
  error,
  icon: Icon,
  onClear,
  className = '',
  containerClassName = '',
  type = 'text',
  value,
  ...props
}) => {
  return (
    <div className={`space-y-1.5 ${containerClassName}`}>
      {label && (
        <label className="block text-[10px] uppercase tracking-[0.2em] text-white/40 font-medium">
          {label}
        </label>
      )}
      <div className="relative flex items-center">
        {Icon && (
          <div className="pointer-events-none absolute left-4 text-white/30">
            <Icon className="h-4 w-4" />
          </div>
        )}
        <input
          type={type}
          value={value}
          className={`w-full rounded-full border border-white/10 bg-black/40 text-sm text-white placeholder:text-white/25 transition focus:border-white focus:outline-none ${
            Icon ? 'pl-11' : 'px-5'
          } ${onClear && value ? 'pr-10' : 'pr-5'} py-3.5 ${
            error ? 'border-red-500/50 focus:border-red-400' : ''
          } ${className}`}
          {...props}
        />
        {onClear && value && (
          <button
            type="button"
            onClick={onClear}
            className="absolute right-3.5 flex h-6 w-6 items-center justify-center rounded-full text-white/35 transition hover:bg-white/10 hover:text-white"
            aria-label="Limpiar campo"
          >
            <IconClose className="h-3.5 w-3.5" />
          </button>
        )}
      </div>
      {error && <p className="text-xs text-red-400 px-3">{error}</p>}
    </div>
  )
}

export default Input


