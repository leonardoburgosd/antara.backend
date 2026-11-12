# Integración del backend Antara desde otra aplicación

> **Descripción corta:** Antara es una API REST para gestionar una biblioteca musical, sus usuarios, álbumes, playlists, rutas de almacenamiento y metadatos. Está construida con .NET 10, Dapper y PostgreSQL; autentica con un token propio firmado con HMAC-SHA256 y expone el audio mediante streaming con soporte de `Range`.

Esta guía está pensada para una aplicación externa (web, móvil, escritorio u otro servicio) que necesite consumir el backend. Para el detalle campo por campo de cada DTO, usa también [api.md](./api.md).

---

## 1. Contrato general

| Elemento | Valor |
|---|---|
| URL base | `http://<host>:<API_PORT>` |
| Puerto por defecto | `8080` (`API_PORT` en `.env`) |
| Prefijo de negocio | `/api` |
| Formato de entrada/salida | JSON `application/json`, salvo subidas y portadas |
| Fechas | ISO 8601 (`2026-11-12T18:30:00Z`) |
| Identificadores | GUID |
| Autenticación | `Authorization: Bearer <token>` |
| CORS | `AllowAll` en el código actual |
| Documento OpenAPI/Swagger | No está habilitado en `Program.cs`; este documento y `api.md` son la referencia |

> [!IMPORTANT]
> La URL real depende de cómo se publique la API. En Docker, `API_PORT` es `8080` por defecto. Si el entorno local usa otro puerto, la aplicación externa debe recibirlo por configuración, no hardcodearlo.

### Arquitectura que hay detrás

```text
Aplicación externa
        │ HTTP/JSON o multipart
        ▼
AntaraSoft.Api (Controllers + filtros)
        ▼
AntaraSoft.Application (servicios y DTOs)
        ▼
AntaraSoft.Infrastructure (Dapper, PostgreSQL, archivos, metadatos, yt-dlp)
```

- **Controllers:** validan la petición, aplican autorización y traducen excepciones a códigos HTTP.
- **Servicios:** orquestan reglas de negocio; no contienen SQL.
- **Repositorios:** ejecutan funciones almacenadas de PostgreSQL.
- **Infraestructura:** resuelve rutas del host, guarda audio, descarga YouTube, consulta proveedores de metadatos y analiza duración.

---

## 2. Puesta en marcha de la integración

### Paso 1 — Comprobar disponibilidad

```http
GET /api/sistema/health
```

No requiere token. Si `adminsRegistrados` es `false`, el sistema está en modo bootstrap y **bloquea casi todos los endpoints** con `503`.

```json
{
  "activo": true,
  "version": "1.0.0",
  "timestamp": "2026-11-12T18:30:00Z",
  "usuariosTotales": 1,
  "adminsRegistrados": true,
  "estado": "Operativo"
}
```

### Paso 2 — Crear el primer administrador (solo una vez)

```http
POST /api/sistema/crea-primero-admin
Content-Type: application/json
```

```json
{
  "email": "admin@ejemplo.com",
  "password": "mínimo-6-caracteres",
  "nombre": "Administrador",
  "fechaNacimiento": "1990-01-01T00:00:00Z",
  "genero": "M",
  "pais": "PE"
}
```

Respuesta `201` con el usuario creado. Si ya existe un administrador, responde `400`. Este endpoint es el único de creación que funciona sin token durante el bootstrap.

### Paso 3 — Iniciar sesión

```http
POST /api/usuario/login
Content-Type: application/json
```

```json
{
  "email": "admin@ejemplo.com",
  "password": "mínimo-6-caracteres"
}
```

Respuesta `200`:

```json
{
  "id": "7f7d2e7e-1b1a-4d25-9f60-0d9f9e5e2f11",
  "email": "admin@ejemplo.com",
  "nombre": "Administrador",
  "tipo": "admin",
  "token": "base64url(payload).base64url(firma)"
}
```

> [!NOTE]
> El token **no es un JWT estándar**. Tiene el formato `base64url(payload).base64url(firma)` y se valida con el secreto del servidor. La aplicación externa debe tratarlo como opaco: guardarlo, enviarlo y descartarlo al expirar. No debe intentar validar su firma con una librería JWT.

La vigencia por defecto es de **8 horas** (`AppSettings__TokenExpiracionHoras`). No hay endpoint de refresh ni de revocación: al recibir `401`, hay que volver a autenticarse.

### Paso 4 — Enviar el token

```http
Authorization: Bearer <token>
```

El filtro inyecta el usuario autenticado en la petición. Los roles reconocidos son `admin`/`administrador` y `usuario`/`user`. Los endpoints administrativos exigen uno de los dos primeros.

### Paso 5 — Interpretar errores

| Código | Cuándo ocurre | Qué debe hacer la app externa |
|---|---|---|
| `200` | Consulta o actualización correcta | Leer el cuerpo |
| `201` | Recurso creado | Leer el cuerpo y, si aplica, `Location` |
| `202` | Trabajo aceptado en segundo plano (escaneo) | Consultar el estado por ID o `/actual` |
| `204` | Sin contenido (no hay escaneo actual) | Tratarlo como ausencia de dato |
| `400` | Datos inválidos, archivo no permitido, ruta no escribible | Corregir la petición; mostrar `error` |
| `401` | Token ausente, inválido o expirado | Volver a hacer login |
| `403` | Token válido sin rol/permiso suficiente | No reintentar con el mismo usuario |
| `404` | Recurso inexistente o pista privada no autorizada | No revelar detalles al usuario final |
| `409` | Conflicto (escaneo ya activo, ruta duplicada, etc.) | Esperar o resolver el estado |
| `503` | No existe administrador (bootstrap) | Crear el primer administrador |

El formato habitual de error es:

```json
{ "error": "Mensaje legible para el cliente." }
```

> [!WARNING]
> Algunos endpoints heredados devuelven `500` con un texto plano o con el mensaje de una excepción. No bases la lógica de negocio en esos mensajes; usa el código HTTP y, si necesitas detalle, migra a los endpoints recomendados.

---

## 3. Mapa de módulos

| Módulo | Controlador | Ruta base | Uso principal |
|---|---|---|---|
| Sistema | `SistemaController` | `/api/sistema` | Health, bootstrap y unidades del host |
| Usuarios y sesión | `UsuarioController` | `/api/usuario` | Login, perfil y CRUD de usuarios |
| Pistas de la biblioteca | `PistaController` | `/api/pista` | Catálogo, subidas, metadatos, stream y descarga |
| Mis pistas | `MisPistasController` | `/api/mis-pistas` | Pistas privadas de un usuario |
| Álbumes | `AlbumController` | `/api/album` | Catálogo de álbumes |
| Playlists | `PlaylistController` | `/api/playlist` | Playlists y sus canciones |
| Rutas de audio | `RutaAudioController` | `/api/rutas-audio` | Carpetas de almacenamiento |
| Escaneo | `EscaneoAudioController` | `/api/escaneo-audio` | Importación masiva en segundo plano |
| Dashboard | `DashboardController` | `/api/dashboard` | Métricas agregadas |

---

## 4. Sistema

### Descripción corta

Expone el estado del servicio, las unidades del host visibles desde el contenedor y el bootstrap del primer administrador.

### Endpoints

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/api/sistema/health` | Público | Estado, versión y número de usuarios |
| `GET` | `/api/sistema/unidades-host` | Admin | Letras de unidad montadas en la API |
| `POST` | `/api/sistema/crea-primero-admin` | Público solo en bootstrap | Crea el primer administrador |
| `GET` | `/api/sistema/{id}` | Público | Devuelve un usuario por ID (endpoint heredado) |

### Pasos que realiza el servicio

1. `health` consulta si existe al menos un administrador y cuenta los usuarios.
2. Devuelve un `SistemaDto`; si la base falla, responde `500` con `activo: false`.
3. `crea-primero-admin` valida correo, contraseña mínima de 6 caracteres y nombre.
4. Comprueba que no exista ya un administrador; si existe, rechaza la operación.
5. Crea el usuario con `tipo = "admin"`, hashea la contraseña y responde `201`.
6. `unidades-host` lee los montajes reales del contenedor y devuelve las letras disponibles.

> [!NOTE]
> Mientras no exista administrador, `AdminBootstrapFilter` solo deja pasar `GET /api/sistema/health` y `POST /api/sistema/crea-primero-admin`. El resto responde `503`.
>
> `GET /api/sistema/{id}` no tiene filtro de autenticación en el código actual. No lo uses para exponer datos de usuarios fuera de la red interna.

---

## 5. Autenticación, usuarios y perfil

### Descripción corta

Gestiona el login, el perfil del usuario autenticado y el CRUD administrativo de cuentas.

### Endpoints

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `POST` | `/api/usuario/login` | Público | Login y emisión del token |
| `GET` | `/api/usuario/perfil` | Cualquier token | Perfil propio |
| `PUT` | `/api/usuario/perfil` | Cualquier token | Edita datos propios |
| `POST` | `/api/usuario/perfil/password` | Cualquier token | Cambia la contraseña propia |
| `GET` | `/api/usuario/todos` | Admin | Lista de usuarios |
| `POST` | `/api/usuario` | Admin | Crea un usuario |
| `PUT` | `/api/usuario/{id}` | Admin | Edita cualquier usuario |
| `DELETE` | `/api/usuario/{id}` | Admin | Elimina un usuario |
| `GET` | `/api/usuario/{id}` | Admin | Consulta un usuario |

### Pasos que realiza el servicio

**Login**

1. Busca el usuario por correo.
2. Verifica la contraseña con BCrypt.
3. Vuelve a cargar el usuario y genera el token con `sub`, `email`, `nombre`, `tipo` y `exp`.
4. Devuelve los datos públicos y el token. Si el correo no existe o la contraseña no coincide, el controlador actual responde `404`; no emite un `401` específico de credenciales. La aplicación externa debe tratar ambos casos como fallo de login.

**Crear usuario**

1. Valida correo, contraseña (mínimo 6) y nombre.
2. Comprueba que el correo sea único.
3. Normaliza `tipo` a `admin` o `usuario`.
4. Hashea la contraseña con BCrypt y guarda la cuenta.
5. Responde `201` con el usuario creado (sin contraseña).

**Editar usuario**

1. Carga el usuario y conserva los campos ausentes.
2. Valida el correo y su unicidad.
3. Si se envía contraseña, la hashea; si no, conserva la actual.
4. Impide dejar el sistema sin al menos un administrador activo.
5. Actualiza y devuelve el usuario.

**Eliminar usuario**

1. Impide que un administrador se elimine a sí mismo.
2. Impide eliminar al último administrador.
3. Elimina físicamente la cuenta; si tiene álbumes o playlists asociados, la base puede rechazar la operación.

**Perfil y contraseña**

1. El ID siempre se toma del token, nunca del cuerpo ni de la URL.
2. `PUT /perfil` solo permite cambiar datos personales; no permite cambiar rol ni estado.
3. `POST /perfil/password` exige la contraseña actual y una nueva de al menos 6 caracteres.

### Ejemplo de consumo

```bash
BASE="http://localhost:8080"

TOKEN=$(curl -s -X POST "$BASE/api/usuario/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@ejemplo.com","password":"mi-password"}' \
  | python -c "import json,sys; print(json.load(sys.stdin)['token'])")

curl -s "$BASE/api/usuario/perfil" \
  -H "Authorization: Bearer $TOKEN"
```

---

## 6. Biblioteca de pistas (administración)

### Descripción corta

Es el catálogo global de canciones. Permite listar, subir archivos o audio de YouTube, enriquecer metadatos, obtener letras, descargar paquetes ZIP y reproducir audio.

### Endpoints

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/api/pista/todos` | Admin | Biblioteca completa |
| `GET` | `/api/pista/subir/rutas` | Admin | Rutas activas que pueden recibir archivos |
| `POST` | `/api/pista/subir` | Admin | Sube un archivo de audio (`multipart/form-data`) |
| `POST` | `/api/pista/subir/url` | Admin | Descarga audio de YouTube |
| `GET` | `/api/pista/metadatos/proveedores` | Admin | Proveedores habilitados |
| `GET` | `/api/pista/metadatos/buscar` | Admin | Busca candidatos de metadatos |
| `POST` | `/api/pista/{id}/metadatos` | Admin | Aplica un candidato a una pista |
| `GET` | `/api/pista/{id}/letra-sincronizada` | Admin | Letra LRC desde LRCLIB |
| `GET` | `/api/pista/{id}/descarga` | Admin | ZIP con audio, portada, letras y JSON |
| `GET` | `/api/pista/{id}` | Público/privado | Stream con `Range`; privadas exigen token |
| `GET` | `/api/pista/todos/album/{albumId}` | Público filtrado | Pistas de un álbum |
| `GET` | `/api/pista/todos/playlist/{playlistId}` | Público filtrado | Pistas de una playlist |

### Pasos del servicio: subir un archivo

1. Verifica el token de administrador y que llegue un archivo no vacío.
2. Valida la extensión contra la lista permitida: `.mp3`, `.flac`, `.wav`, `.m4a`, `.aac`, `.ogg`, `.opus`, `.wma`, `.aiff`, `.alac`.
3. Resuelve la ruta de destino: usa `rutaId` si se envía; si no, la ruta predeterminada activa.
4. Rechaza rutas inexistentes, inactivas o no escribibles.
5. Sanea `subcarpeta` (sin `..`, rutas absolutas ni dos puntos) y crea la carpeta dentro de la raíz permitida.
6. Genera un nombre disponible para no sobrescribir archivos existentes.
7. Copia el archivo al disco.
8. Analiza nombre, duración y metadatos; crea o reutiliza álbum y género.
9. Inserta la pista en la biblioteca y responde `201` con `SubidaPistaDto`.

**Campos multipart**

| Campo | Obligatorio | Descripción |
|---|---|---|
| `archivo` | Sí | Archivo de audio |
| `rutaId` | No | GUID de `/api/pista/subir/rutas`; si falta, se usa la predeterminada |
| `subcarpeta` | No | Carpeta relativa, por ejemplo `Rock/2026` |

**Respuesta típica**

```json
{
  "pistaId": "uuid",
  "nombre": "Bohemian Rhapsody",
  "interprete": "Queen",
  "album": "A Night at the Opera",
  "conMetadatos": true,
  "archivo": "/music/Queen/Bohemian Rhapsody.mp3",
  "rutaDestino": "/music/Queen",
  "rutaId": "uuid",
  "rutaNombre": "Biblioteca principal",
  "duracionSegundos": 354,
  "duracionMinutos": 5.9,
  "duracion": "5:54",
  "mensaje": "..."
}
```

### Pasos del servicio: subir desde YouTube

1. Valida que la URL pertenezca a `youtube.com`, `music.youtube.com`, `m.youtube.com` o `youtu.be`.
2. Resuelve y valida la ruta de destino igual que en una subida de archivo.
3. Ejecuta `yt-dlp` con `ffmpeg` para descargar el mejor audio y convertirlo a MP3, ignorando playlists.
4. Comprueba que se haya generado el archivo.
5. Importa la pista con el mismo enriquecimiento de metadatos que un archivo local.
6. Responde `201`; el tiempo máximo de descarga es configurable (`600` segundos por defecto).

```bash
curl -X POST "$BASE/api/pista/subir/url" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"url":"https://www.youtube.com/watch?v=...","rutaId":"uuid-opcional","subcarpeta":"Rock/2026"}'
```

### Pasos del servicio: buscar y aplicar metadatos

**Buscar**

1. Exige `cadena` no vacía.
2. Si `proveedor` es `todos`, consulta todos los proveedores habilitados en paralelo. Envía `proveedor=todos` explícitamente desde la app externa; no dependas de omitir el parámetro.
3. Si se indica un proveedor concreto, lo busca por nombre o etiqueta; si no existe, responde `400` con la lista de disponibles (no cae silenciosamente a otro proveedor).
4. Filtra resultados válidos y devuelve `MetadatosCandidatoDto`.

**Aplicar**

1. Localiza la pista y el candidato por `idExterno`.
2. Si se indicó proveedor, lo usa; si no, recorre los proveedores hasta encontrar el ID.
3. Intenta completar la letra plana con el proveedor de letras (opcional).
4. Crea o reutiliza el álbum y el género.
5. Actualiza título, artista, año, duración, portada, imagen de artista, letra y fuente.
6. Devuelve la pista actualizada.

```bash
curl -G "$BASE/api/pista/metadatos/buscar" \
  -H "Authorization: Bearer $TOKEN" \
  --data-urlencode "cadena=Queen Bohemian Rhapsody" \
  --data-urlencode "proveedor=todos"
```

```bash
curl -X POST "$BASE/api/pista/{id}/metadatos" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"idExterno":"912345","proveedor":"deezer"}'
```

### Pasos del servicio: reproducir una pista

1. Busca la pista en la biblioteca.
2. Si `usuarioId` es nulo, es una pista global y se puede reproducir sin token.
3. Si es una pista propia, exige un token válido del dueño o de un administrador.
4. Acepta el token en `Authorization: Bearer` o en `?token=`, porque `<audio>` no puede enviar encabezados.
5. Resuelve la ruta física del archivo; si no existe, responde `404`.
6. Devuelve el archivo con `Accept-Ranges: bytes` y procesamiento de rangos para permitir seek.

> [!IMPORTANT]
> Si una pista privada no existe o el token no autoriza, la API responde `404`, no `403`, para no revelar su existencia. La aplicación externa debe tratar ambos casos como “no disponible”.

### Pasos del servicio: descargar un ZIP

1. Exige administrador.
2. Resuelve la pista y su archivo de audio.
3. Intenta obtener portada y letra sincronizada; si fallan, el ZIP se genera igualmente.
4. Escribe el audio sin recomprimir, la portada, `letra.txt`, `letra.lrc` (si existe) y `pista.json`.
5. Devuelve `application/zip` con `Content-Disposition: attachment`.

### Límites y consideraciones

- El tamaño máximo de subida es `2048 MB` por defecto (`AppSettings__SubidaAudioMaxMb`).
- La descarga de YouTube tiene un timeout de `600` segundos por defecto.
- La API no impone un límite de pistas por petición: cada archivo es una petición independiente. Una app externa puede implementar su propia cola.
- `GET /api/pista/todos` devuelve toda la biblioteca, sin paginación. Para catálogos grandes, filtra en el cliente o divide por álbum/playlist.

---

## 7. Mis pistas (usuarios normales)

### Descripción corta

Permite que un usuario autenticado suba y gestione sus propias canciones. Siempre se guardan en la ruta predeterminada y quedan aisladas por propietario; un administrador puede ver todas.

### Endpoints

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/api/mis-pistas` | Usuario/Admin | Pistas propias; admin ve todas |
| `POST` | `/api/mis-pistas/subir` | Usuario/Admin | Sube un archivo propio |
| `POST` | `/api/mis-pistas/subir/url` | Usuario/Admin | Descarga YouTube a la ruta predeterminada |
| `GET` | `/api/mis-pistas/{id}` | Dueño/Admin | Detalle de una pista |
| `PUT` | `/api/mis-pistas/{id}` | Dueño/Admin | Edita datos y portada/álbum |
| `GET` | `/api/mis-pistas/metadatos/proveedores` | Usuario/Admin | Proveedores disponibles |
| `GET` | `/api/mis-pistas/metadatos/buscar` | Usuario/Admin | Busca candidatos |
| `POST` | `/api/mis-pistas/{id}/metadatos` | Dueño/Admin | Reasigna metadatos |
| `GET` | `/api/mis-pistas/{id}/letra-sincronizada` | Dueño/Admin | Letra LRC |

### Pasos que realiza el servicio

**Subida propia**

1. Toma el `UsuarioId` del token.
2. Exige que exista una ruta activa marcada como predeterminada; no acepta `rutaId`.
3. Valida extensión, subcarpeta y nombre.
4. Guarda el archivo y lo importa con `propietarioId = usuario autenticado`.
5. Responde `201` con `SubidaPistaDto`.

**Listado y detalle**

1. Un administrador recibe toda la biblioteca.
2. Un usuario normal recibe solo las pistas cuyo `UsuarioId` coincide con su token.
3. El detalle de una pista ajena responde `404` para no revelar su existencia.

**Edición**

1. Verifica que la pista exista y que el solicitante sea dueño o administrador.
2. Conserva los campos no enviados.
3. Si cambia `albumNombre` o `portadaUrl`, crea o reutiliza un álbum dentro del catálogo del propietario.
4. Si cambia `genero`, crea o reutiliza el género.
5. Actualiza la pista sin perder la letra ni la URL del archivo.

**Metadatos**

1. Verifica propiedad.
2. Busca el candidato por proveedor o en todos.
3. Mantiene el álbum resultante dentro del propietario.
4. Actualiza la pista y devuelve el DTO de biblioteca.

### Ejemplo de subida propia

```bash
curl -X POST "$BASE/api/mis-pistas/subir" \
  -H "Authorization: Bearer $TOKEN_USUARIO" \
  -F "archivo=@/ruta/local/cancion.mp3" \
  -F "subcarpeta=Mis canciones"
```

---

## 8. Álbumes

### Descripción corta

Administra el catálogo de álbumes. Las lecturas son públicas; la creación, edición, publicación y borrado requieren administrador.

### Endpoints

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/api/album/todos` | Admin | Todos los álbumes |
| `POST` | `/api/album` | Admin | Crea un álbum |
| `PUT` | `/api/album/{id}` | Admin | Edita un álbum |
| `PUT` | `/api/album/{id}/publicar` | Admin | Alterna publicado/borrador |
| `DELETE` | `/api/album/{id}` | Admin | Elimina un álbum |
| `GET` | `/api/album/{id}` | Público | Consulta un álbum |
| `GET` | `/api/album/todos/{userId}` | Público | Álbumes de un usuario |

### Pasos que realiza el servicio

**Crear**

1. Exige nombre no vacío.
2. Toma el `UsuarioId` del token.
3. Crea el álbum como activo, con `estaPublicado` según el cuerpo y fecha de publicación actual.
4. Responde `201` con el álbum creado.

**Editar**

1. Carga el álbum y conserva los campos ausentes.
2. Valida que el nombre no quede vacío.
3. Actualiza los campos permitidos.

**Publicar**

1. Carga el álbum.
2. Invierte `estaPublicado`.
3. Devuelve el álbum actualizado.

**Eliminar**

1. Comprueba que exista.
2. Intenta eliminarlo; si tiene pistas asociadas, responde `400` y pide quitar la relación primero.

> [!NOTE]
> `GET /api/album/{id}` y `GET /api/album/todos/{userId}` no exigen token en el código actual. Si el catálogo debe ser privado, la aplicación externa no debe asumir que la API lo protege.

---

## 9. Playlists

### Descripción corta

Gestiona playlists por usuario y las canciones que contienen. Un usuario normal solo puede operar sobre sus propias playlists; un administrador puede operar sobre cualquiera.

### Endpoints

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/api/playlist/todos` | Admin | Todas las playlists con resumen |
| `GET` | `/api/playlist/todos/{userId}` | Usuario/Admin | Playlists de un usuario |
| `GET` | `/api/playlist/{id}` | Usuario/Admin | Detalle de una playlist |
| `GET` | `/api/playlist/{id}/pistas` | Usuario/Admin | Canciones de la playlist |
| `POST` | `/api/playlist` | Usuario/Admin | Crea playlist (`multipart/form-data`) |
| `PUT` | `/api/playlist/{id}` | Usuario/Admin | Edita playlist (`multipart/form-data`) |
| `DELETE` | `/api/playlist/{id}` | Usuario/Admin | Elimina playlist |
| `POST` | `/api/playlist/{id}/pistas` | Usuario/Admin | Agrega una canción |
| `DELETE` | `/api/playlist/{id}/pistas/{pistaId}` | Usuario/Admin | Quita una canción |

### Pasos que realiza el servicio

**Crear**

1. Exige nombre.
2. Determina el propietario: un administrador puede enviar `usuarioId`; un usuario normal siempre queda como propietario.
3. Si se adjunta `imagenDePortada`, la sube y guarda su URL; si no, acepta `portadaUrl` opcional.
4. Crea la playlist activa y responde `201` con `Location`.

**Consultar**

1. Carga la playlist.
2. Si el solicitante no es administrador y no es el dueño, responde `403`.
3. Devuelve el DTO o la lista de canciones con datos de biblioteca.

**Editar**

1. Verifica existencia y propiedad.
2. Exige nombre.
3. Conserva la portada si no se envía imagen, URL ni `quitarPortada`.
4. Devuelve la playlist actualizada.

**Agregar canción**

1. Exige `pistaId`.
2. Verifica existencia y propiedad de la playlist.
3. Comprueba que la pista exista en la biblioteca.
4. Crea la relación playlist-pista.
5. Devuelve la lista actualizada de canciones.

**Quitar canción**

1. Verifica existencia y propiedad.
2. Elimina la relación.
3. Devuelve la lista actualizada.

### Ejemplo de creación

```bash
curl -X POST "$BASE/api/playlist" \
  -H "Authorization: Bearer $TOKEN" \
  -F "nombre=Favoritas" \
  -F "descripcion=Selección personal" \
  -F "imagenDePortada=@/ruta/local/portada.jpg"
```

Para un usuario normal, no envíes `usuarioId`: el backend lo ignora y usa el del token. Para un administrador, `usuarioId` permite crear la playlist a nombre de otro usuario.

---

## 10. Rutas de audio

### Descripción corta

Registra las carpetas del host que la API puede leer y escribir. Las subidas y los escaneos usan estas rutas; el cliente nunca envía una ruta física libre al subir un archivo.

### Endpoints

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/api/rutas-audio` | Admin | Lista rutas |
| `GET` | `/api/rutas-audio/{id}` | Admin | Consulta una ruta |
| `POST` | `/api/rutas-audio` | Admin | Crea una ruta |
| `PUT` | `/api/rutas-audio/{id}` | Admin | Actualiza una ruta |
| `DELETE` | `/api/rutas-audio/{id}` | Admin | Elimina una ruta |
| `POST` | `/api/rutas-audio/{id}/probar` | Admin | Comprueba acceso, escritura y archivos |

### Pasos que realiza el servicio

**Crear o actualizar**

1. Valida que haya nombre y al menos una ruta (física o de contenedor).
2. Comprueba que el nombre no esté duplicado; si lo está, responde `409`.
3. Normaliza la ruta física de Windows a la ruta visible dentro del contenedor.
4. Guarda la ruta y actualiza la caché de rutas activas.
5. Si se marca como predeterminada, quita esa marca de las demás; si no había predeterminada, el servicio elige una activa.

**Probar**

1. Resuelve la ruta efectiva.
2. Comprueba si existe, si es directorio y si es escribible.
3. Cuenta archivos de audio y subdirectorios.
4. Devuelve `PruebaRutaAudioDto` con el mensaje resultante.

**Eliminar**

1. Verifica que la ruta exista; si no, responde `404`.
2. Elimina el registro.
3. Si era la predeterminada, intenta marcar otra ruta activa como predeterminada.
4. Refresca la caché. Si ya no queda ninguna ruta predeterminada, las subidas propias fallarán hasta que un administrador marque una.

### Ejemplo

```json
{
  "nombre": "Biblioteca principal",
  "rutaFisica": "C:/Users/Leo/Music",
  "rutaContenedor": "/music",
  "descripcion": "Música principal",
  "estaActiva": true,
  "esPredeterminada": true
}
```

`rutaFisica` es la ruta del host; `rutaContenedor` es la ruta equivalente dentro del contenedor. El backend resuelve ambas con `HostPathResolver`.

---

## 11. Escaneo de audio

### Descripción corta

Importa en segundo plano todos los archivos de audio nuevos encontrados en las rutas activas, con enriquecimiento de metadatos y límite de concurrencia.

### Endpoints

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `POST` | `/api/escaneo-audio` | Admin | Inicia un escaneo |
| `GET` | `/api/escaneo-audio/actual` | Admin | Estado del escaneo actual o último |
| `GET` | `/api/escaneo-audio/{id}` | Admin | Estado por ID |
| `POST` | `/api/escaneo-audio/{id}/cancelar` | Admin | Cancela un escaneo activo |

### Pasos que realiza el servicio

1. Comprueba que no haya otro escaneo activo; si lo hay, responde `409`.
2. Crea un trabajo en memoria con estado `en_cola` y responde `202`.
3. En segundo plano, cambia a `descubriendo` y carga las rutas activas.
4. Enumera archivos de audio y compara con las rutas ya registradas para omitir duplicados.
5. Cambia a `procesando` y procesa los nuevos con `MaxDegreeOfParallelism` (4 por defecto).
6. Por cada archivo: analiza artista/título, consulta metadatos, crea o reutiliza álbum/género, inserta la pista y actualiza contadores.
7. Al terminar, cambia a `completado`, `cancelado` o `error` y guarda `finalizadoEn`.

**Estados posibles:** `en_cola`, `descubriendo`, `procesando`, `completado`, `cancelado`, `error`.

```bash
curl -X POST "$BASE/api/escaneo-audio" -H "Authorization: Bearer $TOKEN"
# 202 Accepted

curl "$BASE/api/escaneo-audio/actual" -H "Authorization: Bearer $TOKEN"
# 200 con el progreso, o 204 si nunca hubo un escaneo
```

> [!NOTE]
> Los trabajos de escaneo viven en memoria. Si la API se reinicia, el historial y el trabajo activo se pierden. La aplicación externa debe tolerar `204` y volver a iniciar el escaneo si es necesario.

---

## 12. Dashboard

### Descripción corta

Devuelve métricas agregadas para el panel: usuarios, álbumes, pistas, playlists, reproducciones y listados recientes.

### Endpoint

| Método | Ruta | Auth actual | Descripción |
|---|---|---|---|
| `GET` | `/api/dashboard` | Sin filtro en el controlador | Métricas y listados recientes |

### Pasos que realiza el servicio

1. Consulta los totales de usuarios, álbumes, pistas, playlists y reproducciones.
2. Obtiene las pistas recientes y las más reproducidas.
3. Obtiene usuarios y álbumes recientes.
4. Devuelve `DashboardDto` con `timestamp`.

> [!WARNING]
> En el código actual `GET /api/dashboard` **no exige token**. Está pensado para el panel interno. Si la API se expone fuera de la red de confianza, protégelo en el reverse proxy o agrega autorización antes de consumirlo desde una app pública.

---

## 13. Recetas de integración

### 13.1 Login y llamada autenticada

```javascript
const BASE = process.env.ANTARA_API_URL; // http://localhost:8080

async function login(email, password) {
  const respuesta = await fetch(`${BASE}/api/usuario/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  });

  if (!respuesta.ok) {
    throw new Error(`Login falló: ${respuesta.status}`);
  }

  return respuesta.json(); // { token, usuario... }
}

async function listarPistas(token) {
  const respuesta = await fetch(`${BASE}/api/pista/todos`, {
    headers: { Authorization: `Bearer ${token}` },
  });

  if (respuesta.status === 401) {
    // Token expirado: volver a autenticar.
  }

  if (!respuesta.ok) {
    throw new Error(`Error ${respuesta.status}`);
  }

  return respuesta.json();
}
```

### 13.2 Subir un archivo con progreso propio

`fetch` no informa el progreso de subida. Si la aplicación externa necesita una barra de progreso real, usa `XMLHttpRequest` o una librería que exponga `upload.onprogress`.

```javascript
function subirArchivo(token, archivo, rutaId, subcarpeta) {
  return new Promise((resolve, reject) => {
    const datos = new FormData();
    datos.append('archivo', archivo);
    if (rutaId) datos.append('rutaId', rutaId);
    if (subcarpeta) datos.append('subcarpeta', subcarpeta);

    const xhr = new XMLHttpRequest();
    xhr.open('POST', `${BASE}/api/pista/subir`);
    xhr.setRequestHeader('Authorization', `Bearer ${token}`);
    xhr.upload.onprogress = (evento) => {
      if (evento.lengthComputable) {
        console.log(`${Math.round((evento.loaded / evento.total) * 100)}%`);
      }
    };
    xhr.onload = () => {
      if (xhr.status === 201) resolve(JSON.parse(xhr.responseText));
      else reject(new Error(`Error ${xhr.status}: ${xhr.responseText}`));
    };
    xhr.onerror = () => reject(new Error('Error de red'));
    xhr.send(datos);
  });
}
```

### 13.3 Reproducir una pista privada

```html
<audio controls>
  <source src="http://localhost:8080/api/pista/UUID?token=TOKEN" type="audio/mpeg" />
</audio>
```

Para pistas globales puedes omitir `?token=`. Para pistas privadas, el token debe pertenecer al dueño o a un administrador.

### 13.4 Crear una playlist y agregar una canción

```javascript
const formulario = new FormData();
formulario.append('nombre', 'Favoritas');
formulario.append('descripcion', 'Selección personal');

const creada = await fetch(`${BASE}/api/playlist`, {
  method: 'POST',
  headers: { Authorization: `Bearer ${token}` },
  body: formulario,
}).then((r) => r.json());

await fetch(`${BASE}/api/playlist/${creada.id}/pistas`, {
  method: 'POST',
  headers: {
    Authorization: `Bearer ${token}`,
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({ pistaId: 'UUID_DE_LA_PISTA' }),
});
```

### 13.5 Escanear y hacer polling

```javascript
const inicio = await fetch(`${BASE}/api/escaneo-audio`, {
  method: 'POST',
  headers: { Authorization: `Bearer ${token}` },
});

if (inicio.status === 409) {
  // Ya hay un escaneo activo: consultar /actual.
}

let estado;
do {
  await new Promise((r) => setTimeout(r, 1500));
  const respuesta = await fetch(`${BASE}/api/escaneo-audio/actual`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  if (respuesta.status === 204) break; // no hay escaneo
  estado = await respuesta.json();
  console.log(estado.estado, estado.procesados, '/', estado.totalArchivos);
} while (estado?.activo);
```

---

## 14. Endpoints heredados (no recomendados para nuevas integraciones)

Estos endpoints existen en el código por compatibilidad con versiones anteriores. Varios usan `[RoleAuthorize]`, que confía en encabezados `X-Usuario-Tipo` y **no valida el token**; no los expongas directamente a una aplicación externa.

| Método | Ruta | Auth | Nota |
|---|---|---|---|
| `POST` | `/api/pista` | `RoleAuthorize` admin | Crea una pista con metadatos y archivo/ruta; usa el flujo moderno `/api/pista/subir` |
| `PUT` | `/api/pista?id={id}` | `RoleAuthorize` admin | Edita campos básicos; usa metadatos o el flujo de mis pistas |
| `DELETE` | `/api/pista/{id}` | `RoleAuthorize` admin | Elimina una pista; no recomendado para clientes externos |
| `GET` | `/buscar?cadena=...` | Público filtrado | Búsqueda heredada; no está bajo `/api/pista` |
| `PUT` | `/reproducir/{id}` | Público | Incrementa el contador de reproducciones; no está bajo `/api/pista` |

Si necesitas borrar o editar pistas desde una app externa, es preferible agregar un endpoint autenticado con `TokenAuthorize` en lugar de reutilizar estos.

---

## 15. Checklist para la aplicación externa

- [ ] Configurar `ANTARA_API_URL` y no hardcodear el puerto.
- [ ] Llamar a `/api/sistema/health` al arrancar.
- [ ] Si `adminsRegistrados` es `false`, mostrar el flujo de primer administrador.
- [ ] Guardar el token en almacenamiento seguro; no loguearlo ni incluirlo en URLs públicas.
- [ ] Enviar `Authorization: Bearer` en cada endpoint protegido.
- [ ] Renovar la sesión haciendo login de nuevo ante `401` (no hay refresh token).
- [ ] Tratar `403` como falta de permisos y `404` de pista privada como “no disponible”.
- [ ] Usar `multipart/form-data` para archivos y portadas; JSON para el resto.
- [ ] Usar una cola propia si se suben varios archivos; la API procesa una petición por archivo.
- [ ] Para archivos grandes, permitir timeouts amplios y progreso con XHR.
- [ ] No enviar rutas físicas arbitrarias: elegir un `rutaId` de `/api/pista/subir/rutas`.
- [ ] Hacer polling del escaneo cada 1–2 segundos y tolerar `204`.
- [ ] No asumir paginación: los listados devuelven el conjunto completo.
- [ ] Restringir CORS y proteger `/api/dashboard` en el proxy si la API se expone a Internet.
- [ ] No usar los endpoints heredados con `RoleAuthorize` desde clientes no confiables.

---

## 16. Resumen de límites actuales

| Límite | Valor actual | Configuración |
|---|---|---|
| Vigencia del token | 8 horas | `AppSettings__TokenExpiracionHoras` |
| Tamaño máximo de subida | 2048 MB | `AppSettings__SubidaAudioMaxMb` |
| Timeout de descarga YouTube | 600 s | `AppSettings__DescargaYouTubeTimeoutSegundos` |
| Paralelismo de escaneo | 4 | `AppSettings__EscaneoAudioMaxParalelo` |
| Timeout de proveedores de metadatos | 20 s | `AppSettings__MetadatosTimeoutSegundos` |
| Paginación | No implementada | — |
| Persistencia de escaneos | En memoria | Se pierde al reiniciar |
| Refresh token | No implementado | Volver a hacer login |

Para el detalle de la configuración de proveedores, almacenamiento y seguridad, consulta [metadata.md](./metadata.md), [audio-storage.md](./audio-storage.md) y [security.md](./security.md).
