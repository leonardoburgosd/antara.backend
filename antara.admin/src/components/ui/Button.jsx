import { IconRefresh } from './Icons'

export const Button = ({
  children,
  variant = 'primary',
  size = 'md',
  loading = false,
  isLoading,
  disabled = false,
  icon: Icon,
  className = '',
  type = 'button',
  ...props
}) => {
  const isActuallyLoading = isLoading !== undefined ? isLoading : loading
  const baseClasses =
    'inline-flex items-center justify-center font-medium transition rounded-full focus:outline-none disabled:opacity-40 disabled:cursor-not-allowed select-none'

  const sizeClasses = {
    sm: 'px-3 py-1.5 text-xs gap-1.5',
    md: 'px-4 py-2.5 text-sm gap-2',
    lg: 'px-6 py-4 text-base gap-2.5',
  }[size] || 'px-4 py-2.5 text-sm gap-2'

  const variantClasses = {
    primary: 'bg-white text-black hover:bg-white/85 shadow-sm active:scale-[0.98]',
    secondary: 'bg-white/10 text-white hover:bg-white/20 active:scale-[0.98]',
    outline: 'border border-white/15 text-white/70 hover:border-white/35 hover:text-white hover:bg-white/5 active:scale-[0.98]',
    danger: 'border border-red-500/30 bg-red-500/10 text-red-200 hover:bg-red-500/20 hover:border-red-500/50 active:scale-[0.98]',
    ghost: 'text-white/50 hover:text-white hover:bg-white/5',
  }[variant] || 'bg-white text-black hover:bg-white/85'

  return (
    <button
      type={type}
      disabled={disabled || isActuallyLoading}
      className={`${baseClasses} ${sizeClasses} ${variantClasses} ${className}`}
      {...props}
    >
      {isActuallyLoading ? (
        <IconRefresh className="h-4 w-4 animate-spin shrink-0" />
      ) : Icon ? (
        <Icon className="h-4 w-4 shrink-0" />
      ) : null}
      {children}
    </button>
  )
}

export default Button

