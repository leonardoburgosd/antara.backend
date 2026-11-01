# Base de Datos

## Tablas del sistema

### `Usuario`

```sql
CREATE TABLE IF NOT EXISTS "Usuario" (
    "Id"              UUID PRIMARY KEY,
    "Email"           VARCHAR(255) UNIQUE NOT NULL,
    "Password"        VARCHAR(255) NOT NULL,          -- Hash BCrypt
    "Nombre"          VARCHAR(255) NOT NULL,
    "FechaNacimiento" TIMESTAMP NOT NULL,
    "Genero"          CHAR(1) NOT NULL,              -- 'M', 'F' u otro
    "EstaActivo"      BOOLEAN NOT NULL DEFAULT TRUE,
    "FechaRegistro"   TIMESTAMP NOT NULL,
    "Pais"            VARCHAR(100),
    "FotoPerfil"      TEXT,
    "Tipo"            VARCHAR(50)                    -- 'admin', 'administrador'
);
```

### `Album`

```sql
CREATE TABLE IF NOT EXISTS "Album" (
    "Id"                  UUID PRIMARY KEY,
    "Nombre"              VARCHAR(255) NOT NULL,
    "Descripcion"         TEXT,
    "EstaPublicado"       BOOLEAN NOT NULL DEFAULT FALSE,
    "FechaPublicacion"    TIMESTAMP,
    "PortadaUrl"          TEXT,
    "UsuarioId"           UUID NOT NULL REFERENCES "Usuario"("Id"),
    "EstaActivo"          BOOLEAN NOT NULL DEFAULT TRUE,
    "Artista"             VARCHAR(255),
    "FuenteMetadatos"     VARCHAR(50),
    "IdExternoMetadatos"  VARCHAR(100)
);
```

### `Genero`

```sql
CREATE TABLE IF NOT EXISTS "Genero" (
    "Id"     SERIAL PRIMARY KEY,     -- Entero autoincremental
    "Nombre" VARCHAR(100) NOT NULL UNIQUE
);
```

### `Pista`

```sql
CREATE TABLE IF NOT EXISTS "Pista" (
    "Id"                  UUID PRIMARY KEY,
    "Nombre"              VARCHAR(500) NOT NULL,
    "FechaRegistro"       TIMESTAMP NOT NULL,
    "AnoCreacion"         INTEGER,
    "Interprete"          VARCHAR(255),
    "Compositor"          VARCHAR(255),
    "Productor"           VARCHAR(255),
    "Reproducciones"      INTEGER NOT NULL DEFAULT 0,
    "GeneroId"            INTEGER REFERENCES "Genero"("Id"),
    "Url"                 TEXT NOT NULL,              -- Ruta física o URL
    "AlbumId"             UUID REFERENCES "Album"("Id"),
    "EstaActivo"          BOOLEAN NOT NULL DEFAULT TRUE,
    "Letra"               TEXT,
    "ImagenArtista"       TEXT,
    "DuracionSegundos"    INTEGER,
    "FuenteMetadatos"     VARCHAR(50),
    "IdExternoMetadatos"  VARCHAR(100),
    "PortadaUrl"          TEXT
);
```

### `Playlist`

```sql
CREATE TABLE IF NOT EXISTS "Playlist" (
    "Id"          UUID PRIMARY KEY,
    "Nombre"      VARCHAR(255) NOT NULL,
    "Descripcion" TEXT,
    "UsuarioId"   UUID NOT NULL REFERENCES "Usuario"("Id"),
    "PortadaUrl"  TEXT,
    "EstaActivo"  BOOLEAN NOT NULL DEFAULT TRUE
);
```

### `PlaylistPista`

```sql
CREATE TABLE IF NOT EXISTS "PlaylistPista" (
    "PlaylistId" UUID NOT NULL REFERENCES "Playlist"("Id"),
    "PistaId"    UUID NOT NULL REFERENCES "Pista"("Id"),
    PRIMARY KEY ("PlaylistId", "PistaId")
);
```

### `RutaAudio`

```sql
CREATE TABLE IF NOT EXISTS "RutaAudio" (
    "Id"                UUID PRIMARY KEY,
    "Nombre"            VARCHAR(255) NOT NULL UNIQUE,
    "RutaFisica"        TEXT,            -- Ruta en el host (ej: C:/Music)
    "RutaContenedor"    TEXT,            -- Ruta en el contenedor (ej: /music)
    "Descripcion"       TEXT,
    "EstaActiva"        BOOLEAN NOT NULL DEFAULT TRUE,
    "EsPredeterminada"  BOOLEAN NOT NULL DEFAULT FALSE,
    "FechaRegistro"     TIMESTAMP NOT NULL,
    "FechaActualizacion" TIMESTAMP
);
```

---

## Inicialización automática

Al arrancar la aplicación, `DatabaseInitializer.InitializeAsync()` ejecuta:

1. `CREATE TABLE IF NOT EXISTS` para todas las tablas.
2. `CREATE OR REPLACE FUNCTION` para todos los stored procedures.
3. `INSERT INTO "Genero" ... ON CONFLICT DO NOTHING` para los géneros semilla.

> [!IMPORTANT]
> Este proceso es **idempotente**: ejecutarlo múltiples veces no produce errores ni duplicados. Los `CREATE TABLE IF NOT EXISTS` y `CREATE OR REPLACE FUNCTION` son seguros de re-ejecutar.

---

## Stored Procedures (funciones PostgreSQL)

El sistema usa **funciones PostgreSQL** en vez de SQL embebido en el código. `DapperWrapper` las invoca con la sintaxis:

```sql
SELECT * FROM "NombreDeLaFuncion"(@Param1, @Param2)
```

### Ejemplos clave

```sql
-- Login: busca usuario por email para verificar contraseña
CREATE OR REPLACE FUNCTION "Login"("Email" VARCHAR)
RETURNS SETOF "Usuario" AS $$
    SELECT * FROM "Usuario" WHERE "Email" = "Email" AND "EstaActivo" = TRUE;
$$ LANGUAGE sql STABLE;

-- Crear pista
CREATE OR REPLACE FUNCTION "CrearPista"(
    "Id" UUID, "Nombre" VARCHAR, "Url" TEXT, ...
) RETURNS VOID AS $$
    INSERT INTO "Pista"(...) VALUES (...);
$$ LANGUAGE sql;

-- ObtenerPistas: devuelve la biblioteca con JOINs para la vista principal
CREATE OR REPLACE FUNCTION "ObtenerPistas"()
RETURNS TABLE(...) AS $$
    SELECT p.*, a."Nombre" AS "AlbumNombre", g."Nombre" AS "GeneroNombre", ...
    FROM "Pista" p
    LEFT JOIN "Album" a ON p."AlbumId" = a."Id"
    LEFT JOIN "Genero" g ON p."GeneroId" = g."Id"
    WHERE p."EstaActivo" = TRUE
    ORDER BY p."FechaRegistro" DESC;
$$ LANGUAGE sql STABLE;
```

---

## Cómo agregar una nueva migración

1. **Agrega la tabla** en el bloque `CREATE TABLE IF NOT EXISTS` dentro de `DatabaseInitializer.cs`.
2. **Agrega las funciones** en el bloque `CREATE OR REPLACE FUNCTION`.
3. **Reinicia el contenedor** para que `DatabaseInitializer` ejecute los nuevos scripts.

```bash
docker compose restart api
```

> [!WARNING]
> `DatabaseInitializer` no elimina columnas ni tablas. Si necesitas eliminar o renombrar algo, hazlo manualmente con `psql` o un cliente PostgreSQL. El `IF NOT EXISTS` solo protege contra crear lo que ya existe, no contra modificar.

---

## Acceso directo a PostgreSQL

```bash
# Desde el host (requiere cliente psql)
psql -h 127.0.0.1 -p 5432 -U postgres -d antaradb

# Desde dentro del contenedor
docker exec -it antara-postgres psql -U postgres -d antaradb
```

---

## ❌ Errores comunes

- **Parámetro con mayúsculas distintas.** `new { id = guid }` vs `"id" VARCHAR` en PostgreSQL. Los nombres de parámetros deben coincidir exactamente, incluyendo mayúsculas.
- **Función sin `CREATE OR REPLACE`.** Si usas solo `CREATE FUNCTION`, fallará en la segunda ejecución del inicializador.
- **Olvidar agregar la tabla al inicializador.** El stored procedure falla con "relación no encontrada".
- **Usar `SERIAL` para IDs de entidades principales.** Usa `UUID`. Solo `Genero.Id` usa `SERIAL`.
- **Confundir `SETOF "Tipo"` con `RETURNS TABLE`.** Ambos funcionan, pero `SETOF` requiere que la tabla exista con exactamente las mismas columnas en el mismo orden.

