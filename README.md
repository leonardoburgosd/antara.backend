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
- [**metadata.md**](antara.backend/docs/metadata.md): Proveedores externos (Deezer, TheAudioDB, Lyrics.ovh), rate limiting y fallback.
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
docker compose up -d --build
```

### Servicios expuestos:
- **Panel Web**: [http://localhost](http://localhost) (Puerto 80)
- **API REST**: [http://localhost:8080](http://localhost:8080) (Puerto 8080)
- **PostgreSQL**: `localhost:5432` (Usuario y BD: `postgres` / `antaradb`)

