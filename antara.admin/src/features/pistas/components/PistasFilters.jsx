import { IconSearch, IconClose } from '../../../components/ui/Icons'

const FILTROS = [
  { id: 'todas', label: 'Todas' },
  { id: 'letra', label: 'Con letra' },
  { id: 'sin-letra', label: 'Sin letra' },
]

export const PistasFilters = ({
  busqueda,
  onBusquedaChange,
  onLimpiarBusqueda,
  filtro,
  onFiltroChange,
  conteos,
}) => {
  return (
    <div className="flex flex-col gap-3 rounded-[1.5rem] border border-white/10 bg-white/[0.03] p-4 lg:flex-row lg:items-center lg:justify-between">
      <div className="flex flex-1 items-center gap-2 rounded-full border border-white/10 bg-black/30 px-4 py-2.5">
        <IconSearch className="h-4 w-4 shrink-0 text-white/30" />
        <input
          type="search"
          value={busqueda}
          onChange={(e) => onBusquedaChange(e.target.value)}
          placeholder="Buscar por canción, artista, álbum o género..."
          className="w-full bg-transparent text-sm text-white placeholder:text-white/25 focus:outline-none"
        />
        {busqueda && (
          <button
            type="button"
            onClick={onLimpiarBusqueda}
            className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full text-white/35 transition hover:bg-white/10 hover:text-white"
            aria-label="Limpiar búsqueda"
          >
            <IconClose className="h-3.5 w-3.5" />
          </button>
        )}
      </div>

      <div className="flex shrink-0 gap-1 overflow-x-auto rounded-full border border-white/10 bg-black/30 p-1">
        {FILTROS.map((item) => {
          const activo = filtro === item.id
          return (
            <button
              key={item.id}
              type="button"
              onClick={() => onFiltroChange(item.id)}
              className={`flex shrink-0 items-center gap-1.5 rounded-full px-3 py-1.5 text-[11px] transition ${
                activo ? 'bg-white text-black' : 'text-white/45 hover:text-white'
              }`}
            >
              {item.label}
              <span className={`text-[9px] ${activo ? 'text-black/50' : 'text-white/25'}`}>
                {conteos[item.id]}
              </span>
            </button>
          )
        })}
      </div>
    </div>
  )
}

export default PistasFilters

