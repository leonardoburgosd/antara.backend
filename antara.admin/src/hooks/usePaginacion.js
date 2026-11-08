import { useEffect, useMemo, useState } from 'react'

export const TAMANOS_PAGINA = [5, 10, 25, 50]

/**
 * Pagina una lista en el cliente.
 * @param {Array} items
 * @param {{ tamanoInicial?: number, resetDeps?: Array }} options
 *   `resetDeps` vuelve a la página 1 cuando cambian (búsqueda, filtros, etc.).
 */
export function usePaginacion(items = [], { tamanoInicial = 10, resetDeps = [] } = {}) {
  const [pagina, setPagina] = useState(1)
  const [tamano, setTamano] = useState(tamanoInicial)

  const total = items.length
  const totalPaginas = Math.max(1, Math.ceil(total / tamano))
  const claveReset = resetDeps.join('|')

  // Si cambian los filtros o se borran elementos, la página actual puede quedar fuera.
  useEffect(() => {
    setPagina(1)
  }, [claveReset])

  useEffect(() => {
    setPagina((actual) => (actual > totalPaginas ? totalPaginas : actual))
  }, [totalPaginas])

  const visibles = useMemo(() => {
    const inicio = (pagina - 1) * tamano
    return items.slice(inicio, inicio + tamano)
  }, [items, pagina, tamano])

  const cambiarTamano = (nuevoTamano) => {
    const valor = Number(nuevoTamano)
    if (!Number.isFinite(valor) || valor <= 0) return
    setTamano(valor)
    setPagina(1)
  }

  const desde = total === 0 ? 0 : (pagina - 1) * tamano + 1
  const hasta = Math.min(pagina * tamano, total)

  return {
    pagina,
    tamano,
    totalPaginas,
    total,
    desde,
    hasta,
    visibles,
    setPagina,
    cambiarTamano,
  }
}

export default usePaginacion
