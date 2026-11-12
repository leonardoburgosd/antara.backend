# Antara Backend — Documentación Técnica

**Plataforma de gestión de biblioteca musical** desarrollada con .NET 10, C#, Dapper y PostgreSQL.

> [!NOTE]
> Esta documentación describe la arquitectura, convenciones y flujos reales del proyecto. Está pensada para que cualquier desarrollador nuevo pueda entender el sistema y agregar funcionalidades sin romper lo existente.

---

## Tecnologías

| Tecnología | Versión | Rol |
|---|---|---|
| .NET | 10 | Runtime y framework web |
| C# | 13 | Lenguaje principal |
| Dapper | 2.1.35 | Micro-ORM para consultas SQL |
| Npgsql | 8.0.5 | Driver PostgreSQL para .NET |
| PostgreSQL | 16 | Base de datos relacional |
| BCrypt.Net-Next | 4.0.3 | Hashing de contraseñas |
| Swashbuckle.AspNetCore | 10.3.1 | Dependencia incluida; Swagger UI no está habilitado en `Program.cs` |
| Newtonsoft.Json | 13.0.1 | Serialización JSON auxiliar |

---

## Índice de documentación

| Documento | Contenido |
|---|---|
| [integracion.md](./integracion.md) | Guía para consumir la API desde otra aplicación: autenticación, módulos, pasos y ejemplos |
| [architecture.md](./architecture.md) | Clean Architecture, capas, reglas de dependencia, flujo de request |
| [domain.md](./domain.md) | Entidades, relaciones, excepciones de dominio, reglas de negocio |
| [features.md](./features.md) | Guía paso a paso para agregar una nueva funcionalidad |
| [api.md](./api.md) | Índice exacto de endpoints, autenticación y reglas de acceso |
| [database.md](./database.md) | Tablas, procedimientos almacenados, migraciones |
| [metadata.md](./metadata.md) | Sistema de metadatos: Deezer, TheAudioDB, MusicBrainz, iTunes, LRCLIB y LyricsOvh |
| [security.md](./security.md) | Tokens HMAC-SHA256, BCrypt, filtros de autorización |
| [audio-storage.md](./audio-storage.md) | Rutas de audio, HostPathResolver, escaneo de biblioteca |

---

## Estructura del repositorio

```
antara.backend/
├── src/
│   ├── AntaraSoft.Domain/        ← Entidades y reglas de negocio
│   ├── AntaraSoft.Application/   ← Contratos, DTOs, servicios de aplicación
│   ├── AntaraSoft.Infrastructure/← Dapper, metadatos, storage, seguridad
│   └── AntaraSoft.Api/           ← Controllers, filtros, middleware, DI
├── tests/
│   └── AntaraSoft.UnitTests/     ← Tests unitarios (17 tests)
├── database/
│   ├── scripts/init_schema.sql   ← Referencia del esquema
│   └── seeds/generos_seed.sql    ← Datos semilla de géneros
├── Dockerfile                    ← Build multi-stage para producción
└── AntaraSoft.sln
```

---

## Inicio rápido

```bash
# Desde la raíz del repositorio principal (antara.backend/)
docker compose up -d

# Verificar que todo esté saludable
curl http://localhost:8080/api/sistema/health
```

El sistema inicializa la base de datos automáticamente al arrancar (`DatabaseInitializer`). No se requiere ejecutar scripts SQL manualmente.

