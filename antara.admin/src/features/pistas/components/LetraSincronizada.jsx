import { useEffect, useMemo, useRef } from 'react'

// Marcas de tiempo LRC: [mm:ss], [mm:ss.xx] o [mm:ss.xxx].
const MARCA = /\[(\d{1,2}):(\d{2})(?:[.:](\d{1,3}))?\]/g

/**
 * Convierte una letra LRC en líneas ordenadas por tiempo. Las líneas con varias
 * marcas (coros repetidos) generan una entrada por cada aparición.
 */
export const parsearLrc = (lrc) => {
  if (!lrc) return []

  const lineas = []

  lrc.split('\n').forEach((linea) => {
    MARCA.lastIndex = 0
    const tiempos = []
    let marca

    while ((marca = MARCA.exec(linea)) !== null) {
      const minutos = Number(marca[1])
      const segundos = Number(marca[2])
      const fraccion = marca[3] ? Number(marca[3].padEnd(3, '0').slice(0, 3)) / 1000 : 0
      tiempos.push(minutos * 60 + segundos + fraccion)
    }

    const texto = linea.replace(MARCA, '').trim()
    if (!texto || tiempos.length === 0) return

    tiempos.forEach((tiempo) => lineas.push({ tiempo, texto }))
  })

  return lineas.sort((a, b) => a.tiempo - b.tiempo)
}

export const LetraSincronizada = ({ letra, progreso = 0, duracion = 0, activa = false, onIrA }) => {
  const contenedorRef = useRef(null)
  const lineaRefs = useRef([])
  const lineas = useMemo(() => parsearLrc(letra), [letra])

  const puedeSeguir = activa && duracion > 0
  const indiceActivo = useMemo(() => {
    if (!puedeSeguir || lineas.length === 0) return -1

    let indice = -1
    for (let i = 0; i < lineas.length; i += 1) {
      if (lineas[i].tiempo <= progreso + 0.2) indice = i
      else break
    }
    return indice
  }, [lineas, progreso, puedeSeguir])

  // Mantiene la línea activa centrada sin mover el resto de la página.
  useEffect(() => {
    const contenedor = contenedorRef.current
    const linea = lineaRefs.current[indiceActivo]
    if (!contenedor || !linea || indiceActivo < 0) return

    const destino = linea.offsetTop - contenedor.clientHeight / 2 + linea.clientHeight / 2
    contenedor.scrollTo({ top: Math.max(0, destino), behavior: 'smooth' })
  }, [indiceActivo])

  if (lineas.length === 0) return null

  return (
    <div
      ref={contenedorRef}
      className="relative mt-2 max-h-64 overflow-y-auto scroll-smooth rounded-2xl border border-white/10 bg-white/[0.02] px-2 py-10"
    >
      {lineas.map((linea, indice) => {
        const esActiva = indice === indiceActivo
        return (
          <button
            key={`${linea.tiempo}-${indice}`}
            ref={(elemento) => {
              lineaRefs.current[indice] = elemento
            }}
            type="button"
            onClick={() => onIrA?.(linea.tiempo)}
            disabled={!puedeSeguir}
            title={puedeSeguir ? 'Ir a este verso' : 'Reproduce la canción para seguir la letra'}
            className={`block w-full rounded-xl px-3 py-1 text-left text-xs leading-6 transition-colors duration-300 ${
              esActiva
                ? 'bg-white/[0.07] font-medium text-white'
                : 'text-white/35 hover:text-white/60 disabled:hover:text-white/35'
            }`}
          >
            {linea.texto}
          </button>
        )
      })}
    </div>
  )
}

export default LetraSincronizada
