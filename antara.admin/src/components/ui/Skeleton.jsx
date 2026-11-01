export const Skeleton = ({ className = '', rounded = false }) => {
  return (
    <div
      className={`animate-pulse bg-white/[0.06] ${rounded ? 'rounded-full' : 'rounded-2xl'} ${className}`}
    />
  )
}

export const SkeletonRow = ({ cols = 4 }) => {
  return (
    <div className="flex items-center gap-4 py-3">
      {Array.from({ length: cols }).map((_, i) => (
        <Skeleton key={i} className="h-4 flex-1" />
      ))}
    </div>
  )
}

export const SkeletonTable = ({ rows = 6, cols = 4 }) => {
  return (
    <div className="space-y-2">
      {Array.from({ length: rows }).map((_, i) => (
        <SkeletonRow key={i} cols={cols} />
      ))}
    </div>
  )
}

export default Skeleton

