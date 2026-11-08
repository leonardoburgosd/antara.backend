import { STORAGE_KEYS } from '../../utils/constants'

export const storage = {
  get(key, defaultValue = null) {
    try {
      const item = localStorage.getItem(key)
      if (item === null) return defaultValue
      try {
        return JSON.parse(item)
      } catch {
        return item
      }
    } catch {
      return defaultValue
    }
  },

  set(key, value) {
    try {
      if (typeof value === 'object' && value !== null) {
        localStorage.setItem(key, JSON.stringify(value))
      } else {
        localStorage.setItem(key, String(value))
      }
      return true
    } catch {
      return false
    }
  },

  remove(key) {
    try {
      localStorage.removeItem(key)
      return true
    } catch {
      return false
    }
  },

  clearAuth() {
    storage.remove(STORAGE_KEYS.USER)
    storage.remove(STORAGE_KEYS.TOKEN)
  },

  getToken() {
    return storage.get(STORAGE_KEYS.TOKEN)
  },

  getUser() {
    return storage.get(STORAGE_KEYS.USER)
  },

  hasSession() {
    return Boolean(storage.getUser() && storage.getToken())
  },

  saveSession(usuario) {
    if (!usuario?.token) return false
    const { token, ...perfil } = usuario
    storage.set(STORAGE_KEYS.USER, perfil)
    storage.set(STORAGE_KEYS.TOKEN, token)
    return true
  },

  /**
   * Actualiza campos del usuario en sesión sin perder el token.
   * @param {object} parcial
   */
  updateUser(parcial) {
    const actual = storage.getUser()
    if (!actual) return false
    return storage.set(STORAGE_KEYS.USER, { ...actual, ...parcial })
  },

  getVolume(defaultValue = 0.8) {
    const guardado = storage.get(STORAGE_KEYS.VOLUME)
    if (guardado === null || guardado === '') return defaultValue
    const numero = Number(guardado)
    return Number.isFinite(numero) && numero >= 0 && numero <= 1 ? numero : defaultValue
  },

  setVolume(value) {
    storage.set(STORAGE_KEYS.VOLUME, String(value))
  },
}

