# Referencia de API — Endpoints HTTP

## Resumen de endpoints

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/api/sistema/health` | No | Estado del sistema |
| `GET` | `/api/sistema/unidades-host` | Admin | Unidades del host visibles desde la API |
| `GET` | `/api/sistema/crear-primer-admin` | No | Verifica si hay admin |
| `POST` | `/api/sistema/crea-primero-admin` | No | Crea el primer admin |
| `POST` | `/api/usuario/login` | No | Login con email/password |
| `GET` | `/api/usuario/todos` | Admin | Lista todos los usuarios |
| `POST` | `/api/usuario` | Admin | Crea un usuario |
| `PUT` | `/api/usuario/{id}` | Admin | Actualiza un usuario (datos, rol, estado, contraseña) |
| `DELETE` | `/api/usuario/{id}` | Admin | Elimina un usuario |
| `GET` | `/api/usuario/perfil` | Token | Datos del usuario autenticado |
| `PUT` | `/api/usuario/perfil` | Token | Actualiza el perfil propio |
| `POST` | `/api/usuario/perfil/password` | Token | Cambia la contraseña propia |
| `GET` | `/api/pista/todos` | Admin | Biblioteca de pistas |
| `GET` | `/api/pista/subir/rutas` | Admin | Rutas disponibles para subir audio |
| `POST` | `/api/pista/subir` | Admin | Sube un archivo de audio a una ruta |
| `POST` | `/api/pista/subir/url` | Admin | Descarga audio de YouTube y lo guarda en una ruta |
| `GET` | `/api/pista/metadatos/proveedores` | Admin | Proveedores de metadatos habilitados |
| `GET` | `/api/pista/metadatos/buscar` | Admin | Busca metadatos en proveedores |
| `POST` | `/api/pista/{id}/metadatos` | Admin | Aplica metadatos a una pista |
| `GET` | `/api/pista/{id}` | No | Stream del archivo de audio |
| `PUT` | `/reproducir/{id}` | No | Registra una reproducción |
| `GET` | `/api/album/todos` | Admin | Lista todos los álbumes |
| `POST` | `/api/album` | Admin | Crea un álbum |
| `PUT` | `/api/album/{id}` | Admin | Edita un álbum |
| `PUT` | `/api/album/{id}/publicar` | Admin | Alterna publicado/borrador |
| `DELETE` | `/api/album/{id}` | Admin | Elimina un álbum |
| `GET` | `/api/album/{id}` | No | Obtiene un álbum |
| `GET` | `/api/album/todos/{usuarioId}` | No | Álbumes de un usuario |
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

### `GET /api/sistema/unidades-host`

Unidades del host que el contenedor de la API puede leer en este momento. Se
detectan desde los montajes reales (`/host/c`, `/host/h`…), sin letras fijas en
el código. Requiere token de administrador.

**Respuesta 200:**
```json
{
  "raiz": "/host",
  "unidades": ["c", "d", "e", "f", "g", "h"]
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

## Usuarios (`UsuarioController`)

Todos los endpoints, salvo `login`, exigen `Authorization: Bearer {token}`. El CRUD
requiere rol `admin`.

### `GET /api/usuario/todos`

Lista de usuarios sin contraseña. Cada elemento incluye `id`, `email`, `nombre`,
`fechaNacimiento`, `genero`, `estaActivo`, `fechaRegistro`, `pais`, `fotoPerfil` y `tipo`.

### `POST /api/usuario`

**Body:**
```json
{
  "email": "usuario@antara.app",
  "password": "mínimo-6-caracteres",
  "nombre": "Nombre completo",
  "fechaNacimiento": "1995-04-12",
  "genero": "F",
  "pais": "Perú",
  "fotoPerfil": "https://...",
  "tipo": "usuario"
}
```

Responde `201` con el usuario creado. `409`/`400` si el correo ya existe o los datos no son válidos.
`tipo` se normaliza a `admin` o `usuario`.

### `PUT /api/usuario/{id}`

Mismos campos que la creación, todos opcionales. `password` solo se reemplaza si se envía.
El correo debe seguir siendo único.

### `DELETE /api/usuario/{id}`

Elimina la cuenta. No permite borrar la propia cuenta ni dejar el sistema sin ningún
administrador. Si el usuario tiene álbumes o playlists asociados, la base rechaza el borrado (`500`).

### `GET /api/usuario/perfil`

Devuelve el usuario del token, con el mismo formato que la lista.

### `PUT /api/usuario/perfil`

Permite cambiar únicamente `email`, `nombre`, `fechaNacimiento`, `genero`, `pais` y
`fotoPerfil`. El rol y el estado no se modifican desde aquí.

### `POST /api/usuario/perfil/password`

**Body:**
```json
{ "passwordActual": "...", "passwordNueva": "mínimo 6 caracteres" }
```

Responde `400` si la contraseña actual no coincide o la nueva es demasiado corta.

---

## Álbumes (`AlbumController`)

### `GET /api/album/todos`

Lista todos los álbumes con `id`, `nombre`, `descripcion`, `estaPublicado`,
`fechaPublicacion`, `portadaUrl`, `usuarioId`, `estaActivo`, `artista`,
`fuenteMetadatos` e `idExternoMetadatos`.

### `POST /api/album`

**Body:**
```json
{
  "nombre": "A Night at the Opera",
  "descripcion": "Álbum de estudio",
  "artista": "Queen",
  "portadaUrl": "https://...",
  "estaPublicado": false
}
```

Responde `201` con el álbum creado. El `usuarioId` se toma del token; el nombre es obligatorio.

### `PUT /api/album/{id}`

Mismos campos, todos opcionales; los ausentes conservan su valor. Al pasar a publicado se
registra la fecha de publicación si aún no existía.

### `PUT /api/album/{id}/publicar`

Alterna el estado de publicación y devuelve el álbum actualizado.

### `DELETE /api/album/{id}`

Responde `400` si el álbum tiene pistas asociadas (la relación debe quitarse antes).

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

### `GET /api/pista/subir/rutas`

Lista las rutas de audio activas como posibles destinos de subida. El panel arma su
selector con esta respuesta.

**Respuesta 200:** Array de `RutaSubidaDto`:

| Campo | Tipo | Descripción |
|---|---|---|
| `id` | uuid | Identificador de la ruta |
| `nombre` | string | Nombre visible |
| `rutaDestino` | string | Carpeta resuelta dentro del contenedor |
| `disponible` | bool | La carpeta existe y es visible desde la API |
| `escribible` | bool | La API puede crear archivos en ella |
| `esPredeterminada` | bool | Ruta que se usa cuando no se elige otra |

### `POST /api/pista/subir`

Sube una canción desde el panel y la registra en la biblioteca (con el mismo
enriquecido de metadatos que el escaneo). Requiere `multipart/form-data`:

| Campo | Tipo | Descripción |
|---|---|---|
| `archivo` | file | Archivo de audio (obligatorio) |
| `rutaId` | uuid | Ruta de destino; si se omite se usa la predeterminada |
| `subcarpeta` | string | Subcarpeta opcional dentro de la ruta (por ejemplo `Rock/2026`) |

Solo se aceptan `.mp3`, `.flac`, `.wav`, `.m4a`, `.aac`, `.ogg`, `.opus`, `.wma`,
`.aiff` y `.alac`. El archivo nunca se escribe fuera de la ruta elegida y no se
sobrescribe: si ya existe, se agrega ` (1)`, ` (2)`, …

**Respuesta 201:** `SubidaPistaDto` con `pistaId`, `nombre`, `interprete`, `album`,
`archivo`, `rutaDestino`, `rutaId`, `rutaNombre`, `conMetadatos` y `mensaje`.

**Errores:**
- `400` — Extensión no permitida, archivo vacío, subcarpeta inválida o ruta no escribible.
- `404` — La ruta indicada no existe.

### `POST /api/pista/subir/url`

Descarga el audio de un enlace de **YouTube** (con `yt-dlp` + `ffmpeg` dentro de la
imagen) y lo guarda en la ruta indicada, registrándolo en la biblioteca con el mismo
enriquecido de metadatos que el escaneo. Cuerpo JSON:

```json
{ "url": "https://www.youtube.com/watch?v=...", "rutaId": "uuid-opcional", "subcarpeta": "Rock/2026" }
```

- Solo se aceptan dominios `youtube.com` (incluye `music.` y `m.`) y `youtu.be`; un
  enlace de otro sitio responde `400`.
- Se descarga el mejor audio disponible y se convierte a **MP3**; las listas de
  reproducción se ignoran (`--no-playlist`).
- El tiempo máximo por descarga se configura con
  `AppSettings__DescargaYouTubeTimeoutSegundos` (600 por defecto).
- Si la ruta no existe o no es escribible responde `400`; si el `rutaId` no existe, `404`.

**Respuesta 201:** `SubidaPistaDto` con `pistaId`, `nombre`, `interprete`, `album`,
`archivo`, `rutaDestino`, `rutaId`, `rutaNombre`, `conMetadatos` y `mensaje`.

### `GET /api/pista/metadatos/buscar?cadena=queen&proveedor=todos`

Busca candidatos de metadatos en los proveedores externos.

**Auth:** `[TokenAuthorize("admin", "administrador")]`

**Query params:**
- `cadena` (requerido) — Texto de búsqueda (artista + título)
- `proveedor` — `"todos"` o el `Nombre` de cualquier proveedor habilitado en la configuración (`deezer`, `theaudiodb`, ...). Defecto: `"todos"`.

**Respuesta 400:** si `proveedor` no corresponde a ningún proveedor habilitado:
```json
{ "error": "El proveedor 'spotify' no esta habilitado. Disponibles: deezer, theaudiodb." }
```

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

