import { useEffect } from 'react'
import { IconClose } from './Icons'

/**
 * Modal genérico con overlay y backdrop blur.
 */
export const Modal = ({
  isOpen,
  onClose,
  title,
  subtitle,
  children,
  size = 'md',
  closeOnBackdrop = true,
}) => {
  useEffect(() => {
    if (!isOpen) return
    const handleKey = (e) => {
      if (e.key === 'Escape') onClose?.()
    }
    document.addEventListener('keydown', handleKey)
    return () => document.removeEventListener('keydown', handleKey)
  }, [isOpen, onClose])

  if (!isOpen) return null

  const sizeClasses = {
    sm: 'max-w-sm',
    md: 'max-w-md',
    lg: 'max-w-lg',
    xl: 'max-w-2xl',
    '2xl': 'max-w-4xl',
  }[size] || 'max-w-md'

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm"
      onClick={closeOnBackdrop ? onClose : undefined}
    >
      <div
        className={`w-full ${sizeClasses} max-h-[90vh] flex flex-col rounded-3xl border border-white/10 bg-[#121212] shadow-2xl overflow-hidden`}
        onClick={(e) => e.stopPropagation()}
      >
        {title && (
          <div className="flex items-center justify-between border-b border-white/10 px-6 py-5 shrink-0">
            <div>
              <h2 className="text-base font-semibold text-white">{title}</h2>
              {subtitle && <p className="text-xs text-white/40 mt-0.5">{subtitle}</p>}
            </div>
            <button
              onClick={onClose}
              className="flex h-8 w-8 items-center justify-center rounded-full text-white/40 transition hover:bg-white/10 hover:text-white"
              aria-label="Cerrar"
            >
              <IconClose className="h-4 w-4" />
            </button>
          </div>
        )}
        <div className="overflow-y-auto flex-1 px-6 py-5">{children}</div>
      </div>
    </div>
  )
}

export default Modal

