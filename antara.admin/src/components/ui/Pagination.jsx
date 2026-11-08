import { TAMANOS_PAGINA } from '../../hooks/usePaginacion'

/**
 * Barra de paginación: rango visible, selector de tamaño y navegación.
 */
export const Pagination = ({
  pagina = 1,
  totalPaginas = 1,
  desde = 0,
  hasta = 0,
  total = 0,
  tamano = 10,
  onPagina = () => {},
  onTamano = () => {},
  className = '',
}) => {
  return (
    <div
      className={`flex flex-col gap-3 rounded-2xl border border-white/10 bg-white/[0.02] px-4 py-3 sm:flex-row sm:items-center sm:justify-between ${className}`}
    >
      <div className="flex items-center gap-2 text-[11px] text-white/40">
        <span>
          Mostrando <span className="text-white/70">{desde}</span>–
          <span className="text-white/70">{hasta}</span> de{' '}
          <span className="text-white/70">{total}</span>
        </span>
      </div>

      <div className="flex flex-wrap items-center gap-3">
        <label className="flex items-center gap-2 text-[10px] uppercase tracking-[0.15em] text-white/30">
          Por página
          <select
            value={tamano}
            onChange={(evento) => onTamano(Number(evento.target.value))}
            className="rounded-full border border-white/10 bg-black/40 px-3 py-1.5 text-xs text-white/70 transition focus:border-white focus:outline-none"
            aria-label="Elementos por página"
          >
            {TAMANOS_PAGINA.map((opcion) => (
              <option key={opcion} value={opcion} className="bg-[#121212] text-white">
                {opcion}
              </option>
            ))}
          </select>
        </label>

        <div className="flex items-center gap-1.5">
          <button
            type="button"
            onClick={() => onPagina(pagina - 1)}
            disabled={pagina <= 1}
            className="flex h-8 items-center rounded-full border border-white/10 px-3 text-xs text-white/60 transition hover:border-white/30 hover:text-white disabled:cursor-not-allowed disabled:opacity-30"
            aria-label="Página anterior"
          >
            Anterior
          </button>
          <span className="px-1 text-[11px] text-white/40">
            Página <span className="text-white/70">{pagina}</span> de {totalPaginas}
          </span>
          <button
            type="button"
            onClick={() => onPagina(pagina + 1)}
            disabled={pagina >= totalPaginas}
            className="flex h-8 items-center rounded-full border border-white/10 px-3 text-xs text-white/60 transition hover:border-white/30 hover:text-white disabled:cursor-not-allowed disabled:opacity-30"
            aria-label="Página siguiente"
          >
            Siguiente
          </button>
        </div>
      </div>
    </div>
  )
}

export default Pagination
