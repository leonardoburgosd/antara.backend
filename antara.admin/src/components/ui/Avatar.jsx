import { useState } from 'react'

export const Avatar = ({ src, alt, name = '', size = 'md', className = '' }) => {
  const [imgError, setImgError] = useState(false)

  const sizes = {
    xs: 'w-6 h-6 text-[10px]',
    sm: 'w-8 h-8 text-xs',
    md: 'w-10 h-10 text-sm',
    lg: 'w-12 h-12 text-base',
    xl: 'w-16 h-16 text-lg',
  }

  const initials = name
    ? name
        .split(' ')
        .filter(Boolean)
        .slice(0, 2)
        .map((w) => w[0])
        .join('')
        .toUpperCase()
    : '?'

  if (src && !imgError) {
    return (
      <img
        src={src}
        alt={alt || name}
        className={`rounded-full object-cover shrink-0 border border-white/10 ${sizes[size] || sizes.md} ${className}`}
        onError={() => setImgError(true)}
      />
    )
  }

  return (
    <div
      className={`rounded-full bg-white/10 border border-white/10 flex items-center justify-center shrink-0 font-medium text-white/70 select-none ${
        sizes[size] || sizes.md
      } ${className}`}
      aria-label={name}
    >
      {initials}
    </div>
  )
}

export default Avatar

