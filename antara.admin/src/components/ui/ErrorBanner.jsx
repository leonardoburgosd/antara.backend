import { IconInfo } from './Icons'

export const ErrorBanner = ({ message, onRetry, className = '' }) => {
  if (!message) return null
  return (
    <div
      className={`flex items-center gap-3 p-4 bg-red-500/10 border border-red-500/25 rounded-2xl text-red-200 text-sm ${className}`}
    >
      <IconInfo className="h-5 w-5 shrink-0 text-red-400" />
      <span className="flex-1 leading-snug">{message}</span>
      {onRetry && (
        <button
          onClick={onRetry}
          className="text-xs font-medium text-white/80 hover:text-white underline underline-offset-2 shrink-0 transition"
        >
          Reintentar
        </button>
      )}
    </div>
  )
}

export default ErrorBanner

