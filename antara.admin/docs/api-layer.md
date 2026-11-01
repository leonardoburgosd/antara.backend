# Capa de API — Comunicación con el Backend

## Estructura

```
src/services/api/
├── apiClient.js    ← Cliente HTTP centralizado (fetch + auth + timeout)
├── apiConfig.js    ← URL base y timeout
└── endpoints.js    ← Mapa de todas las rutas del backend
```

---

## apiConfig.js

```js
export const apiConfig = {
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:8080',
  timeout: 30000, // 30 segundos
}
```

Para cambiar la URL de la API en producción, configura la variable de entorno `VITE_API_URL` en el build de Vite o en `nginx.conf` como proxy inverso.

---

## apiClient.js — El cliente HTTP

### Características automáticas

- **Token automático:** Lee el token de `localStorage` y agrega `Authorization: Bearer {token}` en cada request.
- **Timeout:** Cancela la request con `AbortController` si supera el tiempo configurado.
- **Redirect 401:** Si la API devuelve 401, limpia la sesión de localStorage y redirige a `/login?sesion=expirada`.
- **JSON body automático:** Si el body es un objeto (no `FormData`), serializa a JSON y agrega `Content-Type: application/json`.

### Métodos disponibles

```js
import { apiClient } from '../../../services/api/apiClient'

// GET
const response = await apiClient.get('/api/pista/todos')

// POST con body JSON
const response = await apiClient.post('/api/usuario/login', { email, password })

// PUT con body JSON
const response = await apiClient.put(`/api/rutas-audio/${id}`, formData)

// DELETE
const response = await apiClient.delete(`/api/rutas-audio/${id}`)

// Con signal para cancelar (ej: búsqueda con AbortController)
const response = await apiClient.get('/api/pista/metadatos/buscar', { signal })
```

> [!IMPORTANT]
> `apiClient` devuelve el `Response` object de fetch, no los datos. Siempre llama a `response.json()` o verifica `response.ok` en el servicio.

### Patrón estándar de un servicio

```js
async getMisDatos() {
  const response = await apiClient.get(ENDPOINTS.MI_FEATURE.LIST)
  if (!response.ok) {
    const msg = await parseErrorMessage(response, 'Error por defecto.')
    throw new ApiError(msg, response.status)
  }
  return response.json()
}
```

---

## parseErrorMessage

Intenta extraer el mensaje de error del cuerpo de la respuesta:

```js
// Busca en este orden:
// 1. data.error
// 2. data.errors (primero de la lista)
// 3. data.title
// 4. Si nada funciona, devuelve el fallback
const msg = await parseErrorMessage(response, 'Mensaje por defecto.')
```

---

## ApiError

Clase de error con código de estado HTTP:

```js
throw new ApiError('No se encontró la pista.', 404)

// En el hook:
try {
  await pistaService.getPistas()
} catch (err) {
  setError(err.message)           // Mensaje legible
  // err.status tiene el código HTTP (404, 500, etc.)
}
```

---

## endpoints.js — Mapa de rutas

```js
export const ENDPOINTS = {
  AUTH: {
    LOGIN: '/api/usuario/login',
    CREATE_FIRST_ADMIN: '/api/sistema/crea-primero-admin',
    HEALTH: '/api/sistema/health',
  },
  PISTAS: {
    LIST: '/api/pista/todos',
    BY_ID: (id) => `/api/pista/${id}`,
    STREAM: (id) => `/api/pista/${id}`,           // Mismo endpoint, streaming
    SEARCH_METADATA: '/api/pista/metadatos/buscar',
    UPDATE_METADATA: (id) => `/api/pista/${id}/metadatos`,
    PLAY: (id) => `/reproducir/${id}`,
  },
  // ... más endpoints
}
```

**Regla:** Todos los endpoints están centralizados aquí. Nunca escribas una URL hardcodeada en un servicio.

---

## Reglas absolutas

1. **NUNCA hagas `fetch()` directamente** en un componente, página o hook. Solo en servicios, a través de `apiClient`.
2. **NUNCA escribas una URL hardcodeada** fuera de `endpoints.js`.
3. **SIEMPRE verifica `response.ok`** antes de parsear el cuerpo.
4. **SIEMPRE lanza `ApiError`** cuando `response.ok === false`.
5. **NUNCA retornes el `Response` object** desde un servicio. Devuelve los datos ya parseados.

---

## ❌ Errores comunes

- **`fetch('/api/...')` en un componente.** Viola la separación de capas.
- **No verificar `response.ok`.** Una respuesta 404 o 500 también pasa el `then()` de fetch.
- **No pasar `signal` en búsquedas en tiempo real.** Sin `AbortController`, las requests anteriores no se cancelan y pueden llegar después de las nuevas.
- **Olvidar el `await` en `response.json()`.** Devuelve una `Promise`, no los datos.
- **Usar `ENDPOINTS.PISTAS.STREAM` para registrar reproducción.** Usa `ENDPOINTS.PISTAS.PLAY` (son rutas distintas: GET para stream, PUT para incrementar contador).

