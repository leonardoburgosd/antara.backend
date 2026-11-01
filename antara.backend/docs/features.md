# Guía para agregar una nueva funcionalidad

Esta guía describe el proceso completo y obligatorio para agregar cualquier funcionalidad nueva al sistema. Sigue los pasos en orden.

> [!IMPORTANT]
> No te saltes ningún paso. El sistema está diseñado con separación estricta de responsabilidades. Poner lógica en la capa equivocada rompe la arquitectura.

---

## Los 8 pasos

### Paso 1 — Entidad en Domain

Crea el archivo en `src/AntaraSoft.Domain/Entities/`.

```csharp
// src/AntaraSoft.Domain/Entities/Genero.cs
namespace AntaraSoft.Domain.Entities
{
    public record Genero
    {
        public int Id { get; init; }
        public string Nombre { get; init; }
        public bool EstaActivo { get; init; } = true;
    }
}
```

**Reglas:**
- Usa `record` con propiedades `init` (inmutables).
- ID es `Guid` para entidades principales, `int` solo si es una tabla de catálogo simple.
- Sin constructores con parámetros obligatorios (incompatible con Dapper).
- Sin referencias a Dapper, Npgsql, DTOs ni nada de Infrastructure.

---

### Paso 2 — Interfaces en Application

Agrega las interfaces al archivo **existente** `src/AntaraSoft.Application/Contracts/Repository/IRepositories.cs`:

```csharp
public interface IGeneroRepository
{
    Task<List<Genero>> ObtenerTodos();
    Task<Genero> ObtenerPorId(int id);
    Task Crear(Genero genero);
    Task Eliminar(int id);
}
```

Y en `src/AntaraSoft.Application/Contracts/Services/IServices.cs`:

```csharp
public interface IGeneroService
{
    Task<List<Genero>> ObtenerTodos();
    Task<Genero> ObtenerPorId(int id);
    Task Crear(CrearGeneroDto dto);
    Task Eliminar(int id);
}
```

**Reglas:**
- Las interfaces **siempre** van en Application.
- No crear nuevos archivos `.cs` para interfaces — agrégalas al archivo existente.
- Los métodos del repositorio trabajan con **entidades de dominio**, no DTOs.
- Los métodos del servicio pueden recibir **DTOs** de entrada.

---

### Paso 3 — DTOs en Application

Agrega los DTOs al archivo **existente** `src/AntaraSoft.Application/Dtos/AllDtos.cs`:

```csharp
public class GeneroDto
{
    public int Id { get; set; }
    public string Nombre { get; set; }
}

public class CrearGeneroDto
{
    public string Nombre { get; set; }
}
```

**Reglas:**
- Todos los DTOs van en `AllDtos.cs`, no en archivos separados.
- Los DTOs de salida (`GeneroDto`) reflejan lo que el cliente recibe.
- Los DTOs de entrada (`CrearGeneroDto`) reflejan lo que el cliente envía.
- Nunca uses una entidad de dominio directamente como respuesta HTTP.

Agrega el método de mapeo en `MappingExtensions.cs`:

```csharp
public static GeneroDto AsDto(this Genero genero) => new()
{
    Id = genero.Id,
    Nombre = genero.Nombre
};
```

---

### Paso 4 — Servicio de aplicación en Application

Crea `src/AntaraSoft.Application/Services/GeneroService.cs`:

```csharp
using AntaraSoft.Application.Contracts.Repository;
using AntaraSoft.Application.Contracts.Services;
using AntaraSoft.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AntaraSoft.Application.Services
{
    public class GeneroService : IGeneroService
    {
        private readonly IGeneroRepository _generoRepository;

        public GeneroService(IGeneroRepository generoRepository)
        {
            _generoRepository = generoRepository;
        }

        public Task<List<Genero>> ObtenerTodos() =>
            _generoRepository.ObtenerTodos();

        public Task<Genero> ObtenerPorId(int id) =>
            _generoRepository.ObtenerPorId(id);

        public async Task Crear(CrearGeneroDto dto)
        {
            var genero = new Genero
            {
                Nombre = dto.Nombre,
                EstaActivo = true
            };
            await _generoRepository.Crear(genero);
        }

        public Task Eliminar(int id) =>
            _generoRepository.Eliminar(id);
    }
}
```

**Reglas:**
- El servicio **solo coordina**: valida, mapea, delega al repositorio.
- No contiene SQL ni código de red.
- Usa inyección de constructores.

---

### Paso 5 — Repositorio en Infrastructure

Crea `src/AntaraSoft.Infrastructure/Persistence/GeneroRepository.cs`:

```csharp
using AntaraSoft.Application.Contracts.Repository;
using AntaraSoft.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AntaraSoft.Infrastructure.Persistence
{
    public class GeneroRepository : IGeneroRepository
    {
        private readonly IDapper _dapper;

        public GeneroRepository(IDapper dapper)
        {
            _dapper = dapper;
        }

        public Task<List<Genero>> ObtenerTodos() =>
            _dapper.Consulta<Genero>("ObtenerTodosGeneros");

        public Task<Genero> ObtenerPorId(int id) =>
            _dapper.QueryWithReturn<Genero>("ObtenerGenero", new { Id = id });

        public Task Crear(Genero genero) =>
            _dapper.QueryWithReturn<Genero>("CrearGenero", new { genero.Nombre });

        public Task Eliminar(int id) =>
            _dapper.QueryWithReturn<Genero>("EliminarGenero", new { Id = id });
    }
}
```

**Reglas críticas de DapperWrapper:**
- `QueryWithReturn<T>` llama a un stored procedure y devuelve **un solo resultado** (`FirstOrDefault`).
- `Consulta<T>` llama a un stored procedure y devuelve **una lista**.
- El nombre del parámetro en `new { Id = id }` **debe coincidir exactamente** con el nombre del parámetro en el stored procedure de PostgreSQL (incluyendo mayúsculas).
- El stored procedure debe existir en la base de datos. Si no existe, el sistema falla en runtime.

---

### Paso 6 — Controlador en Api

Crea `src/AntaraSoft.Api/Controllers/GeneroController.cs`:

```csharp
using AntaraSoft.Api.Filters;
using AntaraSoft.Application;
using AntaraSoft.Application.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AntaraSoft.Api.Controllers
{
    [Route("api/genero")]
    [ApiController]
    public class GeneroController : Controller
    {
        private readonly IGeneroService _generoService;

        public GeneroController(IGeneroService generoService)
        {
            _generoService = generoService;
        }

        [HttpGet]
        [TokenAuthorize("admin", "administrador")]
        public async Task<IActionResult> ObtenerTodos()
        {
            try
            {
                var generos = (await _generoService.ObtenerTodos())
                    .Select(g => g.AsDto());
                return Ok(generos);
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudieron obtener los géneros." });
            }
        }

        [HttpPost]
        [TokenAuthorize("admin", "administrador")]
        public async Task<IActionResult> Crear([FromBody] CrearGeneroDto dto)
        {
            try
            {
                await _generoService.Crear(dto);
                return StatusCode(201);
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "No se pudo crear el género." });
            }
        }
    }
}
```

**Reglas:**
- Todos los controladores extienden `Controller` (no `ControllerBase`).
- Siempre envuelve en `try/catch` y devuelve `StatusCode(500, new { error = "..." })` en el error genérico.
- Usa `[TokenAuthorize("admin", "administrador")]` para endpoints privados.
- Devuelve siempre un DTO, nunca una entidad de dominio.

---

### Paso 7 — Registrar en Program.cs

En `src/AntaraSoft.Api/Program.cs`, agrega el registro de servicios en la sección de `AddTransient`:

```csharp
builder.Services.AddTransient<IGeneroRepository, GeneroRepository>();
builder.Services.AddTransient<IGeneroService, GeneroService>();
```

**Regla:** Repositorios y servicios de aplicación siempre se registran como `Transient`.

---

### Paso 8 — Stored Procedures en PostgreSQL

Los stored procedures se definen en `DatabaseInitializer.cs` (se ejecutan automáticamente al arrancar). Agrégalos al bloque de `CREATE OR REPLACE FUNCTION`:

```sql
CREATE OR REPLACE FUNCTION "ObtenerTodosGeneros"()
RETURNS SETOF "Genero" AS $$
    SELECT * FROM "Genero" WHERE "EstaActivo" = true ORDER BY "Nombre";
$$ LANGUAGE sql STABLE;

CREATE OR REPLACE FUNCTION "CrearGenero"("Nombre" VARCHAR)
RETURNS VOID AS $$
    INSERT INTO "Genero" ("Nombre", "EstaActivo")
    VALUES ("Nombre", true);
$$ LANGUAGE sql;
```

Y la tabla (si es nueva) en el bloque `CREATE TABLE IF NOT EXISTS`:

```sql
CREATE TABLE IF NOT EXISTS "Genero" (
    "Id"        SERIAL PRIMARY KEY,
    "Nombre"    VARCHAR(100) NOT NULL UNIQUE,
    "EstaActivo" BOOLEAN NOT NULL DEFAULT TRUE
);
```

**Reglas críticas:**
- Los nombres de funciones y columnas en PostgreSQL son **case-sensitive** entre comillas dobles.
- El nombre de la función **debe coincidir exactamente** con el string que pasas a `DapperWrapper`.
- Agrega `CREATE OR REPLACE FUNCTION` para que sea idempotente (re-ejecutar no falla).
- `CREATE TABLE IF NOT EXISTS` es siempre seguro de re-ejecutar.
- Después de modificar `DatabaseInitializer.cs`, reinicia el contenedor para que se apliquen los cambios.

---

## Tabla resumen de capas por artefacto

| Artefacto | Capa | Archivo |
|---|---|---|
| Entidad | Domain | `Entities/Genero.cs` |
| Interfaz repositorio | Application | `Contracts/Repository/IRepositories.cs` |
| Interfaz servicio | Application | `Contracts/Services/IServices.cs` |
| DTOs | Application | `Dtos/AllDtos.cs` |
| Extensión de mapeo | Application | `MappingExtensions.cs` |
| Servicio de aplicación | Application | `Services/GeneroService.cs` |
| Repositorio (Dapper) | Infrastructure | `Persistence/GeneroRepository.cs` |
| Controlador | Api | `Controllers/GeneroController.cs` |
| Registro DI | Api | `Program.cs` |
| SQL/stored procedures | Api | `DatabaseInitializer.cs` |

---

## ❌ Errores comunes al crear features

- **Poner SQL en el servicio de aplicación.** Solo en el repositorio.
- **Olvidar registrar en Program.cs.** El sistema falla con `InvalidOperationException` en runtime.
- **Usar mayúsculas distintas en el nombre del stored procedure.** `"CrearGenero"` en C# y `"CrearGenero"` en SQL deben ser idénticos.
- **Crear DTOs en archivos separados.** Todos los DTOs van en `AllDtos.cs`.
- **Olvidar el `CREATE TABLE IF NOT EXISTS`.** Si la tabla no existe, el stored procedure falla.
- **Devolver la entidad de dominio desde el controlador.** Siempre usa `.AsDto()`.
- **No agregar el `[TokenAuthorize]` a un endpoint privado.** Cualquiera podría llamarlo.

