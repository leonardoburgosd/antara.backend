import { IconScan, IconClose, IconMusic } from '../../../components/ui/Icons'

const ESTADOS_ESCANEO = {
  en_cola: 'En cola',
  descubriendo: 'Descubriendo archivos',
  procesando: 'Procesando canciones',
  completado: 'Completado',
  cancelado: 'Cancelado',
  error: 'Error',
}

const MetricEscaneo = ({ label, value }) => {
  return (
    <div className="rounded-2xl border border-white/5 bg-black/20 px-3 py-2">
      <p className="text-[9px] uppercase tracking-[0.15em] text-white/30">{label}</p>
      <p className="mt-0.5 text-sm font-semibold">{value ?? 0}</p>
    </div>
  )
}

export const EscaneoPanel = ({
  escaneo,
  iniciando,
  cancelando,
  error,
  onIniciar,
  onCancelar,
}) => {
  const activo = Boolean(escaneo?.activo)
  const total = escaneo?.totalArchivos ?? 0
  const resueltos = (escaneo?.omitidos ?? 0) + (escaneo?.procesados ?? 0)
  const avance =
    activo && total === 0
      ? 6
      : total > 0
        ? Math.min(100, Math.round((resueltos / total) * 100))
        : escaneo
          ? 100
          : 0
  const resultados = escaneo?.resultados ?? []

  return (
    <section className="overflow-hidden rounded-[2rem] border border-white/10 bg-white/[0.03]">
      <div className="flex flex-col gap-4 p-5 sm:flex-row sm:items-start sm:justify-between sm:p-6">
        <div className="flex items-start gap-3">
          <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-2xl border border-white/10 bg-white/[0.04]">
            <IconScan className={`h-5 w-5 text-white/60 ${activo ? 'animate-pulse' : ''}`} />
          </div>
          <div>
            <p className="text-[10px] uppercase tracking-[0.35em] text-white/35">Biblioteca</p>
            <h3 className="mt-1 text-lg font-semibold tracking-tight">Escanear rutas de audio</h3>
            <p className="mt-1 max-w-xl text-xs leading-5 text-white/40">
              Detecta archivos nuevos, consulta Deezer y lyrics.ovh y registra las pistas con
              artista, álbum, portada, año, género y letra. Se procesan hasta{' '}
              {escaneo?.maxParalelo || 4} canciones a la vez.
            </p>
          </div>
        </div>

        <div className="flex shrink-0 gap-2">
          {activo ? (
            <button
              type="button"
              onClick={onCancelar}
              disabled={cancelando}
              className="flex items-center gap-2 rounded-full border border-white/15 px-4 py-2.5 text-sm text-white/60 transition hover:border-white/30 hover:text-white disabled:opacity-40"
            >
              <IconClose className="h-4 w-4" />
              {cancelando ? 'Cancelando...' : 'Cancelar'}
            </button>
          ) : (
            <button
              type="button"
              onClick={onIniciar}
              disabled={iniciando}
              className="flex items-center gap-2 rounded-full bg-white px-4 py-2.5 text-sm font-medium text-black transition hover:bg-white/85 disabled:opacity-50"
            >
              <IconScan className="h-4 w-4" />
              {iniciando ? 'Iniciando...' : 'Escanear rutas'}
            </button>
          )}
        </div>
      </div>

      {error && (
        <div className="mx-5 mb-5 rounded-2xl border border-red-400/20 bg-red-400/[0.06] px-4 py-3 text-xs text-red-100 sm:mx-6">
          {error}
        </div>
      )}

      {escaneo && (
        <div className="border-t border-white/5 px-5 py-5 sm:px-6">
          <div className="flex flex-wrap items-center justify-between gap-2 text-xs">
            <span className="flex min-w-0 items-center gap-2 text-white/50">
              <span
                className={`h-1.5 w-1.5 shrink-0 rounded-full ${
                  activo
                    ? 'animate-pulse bg-emerald-400'
                    : escaneo.estado === 'error'
                      ? 'bg-red-400'
                      : 'bg-white/40'
                }`}
              />
              <span className="shrink-0">{ESTADOS_ESCANEO[escaneo.estado] || escaneo.estado}</span>
              {escaneo.archivoActual && (
                <span className="truncate font-mono text-[10px] text-white/30">
                  · {escaneo.archivoActual}
                </span>
              )}
            </span>
            <span className="text-white/30">{avance}%</span>
          </div>

          <div className="mt-2 h-1.5 w-full overflow-hidden rounded-full bg-white/10">
            <div
              className="h-full rounded-full bg-white transition-all duration-500"
              style={{ width: `${avance}%` }}
            />
          </div>

          <div className="mt-4 grid grid-cols-2 gap-2 sm:grid-cols-5">
            <MetricEscaneo label="Archivos" value={escaneo.totalArchivos} />
            <MetricEscaneo label="Nuevas" value={escaneo.nuevos} />
            <MetricEscaneo label="Sin metadatos" value={escaneo.sinMetadatos} />
            <MetricEscaneo label="Omitidas" value={escaneo.omitidos} />
            <MetricEscaneo label="Errores" value={escaneo.errores} />
          </div>

          {escaneo.mensaje && (
            <p className="mt-3 rounded-2xl border border-amber-400/15 bg-amber-400/[0.05] px-3 py-2 text-xs text-amber-100/80">
              {escaneo.mensaje}
            </p>
          )}

          {resultados.length > 0 && (
            <div className="mt-4">
              <p className="text-[10px] uppercase tracking-[0.25em] text-white/30">
                Últimas canciones
              </p>
              <div className="mt-2 grid gap-2 sm:grid-cols-2">
                {resultados.slice(0, 6).map((item, index) => (
                  <div
                    key={`${item.archivo}-${index}`}
                    className="flex items-center gap-3 rounded-2xl border border-white/5 bg-black/30 px-3 py-2"
                  >
                    <div className="h-9 w-9 shrink-0 overflow-hidden rounded-xl border border-white/10 bg-white/[0.04]">
                      {item.portadaUrl ? (
                        <img src={item.portadaUrl} alt="" className="h-full w-full object-cover" />
                      ) : (
                        <div className="flex h-full items-center justify-center">
                          <IconMusic className="h-4 w-4 text-white/25" />
                        </div>
                      )}
                    </div>
                    <div className="min-w-0 flex-1">
                      <p className="truncate text-xs font-medium">{item.nombre}</p>
                      <p className="truncate text-[10px] text-white/30">
                        {item.interprete || 'Sin artista'}
                        {item.album ? ` · ${item.album}` : ''}
                      </p>
                    </div>
                    <div className="shrink-0 text-right">
                      <span
                        className={`rounded-full border px-2 py-0.5 text-[8px] uppercase tracking-[0.12em] ${
                          item.estado === 'nuevo'
                            ? 'border-emerald-400/20 text-emerald-300/80'
                            : item.estado === 'error'
                              ? 'border-red-400/20 text-red-300/80'
                              : 'border-white/10 text-white/35'
                        }`}
                      >
                        {item.estado === 'nuevo'
                          ? item.tieneLetra
                            ? 'Con letra'
                            : 'Nueva'
                          : item.estado === 'error'
                            ? 'Error'
                            : 'Sin datos'}
                      </span>
                      {item.ano > 0 && <p className="mt-1 text-[9px] text-white/25">{item.ano}</p>}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      )}
    </section>
  )
}

export default EscaneoPanel

