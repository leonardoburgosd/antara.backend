/**
 * Formatea una fecha a formato corto legible (ej. "29 oct 2026").
 * @param {string|Date} valor
 * @returns {string}
 */
export const formatDate = (valor) => {
  if (!valor) return '—'
  const fecha = new Date(valor)
  if (Number.isNaN(fecha.getTime())) return '—'
  return fecha.toLocaleDateString('es-PE', { day: '2-digit', month: 'short', year: 'numeric' })
}

/**
 * Devuelve una descripción de tiempo relativo respecto al momento actual.
 * @param {string|Date} valor
 * @returns {string}
 */
export const relativeTime = (valor) => {
  if (!valor) return '—'
  const date = new Date(valor)
  if (Number.isNaN(date.getTime())) return '—'

  const minutes = Math.floor((Date.now() - date.getTime()) / 60000)
  if (minutes < 1) return 'Ahora'
  if (minutes < 60) return `Hace ${minutes} min`

  const hours = Math.floor(minutes / 60)
  if (hours < 24) return `Hace ${hours} h`

  const days = Math.floor(hours / 24)
  if (days < 7) return `Hace ${days} d`

  return formatDate(valor)
}

/**
 * Devuelve la fecha actual formateada para la cabecera (ej. "Jueves, 29 de octubre de 2026").
 * @returns {string}
 */
export const getTodayFormatted = () => {
  const value = new Date().toLocaleDateString('es-PE', {
    weekday: 'long',
    day: 'numeric',
    month: 'long',
    year: 'numeric',
  })
  return value.charAt(0).toUpperCase() + value.slice(1)
}

