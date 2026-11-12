# Referencia rápida de la API

> **Documento canónico para integraciones:** [integracion.md](./integracion.md). Allí están el flujo de autenticación, los pasos que ejecuta cada módulo y ejemplos listos para copiar. Esta página es el índice de endpoints y sus reglas de acceso.

## Convenciones

- URL base: `http://<host>:<API_PORT>`; el puerto por defecto es `8080`.
- Prefijo: `/api`, salvo los endpoints heredados `/buscar` y `/reproducir/{id}`.
- Autenticación: `Authorization: Bearer <token>`.
- Subidas y portadas: `multipart/form-data`.
- Resto: `application/json`.
- El token propio vence a las 8 horas por defecto y no tiene refresh.

## Sistema (`/api/sistema`)

| Método | Ruta | Auth | Descripción corta |
|---|---|---|---|
| `GET` | `/health` | Público | Estado, versión, usuarios y si existe administrador |
| `GET` | `/unidades-host` | Admin | Unidades del host montadas en el contenedor |
| `POST` | `/crea-primero-admin` | Público solo en bootstrap | Crea el primer administrador |
| `GET` | `/{id}` | Público | Devuelve un usuario por ID (heredado) |

Mientras no exista administrador, el resto de endpoints responde `503`.

## Autenticación y usuarios (`/api/usuario`)

| Método | Ruta | Auth | Descripción corta |
|---|---|---|---|
| `POST` | `/login` | Público | Login; devuelve datos del usuario y token |
| `GET` | `/perfil` | Token | Perfil del usuario autenticado |
| `PUT` | `/perfil` | Token | Edita los datos personales propios |
| `POST` | `/perfil/password` | Token | Cambia la contraseña propia |
| `GET` | `/todos` | Admin | Lista todos los usuarios |
| `POST` | `` (base) | Admin | Crea un usuario |
| `PUT` | `/{id}` | Admin | Edita rol, estado, datos o contraseña |
| `DELETE` | `/{id}` | Admin | Elimina un usuario |
| `GET` | `/{id}` | Admin | Consulta un usuario |

Reglas: no se puede eliminar la propia cuenta ni al último administrador; el correo es único; la contraseña mínima es de 6 caracteres.

## Pistas de la biblioteca (`/api/pista`)

| Método | Ruta | Auth | Descripción corta |
|---|---|---|---|
| `GET` | `/todos` | Admin | Biblioteca completa |
| `GET` | `/subir/rutas` | Admin | Rutas activas para subir archivos |
| `POST` | `/subir` | Admin | Sube un archivo a una ruta (`multipart`) |
| `POST` | `/subir/url` | Admin | Descarga audio de YouTube y lo registra |
| `GET` | `/metadatos/proveedores` | Admin | Proveedores de metadatos habilitados |
| `GET` | `/metadatos/buscar` | Admin | Busca candidatos por `cadena` y `proveedor` |
| `POST` | `/{id}/metadatos` | Admin | Aplica un candidato a una pista |
| `GET` | `/{id}/letra-sincronizada` | Admin | Letra LRC de LRCLIB |
| `GET` | `/{id}/descarga` | Admin | ZIP con audio, portada, letras y `pista.json` |
| `GET` | `/{id}` | Público/privado | Stream con `Range`; privadas exigen token |
| `GET` | `/todos/album/{albumId}` | Público filtrado | Pistas de un álbum |
| `GET` | `/todos/playlist/{playlistId}` | Público filtrado | Pistas de una playlist |

**Subida de archivo:** campos `archivo` (obligatorio), `rutaId` (opcional) y `subcarpeta` (opcional). Extensiones admitidas: `.mp3`, `.flac`, `.wav`, `.m4a`, `.aac`, `.ogg`, `.opus`, `.wma`, `.aiff`, `.alac`.

**Subida desde YouTube:** JSON `{ "url": "...", "rutaId": "uuid-opcional", "subcarpeta": "..." }`. Solo dominios de YouTube.

**Stream privado:** si la pista tiene `usuarioId`, el token del dueño o de un administrador debe ir en `Authorization` o en `?token=`. Si falta o no autoriza, responde `404`.

**Descarga ZIP:** incluye el audio original, `portada.*`, `letra.txt`, `letra.lrc` y `pista.json` cuando están disponibles.

## Mis pistas (`/api/mis-pistas`)

| Método | Ruta | Auth | Descripción corta |
|---|---|---|---|
| `GET` | `` (base) | Usuario/Admin | Pistas propias; el admin ve todas |
| `POST` | `/subir` | Usuario/Admin | Sube un archivo a la ruta predeterminada |
| `POST` | `/subir/url` | Usuario/Admin | Descarga YouTube a la ruta predeterminada |
| `GET` | `/{id}` | Dueño/Admin | Detalle de una pista |
| `PUT` | `/{id}` | Dueño/Admin | Edita nombre, artista, año, género, álbum y portada |
| `GET` | `/metadatos/proveedores` | Usuario/Admin | Proveedores disponibles |
| `GET` | `/metadatos/buscar` | Usuario/Admin | Busca candidatos |
| `POST` | `/{id}/metadatos` | Dueño/Admin | Reasigna metadatos |
| `GET` | `/{id}/letra-sincronizada` | Dueño/Admin | Letra LRC |

Un usuario normal siempre usa la ruta predeterminada activa y solo ve sus pistas. Un administrador ve todas.

## Álbumes (`/api/album`)

| Método | Ruta | Auth | Descripción corta |
|---|---|---|---|
| `GET` | `/todos` | Admin | Todos los álbumes |
| `POST` | `` (base) | Admin | Crea un álbum |
| `PUT` | `/{id}` | Admin | Edita un álbum |
| `PUT` | `/{id}/publicar` | Admin | Alterna publicado/borrador |
| `DELETE` | `/{id}` | Admin | Elimina un álbum |
| `GET` | `/{id}` | Público | Consulta un álbum |
| `GET` | `/todos/{userId}` | Público | Álbumes de un usuario |

No se puede eliminar un álbum con pistas asociadas (`400`). Las lecturas públicas no validan token en el código actual.

## Playlists (`/api/playlist`)

| Método | Ruta | Auth | Descripción corta |
|---|---|---|---|
| `GET` | `/todos` | Admin | Todas las playlists con conteos |
| `GET` | `/todos/{userId}` | Usuario/Admin | Playlists de un usuario |
| `GET` | `/{id}` | Usuario/Admin | Detalle de una playlist |
| `GET` | `/{id}/pistas` | Usuario/Admin | Canciones de la playlist |
| `POST` | `` (base) | Usuario/Admin | Crea playlist (`multipart`) |
| `PUT` | `/{id}` | Usuario/Admin | Edita playlist (`multipart`) |
| `DELETE` | `/{id}` | Usuario/Admin | Elimina playlist |
| `POST` | `/{id}/pistas` | Usuario/Admin | Agrega una canción (`{ "pistaId": "uuid" }`) |
| `DELETE` | `/{id}/pistas/{pistaId}` | Usuario/Admin | Quita una canción |

Un usuario normal solo puede ver y modificar sus playlists (`403` en caso contrario). Un administrador puede operar sobre cualquiera y puede indicar `usuarioId` al crear.

## Rutas de audio (`/api/rutas-audio`)

| Método | Ruta | Auth | Descripción corta |
|---|---|---|---|
| `GET` | `` (base) | Admin | Lista rutas registradas |
| `GET` | `/{id}` | Admin | Consulta una ruta |
| `POST` | `` (base) | Admin | Crea una ruta |
| `PUT` | `/{id}` | Admin | Actualiza una ruta |
| `DELETE` | `/{id}` | Admin | Elimina una ruta |
| `POST` | `/{id}/probar` | Admin | Comprueba acceso, escritura y archivos |

Crear/editar acepta `nombre`, `rutaFisica`, `rutaContenedor`, `descripcion`, `estaActiva` y `esPredeterminada`. El nombre es único (`409` si se repite).

## Escaneo de audio (`/api/escaneo-audio`)

| Método | Ruta | Auth | Descripción corta |
|---|---|---|---|
| `POST` | `` (base) | Admin | Inicia un escaneo; responde `202` |
| `GET` | `/actual` | Admin | Estado actual o último; `204` si no hay |
| `GET` | `/{id}` | Admin | Estado por ID |
| `POST` | `/{id}/cancelar` | Admin | Cancela un escaneo activo |

Solo puede haber un escaneo activo a la vez (`409`). Los trabajos se guardan en memoria y se pierden al reiniciar la API.

## Dashboard (`/api/dashboard`)

| Método | Ruta | Auth actual | Descripción corta |
|---|---|---|---|
| `GET` | `` (base) | Sin filtro en el controlador | Totales y listados recientes |

> [!WARNING]
> El controlador del dashboard no exige token actualmente. Protégelo en el reverse proxy si la API se expone fuera de una red de confianza.

## Endpoints heredados

| Método | Ruta | Auth | Nota |
|---|---|---|---|
| `POST` | `/api/pista` | `RoleAuthorize` admin | Creación heredada con `multipart`; usa `/api/pista/subir` |
| `PUT` | `/api/pista?id={id}` | `RoleAuthorize` admin | Edición básica; usa los flujos de metadatos/mis pistas |
| `DELETE` | `/api/pista/{id}` | `RoleAuthorize` admin | Borrado heredado; no recomendado para clientes externos |
| `GET` | `/buscar?cadena=...` | Público filtrado | Búsqueda heredada, fuera de `/api/pista` |
| `PUT` | `/reproducir/{id}` | Público | Incrementa reproducciones, fuera de `/api/pista` |

`RoleAuthorize` confía en `X-Usuario-Tipo` y no valida el token. No expongas estos endpoints a aplicaciones externas no confiables.

## Códigos de error

| Código | Significado |
|---|---|
| `200` | Éxito |
| `201` | Recurso creado |
| `202` | Trabajo aceptado (escaneo) |
| `204` | Sin contenido |
| `400` | Validación o estado inválido |
| `401` | Token ausente, inválido o expirado |
| `403` | Sin permisos para el recurso |
| `404` | No encontrado, incluida una pista privada no autorizada |
| `409` | Conflicto (duplicado, escaneo activo, ruta repetida) |
| `500` | Error interno |
| `503` | Sistema sin administrador (bootstrap) |

Para los flujos completos, ejemplos de subida, streaming, playlists y polling del escaneo, consulta [integracion.md](./integracion.md).
