import { useState, useRef, useEffect, useCallback } from 'react'
import { storage } from '../../../services/storage/localStorage'
import { pistaService } from '../services/pistaService'

export function useAudioPlayer({ onToast = () => {} } = {}) {
  const [reproduciendoId, setReproduciendoId] = useState(null)
  const [sonando, setSonando] = useState(false)
  const [progreso, setProgreso] = useState(0)
  const [duracionAudio, setDuracionAudio] = useState(0)
  const [volumen, setVolumen] = useState(() => storage.getVolume(0.8))
  const [silenciado, setSilenciado] = useState(false)
  const audioRef = useRef(null)

  // Cleanup on unmount
  useEffect(() => {
    const audio = audioRef.current
    return () => {
      if (audio) {
        audio.pause()
        audio.removeAttribute('src')
      }
    }
  }, [])

  // Sync volume
  useEffect(() => {
    const audio = audioRef.current
    if (audio) audio.volume = silenciado ? 0 : volumen
    storage.setVolume(volumen)
  }, [volumen, silenciado])

  const alternarReproduccion = useCallback(
    async (pista) => {
      const audio = audioRef.current
      if (!audio) return

      if (reproduciendoId === pista.id) {
        if (audio.paused) {
          try {
            await audio.play()
          } catch {
            onToast('No se pudo reanudar la reproducción.')
          }
        } else {
          audio.pause()
        }
        return
      }

      audio.pause()
      audio.src = pistaService.getStreamUrl(pista.id)
      audio.currentTime = 0
      setProgreso(0)
      setDuracionAudio(0)
      setReproduciendoId(pista.id)

      try {
        await audio.play()
        pistaService.registerPlay(pista.id)
      } catch {
        setReproduciendoId(null)
        setSonando(false)
        onToast('No se pudo reproducir esta pista. Verifica que el archivo siga disponible.')
      }
    },
    [reproduciendoId, onToast],
  )

  const detener = useCallback(() => {
    const audio = audioRef.current
    if (audio) {
      audio.pause()
      audio.removeAttribute('src')
    }
    setReproduciendoId(null)
    setSonando(false)
    setProgreso(0)
    setDuracionAudio(0)
  }, [])

  const buscarTiempo = useCallback((evento) => {
    const audio = audioRef.current
    if (!audio) return
    const valor = Number(evento.target.value) || 0
    audio.currentTime = valor
    setProgreso(valor)
  }, [])

  const cambiarVolumen = useCallback((evento) => {
    const valor = Math.min(1, Math.max(0, Number(evento.target.value) / 100))
    setVolumen(valor)
    if (valor > 0) setSilenciado(false)
  }, [])

  const alternarSilencio = useCallback(() => {
    setSilenciado((prev) => {
      if (prev) {
        if (volumen === 0) setVolumen(0.8)
        return false
      }
      return true
    })
  }, [volumen])

  return {
    audioRef,
    reproduciendoId,
    sonando,
    progreso,
    duracionAudio,
    volumen,
    silenciado,
    alternarReproduccion,
    detener,
    buscarTiempo,
    cambiarVolumen,
    alternarSilencio,
    setSonando,
    setProgreso,
    setDuracionAudio,
  }
}

export default useAudioPlayer

