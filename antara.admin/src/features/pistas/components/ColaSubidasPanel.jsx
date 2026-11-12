import { useEffect, useRef, useState } from 'react'
import { useColaSubidas } from '../context/ColaSubidasContext'
import {
  IconAlertCircle,
  IconCheck,
  IconClose,
  IconLink,
  IconMusic,
  IconRefresh,
  IconUpload,
} from '../../../components/ui/Icons'

const Detalle = ({ item }) => {
  const partes = [item.rutaNombre, item.subcarpeta].filter(Boolean)
  return partes.length ? <span className="truncate">{partes.join(' · ')}</span> : null
}

const Estado = ({ item }) => {
  if (item.estado === 'pendiente') {
    return <span className="text-white/35">En cola</span>
  }

  if (item.estado === 'subiendo') {
    return (
      <span className="flex items-center gap-1.5 text-white/70">
        <span className="h-3 w-3 animate-spin rounded-full border border-white/25 border-t-white" />
        {item.tipo === 'youtube' ? 'Descargando…' : 'Subiendo…'}
      </span>
    )
  }

  if (item.estado === 'completado') {
    return (
      <span className="flex items-center gap-1 text-emerald-300/80">
        <IconCheck className="h-3.5 w-3.5" />
        Lista
      </span>
    )
  }

  return (
    <span className="flex items-center gap-1 text-red-300/80">
      <IconAlertCircle className="h-3.5 w-3.5" />
      Error
    </span>
  )
}

export const ColaSubidasPanel = () => {
  const {
    items,
    pendientes,
    finalizadas,
    cancelar,
    quitar,
    reintentar,
    limpiarFinalizadas,
  } = useColaSubidas()
  const [abierto, setAbierto] = useState(true)
  const longitudAnterior = useRef(0)

  useEffect(() => {
    if (items.length > longitudAnterior.current) setAbierto(true)
    longitudAnterior.current = items.length
  }, [items.length])

  if (!items.length) return null

  if (!abierto) {
    return (
      <button
        type="button"
        onClick={() => setAbierto(true)}
        className="fixed bottom-24 left-4 z-50 flex items-center gap-2 rounded-full border border-white/15 bg-[#121212]/95 px-4 py-3 text-xs font-medium text-white shadow-2xl backdrop-blur transition hover:border-white/30 sm:bottom-6"
      >
        <IconUpload className="h-4 w-4" />
        Subidas
        <span className="flex h-5 min-w-5 items-center justify-center rounded-full bg-white px-1.5 text-[10px] font-semibold text-black">
          {pendientes || items.length}
        </span>
      </button>
    )
  }

  return (
    <aside
      className="fixed bottom-24 left-4 z-50 w-[calc(100%-2rem)] max-w-md rounded-[1.75rem] border border-white/15 bg-[#0a0a0a]/95 shadow-2xl backdrop-blur sm:bottom-6"
      aria-label="Cola de subidas"
    >
      <div className="flex items-start justify-between gap-3 border-b border-white/10 px-4 py-3.5">
        <div>
          <p className="text-sm font-semibold">Cola de subidas</p>
          <p className="mt-0.5 text-[10px] text-white/35">
            {pendientes > 0
              ? `${pendientes} pendiente${pendientes === 1 ? '' : 's'} · se procesan en segundo plano`
              : 'Se procesan en segundo plano'}
          </p>
        </div>
        <div className="flex items-center gap-1">
          {finalizadas > 0 && (
            <button
              type="button"
              onClick={limpiarFinalizadas}
              className="rounded-full px-2 py-1 text-[10px] text-white/40 transition hover:bg-white/10 hover:text-white"
            >
              Limpiar
            </button>
          )}
          <button
            type="button"
            onClick={() => setAbierto(false)}
            className="flex h-7 w-7 items-center justify-center rounded-full text-white/40 transition hover:bg-white/10 hover:text-white"
            aria-label="Ocultar cola de subidas"
          >
            <IconClose className="h-4 w-4" />
          </button>
        </div>
      </div>

      <div className="max-h-[min(28rem,55vh)] space-y-2 overflow-y-auto p-3">
        {items.map((item) => (
          <div
            key={item.id}
            className="rounded-2xl border border-white/10 bg-white/[0.025] px-3 py-2.5"
          >
            <div className="flex items-start gap-2.5">
              <span className="mt-0.5 flex h-8 w-8 shrink-0 items-center justify-center rounded-xl border border-white/10 bg-white/[0.04]">
                {item.tipo === 'youtube' ? (
                  <IconLink className="h-4 w-4 text-white/45" />
                ) : (
                  <IconMusic className="h-4 w-4 text-white/45" />
                )}
              </span>

              <div className="min-w-0 flex-1">
                <p className="truncate text-xs font-medium text-white/85" title={item.nombre}>
                  {item.nombre}
                </p>
                <div className="mt-0.5 flex min-w-0 items-center gap-1.5 text-[10px] text-white/30">
                  <Detalle item={item} />
                </div>
                <div className="mt-1 text-[10px]">
                  <Estado item={item} />
                </div>
                {item.error && (
                  <p className="mt-1 line-clamp-2 text-[10px] leading-4 text-red-200/70" title={item.error}>
                    {item.error}
                  </p>
                )}
              </div>

              <div className="flex shrink-0 items-center gap-1">
                {item.estado === 'pendiente' && (
                  <button
                    type="button"
                    onClick={() => cancelar(item.id)}
                    className="flex h-7 w-7 items-center justify-center rounded-full text-white/35 transition hover:bg-white/10 hover:text-white"
                    aria-label={`Cancelar ${item.nombre}`}
                    title="Cancelar"
                  >
                    <IconClose className="h-3.5 w-3.5" />
                  </button>
                )}
                {item.estado === 'error' && (
                  <button
                    type="button"
                    onClick={() => reintentar(item.id)}
                    className="flex h-7 w-7 items-center justify-center rounded-full text-white/45 transition hover:bg-white/10 hover:text-white"
                    aria-label={`Reintentar ${item.nombre}`}
                    title="Reintentar"
                  >
                    <IconRefresh className="h-3.5 w-3.5" />
                  </button>
                )}
                {(item.estado === 'completado' || item.estado === 'error') && (
                  <button
                    type="button"
                    onClick={() => quitar(item.id)}
                    className="flex h-7 w-7 items-center justify-center rounded-full text-white/30 transition hover:bg-white/10 hover:text-white"
                    aria-label={`Quitar ${item.nombre} de la cola`}
                    title="Quitar de la cola"
                  >
                    <IconClose className="h-3.5 w-3.5" />
                  </button>
                )}
              </div>
            </div>

            {item.estado === 'subiendo' && (
              <div className="mt-2 h-1 overflow-hidden rounded-full bg-white/10">
                <div className="h-full w-2/3 animate-pulse rounded-full bg-white/70" />
              </div>
            )}
          </div>
        ))}
      </div>
    </aside>
  )
}

export default ColaSubidasPanel
