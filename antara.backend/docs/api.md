# Referencia de API — Endpoints HTTP

## Resumen de endpoints

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/api/sistema/health` | No | Estado del sistema |
| `GET` | `/api/sistema/crear-primer-admin` | No | Verifica si hay admin |
| `POST` | `/api/sistema/crea-primero-admin` | No | Crea el primer admin |
| `POST` | `/api/usuario/login` | No | Login con email/password |
| `GET` | `/api/usuario/todos` | Admin | Lista todos los usuarios |
| `POST` | `/api/usuario` | Admin | Crea un usuario |
| `DELETE` | `/api/usuario/{id}` | Admin | Elimina un usuario |
| `GET` | `/api/pista/todos` | Admin | Biblioteca de pistas |
| `GET` | `/api/pista/metadatos/buscar` | Admin | Busca metadatos en proveedores |
| `POST` | `/api/pista/{id}/metadatos` | Admin | Aplica metadatos a una pista |
| `GET` | `/api/pista/{id}` | No | Stream del archivo de audio |
| `PUT` | `/reproducir/{id}` | No | Registra una reproducción |
| `GET` | `/api/album/todos` | No | Lista todos los álbumes |
| `POST` | `/api/album` | Admin | Crea un álbum |
| `PUT` | `/api/album/{id}` | Admin | Edita un álbum |
| `DELETE` | `/api/album/{id}` | Admin | Elimina un álbum |
| `GET` | `/api/playlist` | Admin | Playlists del usuario autenticado |
| `POST` | `/api/playlist` | Admin | Crea una playlist |
| `GET` | `/api/rutas-audio` | Admin | Lista rutas de audio |
| `POST` | `/api/rutas-audio` | Admin | Crea una ruta de audio |
| `PUT` | `/api/rutas-audio/{id}` | Admin | Actualiza una ruta |
| `DELETE` | `/api/rutas-audio/{id}` | Admin | Elimina una ruta |
| `GET` | `/api/rutas-audio/{id}/probar` | Admin | Prueba una ruta |
| `GET` | `/api/dashboard` | Admin | Métricas del dashboard |
| `POST` | `/api/escaneo-audio` | Admin | Inicia un escaneo |
| `GET` | `/api/escaneo-audio/actual` | Admin | Estado del escaneo actual |
| `GET` | `/api/escaneo-audio/{id}` | Admin | Estado de un escaneo por ID |
| `DELETE` | `/api/escaneo-audio/{id}/cancelar` | Admin | Cancela un escaneo |

---

## Sistema (`SistemaController`)

### `GET /api/sistema/health`

Estado del sistema. No requiere autenticación. Usado por el healthcheck de Docker.

**Respuesta 200:**
```json
{
  "activo": true,
  "version": "1.0.0",
  "timestamp": "2026-11-01T06:00:00Z",
  "usuariosTotales": 1,
  "adminsRegistrados": true,
  "estado": "Operativo"
}
```

---

### `POST /api/sistema/crea-primero-admin`

Crea el primer administrador del sistema. Solo funciona si no existe ningún admin.

**Body:**
```json
{
  "email": "admin@antara.app",
  "password": "contraseña-segura",
  "nombre": "Administrador",
  "fechaNacimiento": "1990-01-01T00:00:00Z",
  "genero": "M",
  "pais": "PE",
  "tipo": "admin"
}
```

**Respuesta 200:** `{ "ok": true }`

**Errores:**
- `409` — Ya existe un administrador registrado.

---

## Autenticación (`UsuarioController`)

### `POST /api/usuario/login`

**Body:**
```json
{
  "email": "admin@antara.app",
  "password": "contraseña"
}
```

**Respuesta 200:**
```json
{
  "id": "uuid",
  "email": "admin@antara.app",
  "nombre": "Administrador",
  "tipo": "admin",
  "token": "base64url.firma"
}
```

**Errores:**
- `401` — Credenciales incorrectas.
- `404` — Usuario no encontrado.

> [!NOTE]
> El token es un JWT propio firmado con HMAC-SHA256. No es compatible con librerías JWT estándar. Ver [security.md](./security.md).

---

## Pistas (`PistaController`)

### `GET /api/pista/todos`

Lista todas las pistas de la biblioteca con metadatos completos.

**Auth:** `[TokenAuthorize("admin", "administrador")]`

**Respuesta 200:** Array de `PistaBibliotecaDto`:
```json
[
  {
    "id": "uuid",
    "nombre": "Bohemian Rhapsody",
    "interprete": "Queen",
    "albumNombre": "A Night at the Opera",
    "portadaUrl": "https://...",
    "anoCreacion": 1975,
    "generoNombre": "Rock",
    "duracionSegundos": 354,
    "reproducciones": 42,
    "tieneLetra": true,
    "fuenteMetadatos": "deezer"
  }
]
```

---

### `GET /api/pista/metadatos/buscar?cadena=queen&proveedor=todos`

Busca candidatos de metadatos en los proveedores externos.

**Auth:** `[TokenAuthorize("admin", "administrador")]`

**Query params:**
- `cadena` (requerido) — Texto de búsqueda (artista + título)
- `proveedor` — `"todos"` | `"deezer"` | `"theaudiodb"` (defecto: `"todos"`)

**Respuesta 200:** Array de candidatos:
```json
[
  {
    "idExterno": "912345",
    "titulo": "Bohemian Rhapsody",
    "artista": "Queen",
    "album": "A Night at the Opera",
    "portadaUrl": "https://...",
    "ano": 1975,
    "duracionSegundos": 354,
    "fuente": "deezer"
  }
]
```

---

### `POST /api/pista/{id}/metadatos`

Aplica los metadatos de un candidato a una pista existente.

**Auth:** `[TokenAuthorize("admin", "administrador")]`

**Body:**
```json
{
  "idExterno": "912345",
  "proveedor": "deezer"
}
```

**Respuesta 200:** `PistaBibliotecaDto` actualizado.

**Errores:**
- `404` — Pista no encontrada.
- `400` — `idExterno` inválido o proveedor no reconocido.

---

### `GET /api/pista/{id}`

Hace streaming del archivo de audio. Soporta **Range requests** (seek desde el frontend).

**Auth:** No requerida (permite reproducir desde el frontend sin token).

**Headers de respuesta:**
```
Accept-Ranges: bytes
Content-Type: audio/mpeg  (o el tipo correcto según extensión)
```

**Extensiones soportadas:** `.mp3`, `.flac`, `.wav`, `.m4a`, `.aac`, `.ogg`, `.opus`, `.wma`

**Errores:**
- `404` — Pista no encontrada o archivo no existe en disco.

---

## Rutas de audio (`RutaAudioController`)

### `POST /api/rutas-audio`

**Body:**
```json
{
  "nombre": "Música Principal",
  "rutaFisica": "C:/Users/Leo/Music",
  "rutaContenedor": "/music",
  "descripcion": "Biblioteca principal",
  "estaActiva": true,
  "esPredeterminada": true
}
```

> [!TIP]
> `rutaFisica` acepta rutas de Windows (`C:/...`). El sistema las traduce automáticamente a rutas del contenedor usando `HostPathResolver`.

---

### `GET /api/rutas-audio/{id}/probar`

Verifica si la ruta es accesible y cuenta los archivos de audio encontrados.

**Respuesta 200:**
```json
{
  "rutaEvaluada": "/host/c/Users/Leo/Music",
  "existe": true,
  "esDirectorio": true,
  "escribible": false,
  "archivosAudio": 342,
  "subdirectorios": 15,
  "mensaje": "Ruta accesible con 342 archivos de audio."
}
```

---

## Escaneo de audio (`EscaneoAudioController`)

### `POST /api/escaneo-audio`

Inicia un escaneo paralelo de todas las rutas activas.

**Auth:** `[TokenAuthorize("admin", "administrador")]`

**Errores:**
- `409` — Ya hay un escaneo activo en curso.

**Respuesta 200:** `EscaneoAudioDto` con el estado inicial.

---

### `GET /api/escaneo-audio/actual`

Devuelve el estado del escaneo en curso (si existe).

```json
{
  "id": "uuid",
  "estado": "EnProgreso",
  "activo": true,
  "totalArchivos": 342,
  "procesados": 120,
  "nuevos": 45,
  "sinMetadatos": 12,
  "omitidos": 63,
  "errores": 0,
  "archivoActual": "/music/queen/bohemian.mp3",
  "maxParalelo": 4
}
```

---

## Dashboard (`DashboardController`)

### `GET /api/dashboard`

**Auth:** `[TokenAuthorize("admin", "administrador")]`

```json
{
  "usuariosTotales": 3,
  "albumesTotales": 42,
  "pistasTotales": 586,
  "reproducciones totales": 12450,
  "pistasRecientes": [...],
  "pistasMasReproducidas": [...],
  "usuariosRecientes": [...],
  "albumesRecientes": [...]
}
```

---

## Códigos de error estándar

| Código | Significado |
|---|---|
| `200` | Éxito |
| `201` | Creado |
| `400` | Error de validación (`{ "error": "mensaje" }`) |
| `401` | No autenticado (`{ "error": "Debe enviar el token..." }`) |
| `403` | Sin permisos (`{ "error": "No tienes permisos..." }`) |
| `404` | No encontrado |
| `409` | Conflicto (duplicado o estado inválido) |
| `500` | Error interno (`{ "error": "mensaje genérico" }`) |

---

## ❌ Errores comunes al consumir la API

- **No enviar `Authorization: Bearer {token}` en endpoints protegidos.** Devuelve 401.
- **Enviar `proveedor: null` en vez de `"todos"`.** El sistema lanza `ArgumentException`.
- **Esperar que el streaming de audio funcione sin Range requests.** El seek requiere que el cliente envíe `Range: bytes=X-`.
- **Reintentar un POST de escaneo mientras hay uno activo.** Devuelve 409.

