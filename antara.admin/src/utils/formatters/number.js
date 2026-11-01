/**
 * Formatea un valor numérico según la localización 'es-PE'.
 * @param {number|string} valor
 * @returns {string}
 */
export const formatNumber = (valor) => new Intl.NumberFormat('es-PE').format(Number(valor) || 0)

