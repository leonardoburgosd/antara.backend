# Manejo de Estado

## Principio general

El proyecto **no usa Redux, Zustand ni Context API** para el estado de datos. Todo el estado vive en hooks locales de cada feature. Esto es intencional: la aplicación es un panel de administración con secciones independientes que no necesitan compartir estado entre sí.

---

## Estado local — hooks de feature

Cada feature gestiona su propio estado con `useState`, `useEffect` y `useCallback`:

```js
// Patrón estándar de un hook de feature
export function usePistas() {
  const [pistas, setPistas] = useState([])     // datos
  const [loading, setLoading] = useState(true) // estado de carga
  const [error, setError] = useState('')        // estado de error

  const cargar = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      const data = await pistaService.getPistas()
      setPistas(data)
    } catch (err) {
      setError(err.message || 'Error de conexión.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { cargar() }, [cargar])

  return { pistas, loading, error, cargar }
}
```

---

## Estado global — localStorage

El único estado verdaderamente global se gestiona en `localStorage` a través de `src/services/storage/localStorage.js`:

### Datos persistidos

| Clave | Contenido | Descripción |
|---|---|---|
| `antara_token` | `string` | Token de autenticación HMAC-SHA256 |
| `antara_user` | `object JSON` | Datos del usuario autenticado (sin password) |
| `antara_volumen` | `string (número)` | Volumen del reproductor (0 a 1) |

### API del storage

```js
import { storage } from '../../../services/storage/localStorage'

storage.getToken()              // string | null
storage.getUser()               // object | null
storage.hasSession()            // boolean
storage.saveSession(usuario)    // guarda token + perfil separados
storage.clearAuth()             // elimina token y perfil (logout)
storage.getVolume(0.8)          // number (0-1), con fallback
storage.setVolume(0.75)         // persiste el volumen
```

> [!NOTE]
> `storage.saveSession()` separa el `token` del resto del perfil antes de guardar. El objeto en `antara_user` nunca contiene el token.

---

## Cuándo usar `useMemo`

`useMemo` se usa para derivar datos del estado sin recalcular en cada render:

```js
// ✅ Correcto: filtrar/calcular deriva de pistas
const filtradas = useMemo(() => {
  const termino = normalizeString(busqueda)
  return pistas.filter(p => {
    if (filtro === 'letra' && !p.tieneLetra) return false
    if (!termino) return true
    return normalizeString(p.nombre).includes(termino)
  })
}, [pistas, busqueda, filtro])

// ✅ Correcto: estadísticas calculadas de la lista
const estadisticas = useMemo(() => ({
  total: pistas.length,
  conLetra: pistas.filter(p => p.tieneLetra).length,
}), [pistas])

// ❌ Incorrecto: operación trivial que no necesita memoización
const titulo = useMemo(() => `Pistas (${pistas.length})`, [pistas])
```

---

## Cuándo usar `useCallback`

`useCallback` se usa para funciones que:
- Se pasan como props a componentes hijos
- Son dependencias de `useEffect`

```js
// ✅ Correcto: se pasa como prop a hijo y es dependencia de useEffect
const cargar = useCallback(async () => { ... }, [])
useEffect(() => { cargar() }, [cargar])

// ✅ Correcto: se pasa como prop a componente hijo
const alternarReproduccion = useCallback(async (pista) => { ... }, [reproduciendoId])

// ❌ Incorrecto: función local que no se pasa a nadie
const calcularTotal = useCallback(() => pistas.length, [pistas])
```

---

## Cuándo extraer lógica a un hook

| Criterio | Extraer a hook | Mantener en componente |
|---|---|---|
| ¿Hay `useEffect` para cargar datos? | ✅ Siempre | ❌ |
| ¿El estado tiene más de 2-3 variables? | ✅ Sí | ❌ |
| ¿La lógica se reutiliza en más de un lugar? | ✅ Sí | ❌ |
| ¿Es solo estado de UI? (ej: `modalAbierto`) | ❌ | ✅ Sí |
| ¿Es un formulario simple de 2 campos? | ❌ | ✅ Sí |

---

## Cuándo considerar agregar estado global (Context / Zustand)

Actualmente **no es necesario**, pero considera agregarlo cuando:
- Múltiples features necesiten conocer el usuario autenticado en tiempo real (actualmente lo leen de `localStorage` directamente).
- Haya notificaciones globales que se disparen desde cualquier feature.
- El reproductor de audio necesite ser controlado desde múltiples páginas simultáneamente.

---

## ❌ Errores comunes de estado

- **Estado en un componente que debería estar en el hook.** Los componentes deben recibir datos por props.
- **Duplicar estado.** Si `pistas` está en el hook, no lo copies en la página con `useState`.
- **`useEffect` sin dependencias correctas.** Provoca bucles infinitos o datos obsoletos.
- **No limpiar efectos.** Los `EventListener`, `setInterval` y `setTimeout` deben limpiarse en el return del `useEffect`.
- **`useMemo` en operaciones triviales.** Tiene costo de overhead; solo úsalo cuando el cálculo sea costoso o cuando el objeto resultado necesite identidad estable.

