import { useState, useCallback } from 'react'

export function useLocalStorage(key, initialValue) {
  const [storedValue, setStoredValue] = useState(() => {
    try {
      const item = window.localStorage.getItem(key)
      return item !== null ? JSON.parse(item) : initialValue
    } catch {
      return initialValue
    }
  })

  const setValue = useCallback(
    (value) => {
      try {
        const valueToStore = value instanceof Function ? value(storedValue) : value
        setStoredValue(valueToStore)
        window.localStorage.setItem(key, JSON.stringify(valueToStore))
      } catch (err) {
        console.error(`useLocalStorage[${key}] error:`, err)
      }
    },
    [key, storedValue]
  )

  const removeValue = useCallback(() => {
    try {
      setStoredValue(initialValue)
      window.localStorage.removeItem(key)
    } catch (err) {
      console.error(`useLocalStorage[${key}] error:`, err)
    }
  }, [key, initialValue])

  return [storedValue, setValue, removeValue]
}

export default useLocalStorage

