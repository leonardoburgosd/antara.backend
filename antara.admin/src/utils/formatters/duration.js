/**
 * Formatea una duración en segundos a minutos:segundos (ej. 215 -> "3:35").
 * @param {number|string} segundos
 * @returns {string}
 */
export const formatDuration = (segundos) => {
  const total = Math.max(0, Math.round(Number(segundos) || 0))
  if (total === 0) return '—'
  const minutos = Math.floor(total / 60)
  const resto = total % 60
  return `${minutos}:${String(resto).padStart(2, '0')}`
}

