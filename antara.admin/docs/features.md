# Guía para agregar una nueva funcionalidad (Feature)

Esta guía muestra cómo agregar un nuevo feature completo. El ejemplo es "Géneros musicales".

> [!IMPORTANT]
> Sigue los pasos en orden. Cada paso depende del anterior.

---

## Los 7 pasos

### Paso 1 — Estructura de carpetas

Crea la estructura del feature:

```
src/features/generos/
├── components/
├── hooks/
├── pages/
├── services/
└── index.js
```

---

### Paso 2 — Servicio (llamadas a la API)

`src/features/generos/services/generoService.js`:

```js
import { apiClient, parseErrorMessage, ApiError } from '../../../services/api/apiClient'
import { ENDPOINTS } from '../../../services/api/endpoints'

export const generoService = {
  async getGeneros() {
    const response = await apiClient.get(ENDPOINTS.GENEROS.LIST)
    if (!response.ok) {
      const msg = await parseErrorMessage(response, 'No se pudieron cargar los géneros.')
      throw new ApiError(msg, response.status)
    }
    const data = await response.json()
    return Array.isArray(data) ? data : []
  },

  async crearGenero(dto) {
    const response = await apiClient.post(ENDPOINTS.GENEROS.CREATE, dto)
    if (!response.ok) {
      const msg = await parseErrorMessage(response, 'No se pudo crear el género.')
      throw new ApiError(msg, response.status)
    }
    return response.json()
  },
}
```

**Reglas del servicio:**
- Siempre usa `apiClient` — nunca `fetch` directo.
- Siempre verifica `response.ok` y lanza `ApiError` si falla.
- El servicio devuelve datos ya parseados (no el `Response` object).
- No tiene estado React (`useState`, `useEffect`).

---

### Paso 3 — Agregar endpoints

`src/services/api/endpoints.js`:

```js
export const ENDPOINTS = {
  // ... endpoints existentes ...
  GENEROS: {
    LIST: '/api/genero',
    CREATE: '/api/genero',
    BY_ID: (id) => `/api/genero/${id}`,
    DELETE: (id) => `/api/genero/${id}`,
  },
}
```

---

### Paso 4 — Hook (estado y lógica)

`src/features/generos/hooks/useGeneros.js`:

```js
import { useState, useEffect, useCallback } from 'react'
import { generoService } from '../services/generoService'

export function useGeneros({ onToast = () => {} } = {}) {
  const [generos, setGeneros] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const cargar = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      const data = await generoService.getGeneros()
      setGeneros(data)
    } catch (err) {
      setError(err.message || 'Error al cargar los géneros.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    cargar()
  }, [cargar])

  const crear = useCallback(async (dto) => {
    try {
      await generoService.crearGenero(dto)
      onToast('Género creado correctamente.')
      await cargar()
    } catch (err) {
      onToast(err.message || 'Error al crear el género.')
    }
  }, [cargar, onToast])

  return { generos, loading, error, cargar, crear }
}
```

**Reglas del hook:**
- Solo llama al servicio — nunca `apiClient` directamente.
- Gestiona `loading`, `error` y el estado de los datos.
- Usa `useCallback` para funciones que se pasan a componentes hijos.
- El `onToast` es un callback opcional para mostrar notificaciones.

---

### Paso 5 — Página

`src/features/generos/pages/GenerosPage.jsx`:

```jsx
import { useGeneros } from '../hooks/useGeneros'
import { EmptyState } from '../../../components/ui/EmptyState'
import { ErrorBanner } from '../../../components/ui/ErrorBanner'
import { SkeletonTable } from '../../../components/ui/Skeleton'
import { IconList } from '../../../components/ui/Icons'

export const GenerosPage = ({ onToast = () => {} }) => {
  const { generos, loading, error, cargar, crear } = useGeneros({ onToast })

  return (
    <section className="space-y-5">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-semibold tracking-tight">Géneros</h2>
      </div>

      {error && <ErrorBanner message={error} onRetry={cargar} />}

      {loading ? (
        <SkeletonTable rows={4} cols={2} />
      ) : generos.length === 0 ? (
        <EmptyState icon={IconList} title="Sin géneros" description="No hay géneros registrados." />
      ) : (
        <ul className="space-y-2">
          {generos.map((g) => (
            <li key={g.id} className="rounded-2xl border border-white/10 bg-white/[0.03] px-4 py-3">
              {g.nombre}
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}

export default GenerosPage
```

---

### Paso 6 — index.js del feature

`src/features/generos/index.js`:

```js
export { GenerosPage } from './pages/GenerosPage'
export { useGeneros } from './hooks/useGeneros'
export { generoService } from './services/generoService'
```

---

### Paso 7 — Integrar en DashboardPage

En `src/features/dashboard/pages/DashboardPage.jsx`:

1. Agrega el import:
```js
import { GenerosPage } from '../../generos'
```

2. Agrega al array `NAV_ITEMS`:
```js
const NAV_ITEMS = [
  // ... items existentes ...
  { id: 'generos', label: 'Géneros', icon: IconList },
]
```

3. Agrega al switch de contenido:
```jsx
{activeNav === 'generos' ? (
  <GenerosPage onToast={setToast} />
) : /* ... otros casos ... */}
```

4. Actualiza `handleNav` para no mostrar "próximamente":
```js
const handleNav = (id) => {
  setActiveNav(id)
  // Quita 'generos' de los que muestran "próximamente"
  if (id !== 'dashboard' && id !== 'rutas' && id !== 'pistas' && id !== 'generos') {
    setToast(`${item?.label}: próximamente.`)
  }
}
```

---

## Convenciones de código

| Convención | Detalle |
|---|---|
| **Archivos** | `camelCase.js` para servicios y hooks, `PascalCase.jsx` para componentes |
| **Exportaciones** | Named exports en todos los archivos (`export const X`) |
| **Props de onToast** | Siempre opcional con default: `onToast = () => {}` |
| **Loading skeleton** | Usa `<SkeletonTable>` o `<Skeleton>` de `components/ui/` |
| **Error banner** | Usa `<ErrorBanner message={error} onRetry={cargar} />` |
| **Estado vacío** | Usa `<EmptyState icon={...} title="..." description="..." />` |
| **Estilos** | Solo Tailwind CSS. Sin CSS modules, sin styled-components, sin CSS inline salvo gradientes dinámicos |

---

## ❌ Errores comunes al crear features

- **Hacer `fetch` en la página o en el hook directamente.** Solo en el servicio, a través de `apiClient`.
- **Olvidar `index.js`.** El feature no es importable desde otros archivos sin él.
- **Crear un hook que devuelve el `Response` object de fetch.** El hook devuelve datos ya procesados.
- **No manejar el estado `loading`.** Siempre muestra un skeleton mientras cargan los datos.
- **No manejar el estado `error`.** Siempre muestra un `<ErrorBanner>` cuando hay error.
- **Poner lógica de negocio en el componente.** Va en el hook.
- **Importar directamente desde `features/X/hooks/usePistas.js` en otro feature.** Usa solo `features/X/index.js`.

