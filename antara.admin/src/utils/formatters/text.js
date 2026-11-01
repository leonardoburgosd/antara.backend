/**
 * Normaliza una cadena quitando diacríticos, acentos y pasando a minúsculas.
 * @param {string} valor
 * @returns {string}
 */
export const normalizeText = (valor) =>
  String(valor ?? '')
    .normalize('NFD')
    .replace(/\p{Diacritic}/gu, '')
    .toLowerCase()
    .trim()

export const normalizeString = normalizeText

/**
 * Obtiene las iniciales de un nombre (máximo 2 caracteres).
 * @param {string} nombre
 * @param {string} fallback
 * @returns {string}
 */
export const getInitials = (nombre = '', fallback = 'AD') => {
  const partes = String(nombre).trim().split(/\s+/).filter(Boolean)
  if (partes.length === 0) return fallback
  if (partes.length === 1) return partes[0].slice(0, 2).toUpperCase()
  return `${partes[0][0]}${partes[partes.length - 1][0]}`.toUpperCase()
}

/**
 * Trunca un texto si supera el límite especificado.
 * @param {string} texto
 * @param {number} limite
 * @returns {string}
 */
export const truncateText = (texto, limite = 30) => {
  if (!texto) return ''
  return texto.length <= limite ? texto : `${texto.slice(0, limite)}...`
}

