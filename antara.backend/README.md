# Antara Backend

Backend de plataforma de gestión y streaming de biblioteca musical desarrollado con **.NET 10**, **Clean Architecture**, **Dapper** y **PostgreSQL**.

> 📚 **Documentación técnica completa disponible en [`docs/`](./docs/README.md)**

---

## 🚀 Tecnologías

- **.NET 10** (C# 13)
- **PostgreSQL 16**
- **Dapper 2.1.35**
- **Docker & Docker Compose**
- **BCrypt.Net-Next** (Seguridad de contraseñas)
- **Tokens HMAC-SHA256** (Autenticación sin estado)
- **Proveedores de Metadatos:** Deezer API, TheAudioDB API, MusicBrainz, iTunes, LRCLIB y Lyrics.ovh

---

## 🏛️ Arquitectura

El proyecto está estructurado siguiendo **Clean Architecture**:

- `src/AntaraSoft.Domain`: Entidades inmutables y reglas de negocio.
- `src/AntaraSoft.Application`: Contratos, DTOs y orquestación de casos de uso.
- `src/AntaraSoft.Infrastructure`: Acceso a PostgreSQL con Dapper, integración con APIs externas de metadatos, almacenamiento de audio y seguridad.
- `src/AntaraSoft.Api`: Controladores REST, filtros de autorización, inicialización automática y middlewares.
- `tests/AntaraSoft.UnitTests`: Suite de pruebas unitarias.

---

## 📖 Documentación para Desarrolladores

Para comprender cómo construir nuevas funcionalidades, respetar las reglas arquitectónicas y conocer las limitaciones:

1. [Índice de Documentación](docs/README.md)
2. [Arquitectura y Reglas de Dependencia](docs/architecture.md)
3. [Modelo de Dominio](docs/domain.md)
4. [Guía para Construir Nuevas Features](docs/features.md)
5. [Guía de Integración para Aplicaciones Externas](docs/integracion.md)
6. [Referencia de Endpoints (API)](docs/api.md)
7. [Base de Datos y Procedimientos Almacenados](docs/database.md)
8. [Sistema de Enriquecimiento de Metadatos](docs/metadata.md)
9. [Seguridad y Autenticación](docs/security.md)
10. [Almacenamiento y Escaneo de Audio](docs/audio-storage.md)

---

## 🧪 Pruebas Unitarias

```bash
dotnet test tests/AntaraSoft.UnitTests/AntaraSoft.UnitTests.csproj
```
