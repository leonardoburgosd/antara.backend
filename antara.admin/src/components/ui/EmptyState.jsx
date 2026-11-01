export const EmptyState = ({
  icon: Icon,
  title,
  description,
  action,
  className = '',
}) => {
  return (
    <div className={`flex flex-col items-center justify-center py-16 text-center gap-3 ${className}`}>
      {Icon && (
        <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-white/[0.04] border border-white/5 text-white/30 mb-2">
          <Icon className="h-7 w-7" />
        </div>
      )}
      <div>
        <h3 className="text-sm font-medium text-white/80">{title}</h3>
        {description && (
          <p className="text-xs text-white/40 mt-1 max-w-sm mx-auto leading-relaxed">
            {description}
          </p>
        )}
      </div>
      {action && <div className="mt-2">{action}</div>}
    </div>
  )
}

export default EmptyState

