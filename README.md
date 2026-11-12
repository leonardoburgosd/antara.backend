# Antara

Plataforma integral de gestión, escaneo y streaming de biblioteca musical compuesta por:
- **`antara.backend`**: API REST en .NET 10 (Clean Architecture, Dapper, PostgreSQL 16).
- **`antara.admin`**: Panel de administración web en React 19 + Vite 8 + Tailwind CSS 4 (Feature-Based Architecture).

---

## 📂 Estructura del Repositorio

```
.
├── antara.backend/             # Código fuente y documentación del backend (.NET 10)
│   ├── src/                    # Clean Architecture: Domain, Application, Infrastructure, Api
│   ├── tests/                  # Pruebas unitarias
│   ├── database/               # Scripts y seeds de base de datos
│   ├── docs/                   # 📚 Documentación técnica del backend
│   └── Dockerfile              # Construcción multi-stage de la API
│
├── antara.admin/               # Código fuente y documentación del frontend (React 19)
│   ├── src/                    # Feature-Based: features, components/ui, services
│   ├── docs/                   # 📚 Documentación técnica del frontend
│   └── Dockerfile              # Construcción multi-stage Nginx del frontend
│
├── docker-compose.yml          # Despliegue de PostgreSQL, API y Admin Web
└── README.md                   # Este archivo
```

---

## 📚 Documentación Técnica Detallada

Cada subproyecto contiene su propia carpeta `docs/` con especificaciones completas para guiar futuras implementaciones:

### ⚙️ Backend (`antara.backend/docs/`)
- [**README / Índice**](antara.backend/docs/README.md)
- [**architecture.md**](antara.backend/docs/architecture.md): Principios de Clean Architecture, reglas de dependencia entre capas y flujo de requests.
- [**domain.md**](antara.backend/docs/domain.md): Entidades de negocio, diagrama ER, invariantes y excepciones.
- [**features.md**](antara.backend/docs/features.md): Guía paso a paso para crear un feature (Entidad → Repositorio → DTO → Servicio → Controller → DB).
- [**api.md**](antara.backend/docs/api.md): Catálogo de endpoints HTTP, autenticación requerida y modelos de datos.
- [**database.md**](antara.backend/docs/database.md): Tablas, funciones almacenadas y migraciones automáticas (`DatabaseInitializer`).
- [**metadata.md**](antara.backend/docs/metadata.md): Proveedores externos (Deezer, TheAudioDB, MusicBrainz, iTunes, LRCLIB y Lyrics.ovh), rate limiting y fallback.
- [**security.md**](antara.backend/docs/security.md): Tokens HMAC-SHA256, hash BCrypt y filtros de autorización.
- [**audio-storage.md**](antara.backend/docs/audio-storage.md): Mapeo de rutas Windows en Linux (`HostPathResolver`), streaming y escaneo concurrente.

### 🎨 Frontend (`antara.admin/docs/`)
- [**README / Índice**](antara.admin/docs/README.md)
- [**architecture.md**](antara.admin/docs/architecture.md): Arquitectura Feature-Based, desacoplamiento y reglas de importación.
- [**features.md**](antara.admin/docs/features.md): Guía paso a paso para crear un nuevo módulo (Service → Hook → Componentes → Página → Nav).
- [**api-layer.md**](antara.admin/docs/api-layer.md): Cliente centralizado `apiClient`, interceptores, manejo de 401 y `endpoints.js`.
- [**components.md**](antara.admin/docs/components.md): Catálogo de componentes UI y sistema de iconos SVG.
- [**state.md**](antara.admin/docs/state.md): Gestión de estado local con React Hooks y persistencia en `localStorage`.
- [**routing.md**](antara.admin/docs/routing.md): Enrutamiento con React Router 7 y navegación por pestañas en Dashboard.
- [**audio-player.md**](antara.admin/docs/audio-player.md): Hook `useAudioPlayer`, reproductor flotante, control de volumen y modal de metadatos.

---

## 🚀 Despliegue con Docker Compose

Para levantar todo el ecosistema (Base de datos, Backend y Frontend):

```bash
./scripts/montar-unidades.sh --build
```

El script detecta las unidades del host y genera `docker-compose.unidades.yml` con
un montaje por unidad (`C:/` → `/host/c`, `H:/` → `/host/h`, …), de modo que las
rutas que registres en el panel sean visibles desde el contenedor sin editar el
compose. Los montajes son de **escritura**, porque el panel permite subir canciones
a esas carpetas; si preferís solo lectura, ejecutalo con `UNIDADES_SOLO_LECTURA=1`.
Vuelve a ejecutarlo cuando agregues una unidad o conectes un disco nuevo:
reescribe el archivo y recrea el servicio `api`.

Si no necesitas carpetas del host, `docker compose up -d --build` sigue siendo válido,
pero las rutas registradas con una letra de unidad no serán accesibles.

> En Linux/macOS no hay letras de unidad: pasa las carpetas con
> `RUTAS_EXTRA=/mnt/musica ./scripts/montar-unidades.sh` o usa directamente la ruta
> del contenedor al registrar la ruta en el panel.

### Servicios expuestos:
- **Panel Web**: [http://localhost](http://localhost) (Puerto 80)
- **API REST**: [http://localhost:8080](http://localhost:8080) (Puerto 8080)
- **PostgreSQL**: `localhost:5432` (Usuario y BD: `postgres` / `antaradb`)

---

## 🌐 Acceso remoto con Tailscale

El compose incluye un nodo Tailscale opcional que publica el panel dentro de tu
tailnet por HTTPS, sin abrir puertos en el router. La clave se configura en `.env`:

```bash
TAILSCALE_AUTHKEY=tskey-auth-xxxxxxxxxxxx
TAILSCALE_HOSTNAME=antara
```

Genera la clave en <https://login.tailscale.com/admin/settings/keys> (marcá
*Reusable*, sin *Ephemeral*). Después levantá el perfil:

```bash
# Una vez configurada la clave
docker compose --profile tailscale up -d
docker compose logs -f tailscale   # muestra la URL https://<hostname>.<tailnet>.ts.net
```

El entrypoint (`tailscale/entrypoint.sh`) arranca el nodo y, cuando la sesión está
activa, publica el servicio `web` (Nginx) con `tailscale serve` usando el nombre
DNS real del nodo. El modo se elige en `.env`:

| `TAILSCALE_SERVE_MODE` | Resultado |
|---|---|
| `http` (por defecto) | `http://<hostname>.<tu-tailnet>.ts.net` en el puerto 80, sin certificados |
| `https` | `https://<hostname>.<tu-tailnet>.ts.net`, requiere *HTTPS certificates* habilitado en <https://login.tailscale.com/admin/dns> |
| `ninguno` | El nodo entra al tailnet pero no publica el panel |

El destino se cambia con `TAILSCALE_SERVE_URL` (por defecto `http://web:80`). Si
cambiás el modo, recreá el contenedor:

```bash
docker compose --profile tailscale up -d --force-recreate tailscale
```

Para detenerlo: `docker compose --profile tailscale stop tailscale`.

La identidad del nodo se guarda en el volumen `antara-tailscale-state`, así que no
hace falta una clave nueva en cada reinicio.

> El contenedor usa *userspace networking*, por lo que funciona en Docker Desktop
> (Windows/macOS) sin `NET_ADMIN` ni `/dev/net/tun`.

