export const Select = ({
  label,
  error,
  options = [],
  className = '',
  containerClassName = '',
  children,
  ...props
}) => {
  return (
    <div className={`space-y-1.5 ${containerClassName}`}>
      {label && (
        <label className="block text-[10px] uppercase tracking-[0.2em] text-white/40 font-medium">
          {label}
        </label>
      )}
      <div className="relative">
        <select
          className={`w-full rounded-full border border-white/10 bg-black/40 px-5 py-3.5 text-sm text-white transition focus:border-white focus:outline-none appearance-none ${
            error ? 'border-red-500/50 focus:border-red-400' : ''
          } ${className}`}
          {...props}
        >
          {children ||
            options.map((opt) => (
              <option key={opt.value} value={opt.value} className="bg-[#121212] text-white">
                {opt.label}
              </option>
            ))}
        </select>
        <div className="pointer-events-none absolute right-4 top-1/2 -translate-y-1/2 text-white/40 text-xs">
          ▼
        </div>
      </div>
      {error && <p className="text-xs text-red-400 px-3">{error}</p>}
    </div>
  )
}

export default Select


