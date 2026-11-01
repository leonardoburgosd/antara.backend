import Modal from './Modal'
import Button from './Button'

export const ConfirmDialog = ({
  isOpen,
  onClose,
  onConfirm,
  title = '¿Confirmar acción?',
  message = 'Esta acción no se puede deshacer.',
  confirmLabel = 'Confirmar',
  cancelLabel = 'Cancelar',
  isLoading = false,
  variant = 'danger',
}) => {
  return (
    <Modal isOpen={isOpen} onClose={onClose} title={title} size="sm">
      <div className="space-y-5">
        <p className="text-white/70 text-sm leading-relaxed">{message}</p>
        <div className="flex gap-3 justify-end pt-2">
          <Button variant="ghost" onClick={onClose} disabled={isLoading}>
            {cancelLabel}
          </Button>
          <Button
            variant={variant === 'danger' ? 'danger' : 'primary'}
            onClick={onConfirm}
            loading={isLoading}
          >
            {confirmLabel}
          </Button>
        </div>
      </div>
    </Modal>
  )
}

export default ConfirmDialog

