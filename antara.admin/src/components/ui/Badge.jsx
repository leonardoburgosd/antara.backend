/**
 * Badge / chip de estado.
 */
export const Badge = ({
  variant = 'default',
  size = 'md',
  children,
  className = '',
  icon: Icon,
}) => {
  const variants = {
    default: 'bg-white/10 text-white/70 border border-white/10',
    success: 'bg-emerald-500/10 text-emerald-300 border border-emerald-500/20',
    warning: 'bg-amber-500/10 text-amber-300 border border-amber-500/20',
    danger: 'bg-red-500/10 text-red-300 border border-red-500/20',
    info: 'bg-blue-500/10 text-blue-300 border border-blue-500/20',
    purple: 'bg-purple-500/10 text-purple-300 border border-purple-500/20',
  }

  const sizes = {
    sm: 'text-[10px] px-2 py-0.5 tracking-wider',
    md: 'text-xs px-2.5 py-1',
  }

  return (
    <span
      className={`inline-flex items-center gap-1.5 rounded-full font-medium ${variants[variant] || variants.default} ${sizes[size] || sizes.md} ${className}`}
    >
      {Icon && <Icon className="h-3 w-3 shrink-0" />}
      {children}
    </span>
  )
}

export default Badge

