import { useMemo } from 'react'

export const AmbientBackground = () => {
  const lines = useMemo(
    () =>
      Array.from({ length: 14 }).map(() => ({
        x1: Math.random() * 100,
        y1: Math.random() * 100,
        x2: Math.random() * 100,
        y2: Math.random() * 100,
        delay: Math.random() * 6,
        duration: 8 + Math.random() * 8,
      })),
    [],
  )

  const dots = useMemo(
    () =>
      Array.from({ length: 24 }).map(() => ({
        cx: Math.random() * 100,
        cy: Math.random() * 100,
        r: 0.2 + Math.random() * 0.5,
        delay: Math.random() * 5,
        duration: 3 + Math.random() * 4,
      })),
    [],
  )

  return (
    <div className="pointer-events-none fixed inset-0 overflow-hidden">
      <div className="absolute -left-40 -top-40 h-[28rem] w-[28rem] rounded-full bg-white/[0.03] blur-3xl" />
      <div className="absolute -bottom-52 -right-40 h-[34rem] w-[34rem] rounded-full bg-white/[0.02] blur-3xl" />
      <svg
        className="absolute inset-0 h-full w-full opacity-[0.06]"
        viewBox="0 0 100 100"
        preserveAspectRatio="none"
        aria-hidden="true"
      >
        {lines.map((line, index) => (
          <line
            key={`line-${index}`}
            x1={line.x1}
            y1={line.y1}
            x2={line.x2}
            y2={line.y2}
            stroke="white"
            strokeWidth="0.08"
            className="animate-float"
            style={{ animationDelay: `${line.delay}s`, animationDuration: `${line.duration}s` }}
          />
        ))}
        {dots.map((dot, index) => (
          <circle
            key={`dot-${index}`}
            cx={dot.cx}
            cy={dot.cy}
            r={dot.r}
            fill="white"
            className="animate-pulse"
            style={{ animationDelay: `${dot.delay}s`, animationDuration: `${dot.duration}s` }}
          />
        ))}
      </svg>
    </div>
  )
}

export default AmbientBackground

