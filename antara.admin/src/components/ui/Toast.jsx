import { useEffect } from 'react'
import { IconCheck, IconClose } from './Icons'

export const Toast = ({ message, type = 'info', onDismiss, duration = 4000 }) => {
  useEffect(() => {
    if (!message) return
    const timer = setTimeout(() => onDismiss?.(), duration)
    return () => clearTimeout(timer)
  }, [message, onDismiss, duration])

  if (!message) return null

  const styles = {
    success: 'bg-emerald-950/90 border-emerald-500/30 text-emerald-200',
    error: 'bg-red-950/90 border-red-500/30 text-red-200',
    warning: 'bg-amber-950/90 border-amber-500/30 text-amber-200',
    info: 'bg-[#181818]/90 border-white/15 text-white',
  }

  return (
    <div className="fixed bottom-6 right-6 z-[100] max-w-sm animate-in fade-in slide-in-from-bottom-3 duration-200">
      <div
        className={`flex items-center gap-3 rounded-2xl border px-4 py-3 shadow-2xl backdrop-blur-md text-sm ${
          styles[type] || styles.info
        }`}
      >
        {type === 'success' && <IconCheck className="h-4 w-4 shrink-0 text-emerald-400" />}
        <span className="flex-1 leading-snug">{message}</span>
        {onDismiss && (
          <button
            onClick={onDismiss}
            className="flex h-5 w-5 items-center justify-center rounded-full text-white/40 hover:text-white transition"
            aria-label="Cerrar notificación"
          >
            <IconClose className="h-3 w-3" />
          </button>
        )}
      </div>
    </div>
  )
}

export default Toast

