using AntaraSoft.Application;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Threading.Tasks;

namespace AntaraSoft.Api
{
    public class DatabaseInitializer
    {
        private readonly string _connectionString;
        private readonly AppSettings _settings;

        public DatabaseInitializer(IOptions<AppSettings> options)
        {
            _settings = options.Value;
            _connectionString = options.Value.ConexionString;
        }

        public async Task InitializeAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // Create tables
                var createTablesSql = @"
                    CREATE TABLE IF NOT EXISTS ""Usuario"" (
                        ""Id"" UUID PRIMARY KEY,
                        ""Email"" VARCHAR(255) UNIQUE NOT NULL,
                        ""Password"" VARCHAR(255) NOT NULL,
                        ""Nombre"" VARCHAR(255) NOT NULL,
                        ""FechaNacimiento"" TIMESTAMP NOT NULL,
                        ""Genero"" CHAR(1) NOT NULL,
                        ""EstaActivo"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""FechaRegistro"" TIMESTAMP NOT NULL,
                        ""Pais"" VARCHAR(100),
                        ""FotoPerfil"" TEXT,
                        ""Tipo"" VARCHAR(50)
                    );

                    CREATE TABLE IF NOT EXISTS ""Album"" (
                        ""Id"" UUID PRIMARY KEY,
                        ""Nombre"" VARCHAR(255) NOT NULL,
                        ""Descripcion"" TEXT,
                        ""EstaPublicado"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""FechaPublicacion"" TIMESTAMP,
                        ""PortadaUrl"" TEXT,
                        ""UsuarioId"" UUID NOT NULL REFERENCES ""Usuario""(""Id""),
                        ""EstaActivo"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""Artista"" VARCHAR(255),
                        ""FuenteMetadatos"" VARCHAR(50),
                        ""IdExternoMetadatos"" VARCHAR(100)
                    );

                    CREATE TABLE IF NOT EXISTS ""Playlist"" (
                        ""Id"" UUID PRIMARY KEY,
                        ""Nombre"" VARCHAR(255) NOT NULL,
                        ""Descripcion"" TEXT,
                        ""PortadaUrl"" TEXT,
                        ""UsuarioId"" UUID NOT NULL REFERENCES ""Usuario""(""Id""),
                        ""EstaActivo"" BOOLEAN NOT NULL DEFAULT TRUE
                    );

                    CREATE TABLE IF NOT EXISTS ""Pista"" (
                        ""Id"" UUID PRIMARY KEY,
                        ""Nombre"" VARCHAR(255) NOT NULL,
                        ""FechaRegistro"" TIMESTAMP NOT NULL,
                        ""AnoCreacion"" INTEGER NOT NULL,
                        ""Interprete"" VARCHAR(255),
                        ""Compositor"" VARCHAR(255),
                        ""Productor"" VARCHAR(255),
                        ""Reproducciones"" INTEGER NOT NULL DEFAULT 0,
                        ""GeneroId"" INTEGER NOT NULL,
                        ""Url"" TEXT,
                        ""AlbumId"" UUID NOT NULL REFERENCES ""Album""(""Id""),
                        ""UsuarioId"" UUID REFERENCES ""Usuario""(""Id""),
                        ""EstaActivo"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""Letra"" TEXT,
                        ""ImagenArtista"" TEXT,
                        ""DuracionSegundos"" INTEGER NOT NULL DEFAULT 0,
                        ""FuenteMetadatos"" VARCHAR(50),
                        ""IdExternoMetadatos"" VARCHAR(100),
                        ""MetadatosJson"" TEXT
                    );

                    CREATE TABLE IF NOT EXISTS ""PlaylistPista"" (
                        ""PlaylistId"" UUID NOT NULL REFERENCES ""Playlist""(""Id""),
                        ""PistaId"" UUID NOT NULL REFERENCES ""Pista""(""Id""),
                        ""FechaRegistro"" TIMESTAMP NOT NULL,
                        PRIMARY KEY (""PlaylistId"", ""PistaId"")
                    );

                    CREATE TABLE IF NOT EXISTS ""RutaAudio"" (
                        ""Id"" UUID PRIMARY KEY,
                        ""Nombre"" VARCHAR(100) NOT NULL,
                        ""RutaFisica"" TEXT NOT NULL DEFAULT '',
                        ""RutaContenedor"" TEXT,
                        ""Descripcion"" TEXT,
                        ""EstaActiva"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""EsPredeterminada"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""FechaRegistro"" TIMESTAMP NOT NULL,
                        ""FechaActualizacion"" TIMESTAMP
                    );

                    CREATE UNIQUE INDEX IF NOT EXISTS ""IX_RutaAudio_Nombre""
                        ON ""RutaAudio"" (LOWER(""Nombre""));

                    CREATE TABLE IF NOT EXISTS ""Genero"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""Nombre"" VARCHAR(100) NOT NULL
                    );

                    CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Genero_Nombre""
                        ON ""Genero"" (LOWER(""Nombre""));
                ";

                using var cmd = new NpgsqlCommand(createTablesSql, connection);
                await cmd.ExecuteNonQueryAsync();

                // Columnas agregadas para el escaneo de biblioteca (bases ya existentes).
                var migrarColumnasSql = @"
                    ALTER TABLE ""Album"" ADD COLUMN IF NOT EXISTS ""Artista"" VARCHAR(255);
                    ALTER TABLE ""Album"" ADD COLUMN IF NOT EXISTS ""FuenteMetadatos"" VARCHAR(50);
                    ALTER TABLE ""Album"" ADD COLUMN IF NOT EXISTS ""IdExternoMetadatos"" VARCHAR(100);

                    ALTER TABLE ""Pista"" ADD COLUMN IF NOT EXISTS ""Letra"" TEXT;
                    ALTER TABLE ""Pista"" ADD COLUMN IF NOT EXISTS ""ImagenArtista"" TEXT;
                    ALTER TABLE ""Pista"" ADD COLUMN IF NOT EXISTS ""DuracionSegundos"" INTEGER NOT NULL DEFAULT 0;
                    ALTER TABLE ""Pista"" ADD COLUMN IF NOT EXISTS ""FuenteMetadatos"" VARCHAR(50);
                    ALTER TABLE ""Pista"" ADD COLUMN IF NOT EXISTS ""IdExternoMetadatos"" VARCHAR(100);
                    ALTER TABLE ""Pista"" ADD COLUMN IF NOT EXISTS ""MetadatosJson"" TEXT;
                    ALTER TABLE ""Pista"" ADD COLUMN IF NOT EXISTS ""UsuarioId"" UUID REFERENCES ""Usuario""(""Id"");
                    CREATE INDEX IF NOT EXISTS ""IX_Pista_UsuarioId"" ON ""Pista"" (""UsuarioId"");

                    CREATE TABLE IF NOT EXISTS ""Genero"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""Nombre"" VARCHAR(100) NOT NULL
                    );

                    CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Genero_Nombre""
                        ON ""Genero"" (LOWER(""Nombre""));
                    CREATE INDEX IF NOT EXISTS ""IX_Pista_Url"" ON ""Pista"" (""Url"");
                    CREATE INDEX IF NOT EXISTS ""IX_Pista_AlbumId"" ON ""Pista"" (""AlbumId"");
                    CREATE INDEX IF NOT EXISTS ""IX_Album_NombreArtista""
                        ON ""Album"" (LOWER(""Nombre""), LOWER(""Artista""));
                    CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Album_IdExterno""
                        ON ""Album"" (""IdExternoMetadatos"")
                        WHERE ""IdExternoMetadatos"" IS NOT NULL;
                ";

                using var migrarCmd = new NpgsqlCommand(migrarColumnasSql, connection);
                await migrarCmd.ExecuteNonQueryAsync();

                // Registrar las rutas de audio definidas en appsettings.json la primera vez
                await SembrarRutasAudioAsync(connection);

                // Create functions (stored procedures equivalent)
                var createFunctionsSql = @"
                    -- Eliminar funciones previas para permitir cambios en el tipo de retorno.
                    -- CREATE OR REPLACE no puede modificar el tipo de retorno de una funcion.
                    DO $$
                    DECLARE
                        r RECORD;
                    BEGIN
                        FOR r IN
                            SELECT oid::regprocedure AS signature
                            FROM pg_proc
                            WHERE proname IN ('VerificarEmailUnico', 'CrearUsuario', 'ActualizarUsuario', 'ObtenerUsuario', 'Login', 'ObtenerTodosUsuarios',
                                              'ObtenerAlbum', 'ObtenerTodosAlbumes', 'ObtenerTodosAlbumesDeUsuarios', 'EditarAlbum')
                        LOOP
                            EXECUTE 'DROP FUNCTION ' || r.signature;
                        END LOOP;
                    END $$;

                    -- VerificarEmailUnico
                    CREATE OR REPLACE FUNCTION ""VerificarEmailUnico""(p_Email VARCHAR)
                    RETURNS TABLE(""Id"" UUID, ""Email"" VARCHAR, ""Password"" VARCHAR, ""Nombre"" VARCHAR, ""FechaNacimiento"" TIMESTAMP, ""Genero"" CHAR, ""EstaActivo"" BOOLEAN, ""FechaRegistro"" TIMESTAMP, ""Pais"" VARCHAR, ""FotoPerfil"" TEXT, ""Tipo"" VARCHAR)
                    AS $$
                    BEGIN
                        RETURN QUERY SELECT u.""Id"", u.""Email"", u.""Password"", u.""Nombre"", u.""FechaNacimiento"", u.""Genero"", u.""EstaActivo"", u.""FechaRegistro"", u.""Pais"", u.""FotoPerfil"", u.""Tipo""
                        FROM ""Usuario"" u WHERE u.""Email"" = p_Email;
                    END;
                    $$ LANGUAGE plpgsql;

                    -- CrearUsuario
                    CREATE OR REPLACE FUNCTION ""CrearUsuario""(p_Id UUID, p_Email VARCHAR, p_Password VARCHAR, p_Nombre VARCHAR, p_FechaNacimiento TIMESTAMP WITH TIME ZONE, p_Genero CHAR, p_EstaActivo BOOLEAN, p_FechaRegistro TIMESTAMP, p_Pais VARCHAR, p_FotoPerfil TEXT, p_Tipo VARCHAR)
                    RETURNS TABLE(""Id"" UUID)
                    AS $$
                    BEGIN
                        INSERT INTO ""Usuario""(""Id"", ""Email"", ""Password"", ""Nombre"", ""FechaNacimiento"", ""Genero"", ""EstaActivo"", ""FechaRegistro"", ""Pais"", ""FotoPerfil"", ""Tipo"")
                        VALUES (p_Id, p_Email, p_Password, p_Nombre, p_FechaNacimiento, p_Genero, p_EstaActivo, p_FechaRegistro, p_Pais, p_FotoPerfil, p_Tipo);
                        RETURN QUERY SELECT p_Id;
                    END;
                    $$ LANGUAGE plpgsql;

                    -- ActualizarUsuario (p_Password NULL conserva la contraseña actual)
                    CREATE OR REPLACE FUNCTION ""ActualizarUsuario""(p_Id UUID, p_Email VARCHAR, p_Nombre VARCHAR, p_FechaNacimiento TIMESTAMP, p_Genero CHAR, p_EstaActivo BOOLEAN, p_Pais VARCHAR, p_FotoPerfil TEXT, p_Tipo VARCHAR, p_Password VARCHAR)
                    RETURNS TABLE(""Id"" UUID, ""Email"" VARCHAR, ""Password"" VARCHAR, ""Nombre"" VARCHAR, ""FechaNacimiento"" TIMESTAMP, ""Genero"" CHAR, ""EstaActivo"" BOOLEAN, ""FechaRegistro"" TIMESTAMP, ""Pais"" VARCHAR, ""FotoPerfil"" TEXT, ""Tipo"" VARCHAR)
                    AS $$
                    BEGIN
                        UPDATE ""Usuario"" SET
                            ""Email"" = p_Email,
                            ""Nombre"" = p_Nombre,
                            ""FechaNacimiento"" = p_FechaNacimiento,
                            ""Genero"" = p_Genero,
                            ""EstaActivo"" = p_EstaActivo,
                            ""Pais"" = p_Pais,
                            ""FotoPerfil"" = p_FotoPerfil,
                            ""Tipo"" = COALESCE(p_Tipo, ""Usuario"".""Tipo""),
                            ""Password"" = COALESCE(p_Password, ""Usuario"".""Password"")
                        WHERE ""Usuario"".""Id"" = p_Id;

                        RETURN QUERY SELECT u.""Id"", u.""Email"", u.""Password"", u.""Nombre"", u.""FechaNacimiento"", u.""Genero"", u.""EstaActivo"", u.""FechaRegistro"", u.""Pais"", u.""FotoPerfil"", u.""Tipo""
                        FROM ""Usuario"" u WHERE u.""Id"" = p_Id;
                    END;
                    $$ LANGUAGE plpgsql;

                    -- ObtenerUsuario
                    CREATE OR REPLACE FUNCTION ""ObtenerUsuario""(p_Id UUID)
                    RETURNS TABLE(""Id"" UUID, ""Email"" VARCHAR, ""Password"" VARCHAR, ""Nombre"" VARCHAR, ""FechaNacimiento"" TIMESTAMP, ""Genero"" CHAR, ""EstaActivo"" BOOLEAN, ""FechaRegistro"" TIMESTAMP, ""Pais"" VARCHAR, ""FotoPerfil"" TEXT, ""Tipo"" VARCHAR)
                    AS $$
                    BEGIN
                        RETURN QUERY SELECT u.""Id"", u.""Email"", u.""Password"", u.""Nombre"", u.""FechaNacimiento"", u.""Genero"", u.""EstaActivo"", u.""FechaRegistro"", u.""Pais"", u.""FotoPerfil"", u.""Tipo""
                        FROM ""Usuario"" u WHERE u.""Id"" = p_Id;
                    END;
                    $$ LANGUAGE plpgsql;

                    -- Login (busca por email; la contraseña se valida en LoginService)
                    CREATE OR REPLACE FUNCTION ""Login""(p_Email VARCHAR)
                    RETURNS TABLE(""Id"" UUID, ""Email"" VARCHAR, ""Password"" VARCHAR, ""Nombre"" VARCHAR, ""FechaNacimiento"" TIMESTAMP, ""Genero"" CHAR, ""EstaActivo"" BOOLEAN, ""FechaRegistro"" TIMESTAMP, ""Pais"" VARCHAR, ""FotoPerfil"" TEXT, ""Tipo"" VARCHAR)
                    AS $$
                    BEGIN
                        RETURN QUERY SELECT u.""Id"", u.""Email"", u.""Password"", u.""Nombre"", u.""FechaNacimiento"", u.""Genero"", u.""EstaActivo"", u.""FechaRegistro"", u.""Pais"", u.""FotoPerfil"", u.""Tipo""
                        FROM ""Usuario"" u WHERE u.""Email"" = p_Email;
                    END;
                    $$ LANGUAGE plpgsql;

                    -- ObtenerTodosUsuarios
                    CREATE OR REPLACE FUNCTION ""ObtenerTodosUsuarios""()
                    RETURNS TABLE(""Id"" UUID, ""Email"" VARCHAR, ""Password"" VARCHAR, ""Nombre"" VARCHAR, ""FechaNacimiento"" TIMESTAMP, ""Genero"" CHAR, ""EstaActivo"" BOOLEAN, ""FechaRegistro"" TIMESTAMP, ""Pais"" VARCHAR, ""FotoPerfil"" TEXT, ""Tipo"" VARCHAR)
                    AS $$
                    BEGIN
                        RETURN QUERY SELECT u.""Id"", u.""Email"", u.""Password"", u.""Nombre"", u.""FechaNacimiento"", u.""Genero"", u.""EstaActivo"", u.""FechaRegistro"", u.""Pais"", u.""FotoPerfil"", u.""Tipo""
                        FROM ""Usuario"" u;
                    END;
                    $$ LANGUAGE plpgsql;

                    -- EliminarFisicoUsuario
                    CREATE OR REPLACE FUNCTION ""EliminarFisicoUsuario""(p_Id UUID)
                    RETURNS TABLE(""Id"" UUID)
                    AS $$
                    BEGIN
                        DELETE FROM ""Usuario"" WHERE ""Usuario"".""Id"" = p_Id;
                        RETURN QUERY SELECT p_Id;
                    END;
                    $$ LANGUAGE plpgsql;
                ";

                using var cmd2 = new NpgsqlCommand(createFunctionsSql, connection);
                await cmd2.ExecuteNonQueryAsync();

                // Add other functions for Album, Pista, Playlist
                var createMoreFunctionsSql = @"
                    -- CrearAlbum
                    CREATE OR REPLACE FUNCTION ""CrearAlbum""(p_Id UUID, p_Nombre VARCHAR, p_Descripcion TEXT, p_EstaPublicado BOOLEAN, p_FechaPublicacion TIMESTAMP, p_PortadaUrl TEXT, p_UsuarioId UUID, p_EstaActivo BOOLEAN)
                    RETURNS TABLE(""Id"" UUID)
                    AS $$
                    BEGIN
                        INSERT INTO ""Album""(""Id"", ""Nombre"", ""Descripcion"", ""EstaPublicado"", ""FechaPublicacion"", ""PortadaUrl"", ""UsuarioId"", ""EstaActivo"")
                        VALUES (p_Id, p_Nombre, p_Descripcion, p_EstaPublicado, p_FechaPublicacion, p_PortadaUrl, p_UsuarioId, p_EstaActivo);
                        RETURN QUERY SELECT p_Id;
                    END;
                    $$ LANGUAGE plpgsql;

                    -- EliminarAlbum
                    CREATE OR REPLACE FUNCTION ""EliminarAlbum""(p_Id UUID)
                    RETURNS TABLE(""Id"" UUID)
                    AS $$
                    BEGIN
                        DELETE FROM ""Album"" WHERE ""Album"".""Id"" = p_Id;
                        RETURN QUERY SELECT p_Id;
                    END;
                    $$ LANGUAGE plpgsql;

                    -- EditarAlbum (incluye publicación y estado para el CRUD del panel)
                    CREATE OR REPLACE FUNCTION ""EditarAlbum""(p_Id UUID, p_Nombre VARCHAR, p_Descripcion TEXT, p_PortadaUrl TEXT, p_EstaPublicado BOOLEAN, p_EstaActivo BOOLEAN, p_Artista VARCHAR)
                    RETURNS TABLE(""Id"" UUID)
                    AS $$
                    BEGIN
                        UPDATE ""Album"" SET
                            ""Nombre"" = p_Nombre,
                            ""Descripcion"" = p_Descripcion,
                            ""PortadaUrl"" = p_PortadaUrl,
                            ""EstaPublicado"" = p_EstaPublicado,
                            ""EstaActivo"" = p_EstaActivo,
                            ""Artista"" = p_Artista,
                            ""FechaPublicacion"" = CASE WHEN p_EstaPublicado AND ""Album"".""FechaPublicacion"" IS NULL
                                THEN NOW() ELSE ""Album"".""FechaPublicacion"" END
                        WHERE ""Album"".""Id"" = p_Id;
                        RETURN QUERY SELECT p_Id;
                    END;
                    $$ LANGUAGE plpgsql;

                    -- ObtenerAlbum
                    CREATE OR REPLACE FUNCTION ""ObtenerAlbum""(p_Id UUID)
                    RETURNS TABLE(""Id"" UUID, ""Nombre"" VARCHAR, ""Descripcion"" TEXT, ""EstaPublicado"" BOOLEAN, ""FechaPublicacion"" TIMESTAMP, ""PortadaUrl"" TEXT, ""UsuarioId"" UUID, ""EstaActivo"" BOOLEAN, ""Artista"" VARCHAR, ""FuenteMetadatos"" VARCHAR, ""IdExternoMetadatos"" VARCHAR)
                    AS $$
                    BEGIN
                        RETURN QUERY SELECT a.""Id"", a.""Nombre"", a.""Descripcion"", a.""EstaPublicado"", a.""FechaPublicacion"", a.""PortadaUrl"", a.""UsuarioId"", a.""EstaActivo"", a.""Artista"", a.""FuenteMetadatos"", a.""IdExternoMetadatos""
                        FROM ""Album"" a WHERE a.""Id"" = p_Id;
                    END;
                    $$ LANGUAGE plpgsql;

                    -- ObtenerTodosAlbumes
                    CREATE OR REPLACE FUNCTION ""ObtenerTodosAlbumes""()
                    RETURNS TABLE(""Id"" UUID, ""Nombre"" VARCHAR, ""Descripcion"" TEXT, ""EstaPublicado"" BOOLEAN, ""FechaPublicacion"" TIMESTAMP, ""PortadaUrl"" TEXT, ""UsuarioId"" UUID, ""EstaActivo"" BOOLEAN, ""Artista"" VARCHAR, ""FuenteMetadatos"" VARCHAR, ""IdExternoMetadatos"" VARCHAR)
                    AS $$
                    BEGIN
                        RETURN QUERY SELECT a.""Id"", a.""Nombre"", a.""Descripcion"", a.""EstaPublicado"", a.""FechaPublicacion"", a.""PortadaUrl"", a.""UsuarioId"", a.""EstaActivo"", a.""Artista"", a.""FuenteMetadatos"", a.""IdExternoMetadatos""
                        FROM ""Album"" a ORDER BY a.""Nombre"";
                    END;
                    $$ LANGUAGE plpgsql;

                    -- ObtenerTodosAlbumesDeUsuarios
                    CREATE OR REPLACE FUNCTION ""ObtenerTodosAlbumesDeUsuarios""(p_UsuarioId UUID)
                    RETURNS TABLE(""Id"" UUID, ""Nombre"" VARCHAR, ""Descripcion"" TEXT, ""EstaPublicado"" BOOLEAN, ""FechaPublicacion"" TIMESTAMP, ""PortadaUrl"" TEXT, ""UsuarioId"" UUID, ""EstaActivo"" BOOLEAN, ""Artista"" VARCHAR, ""FuenteMetadatos"" VARCHAR, ""IdExternoMetadatos"" VARCHAR)
                    AS $$
                    BEGIN
                        RETURN QUERY SELECT a.""Id"", a.""Nombre"", a.""Descripcion"", a.""EstaPublicado"", a.""FechaPublicacion"", a.""PortadaUrl"", a.""UsuarioId"", a.""EstaActivo"", a.""Artista"", a.""FuenteMetadatos"", a.""IdExternoMetadatos""
                        FROM ""Album"" a WHERE a.""UsuarioId"" = p_UsuarioId;
                    END;
                    $$ LANGUAGE plpgsql;

                    -- PublicarAlbum
                    CREATE OR REPLACE FUNCTION ""PublicarAlbum""(p_Id UUID, p_EstaPublicado BOOLEAN)
                    RETURNS INTEGER
                    AS $$
                    BEGIN
                        UPDATE ""Album"" SET
                            ""EstaPublicado"" = p_EstaPublicado,
                            ""FechaPublicacion"" = CASE WHEN p_EstaPublicado THEN NOW() ELSE ""Album"".""FechaPublicacion"" END
                        WHERE ""Album"".""Id"" = p_Id;
                        RETURN 1;
                    END;
                    $$ LANGUAGE plpgsql;
                ";

                using var cmd3 = new NpgsqlCommand(createMoreFunctionsSql, connection);
                await cmd3.ExecuteNonQueryAsync();

                // Add functions for Playlist and Pista
                var createMoreFunctionsSql2 = @"
                    -- CrearPlaylist
                    CREATE OR REPLACE FUNCTION ""CrearPlaylist""(p_Id UUID, p_Nombre VARCHAR, p_Descripcion TEXT, p_PortadaUrl TEXT, p_UsuarioId UUID, p_EstaActivo BOOLEAN)
                    RETURNS TABLE(""Id"" UUID)
                    AS $$
                    BEGIN
                        INSERT INTO ""Playlist""(""Id"", ""Nombre"", ""Descripcion"", ""PortadaUrl"", ""UsuarioId"", ""EstaActivo"")
                        VALUES (p_Id, p_Nombre, p_Descripcion, p_PortadaUrl, p_UsuarioId, p_EstaActivo);
                        RETURN QUERY SELECT p_Id;
                    END;
                    $$ LANGUAGE plpgsql;

                    -- EditarPlaylist
                    CREATE OR REPLACE FUNCTION ""EditarPlaylist""(p_Id UUID, p_Nombre VARCHAR, p_Descripcion TEXT, p_PortadaUrl TEXT, p_UsuarioId UUID, p_EstaActivo BOOLEAN)
                    RETURNS TABLE(""Id"" UUID)
                    AS $$
                    BEGIN
                        UPDATE ""Playlist"" SET ""Nombre"" = p_Nombre, ""Descripcion"" = p_Descripcion, ""PortadaUrl"" = p_PortadaUrl, ""UsuarioId"" = p_UsuarioId, ""EstaActivo"" = p_EstaActivo
                        WHERE ""Playlist"".""Id"" = p_Id;
                        RETURN QUERY SELECT p_Id;
                    END;
                    $$ LANGUAGE plpgsql;

                    -- EliminarPlaylist
                    CREATE OR REPLACE FUNCTION ""EliminarPlaylist""(p_Id UUID)
                    RETURNS TABLE(""Id"" UUID)
                    AS $$
                    BEGIN
                        -- Las canciones de la playlist no se borran: solo se quita la
                        -- relación. El orden evita depender del ON DELETE de la FK.
                        DELETE FROM ""PlaylistPista"" WHERE ""PlaylistPista"".""PlaylistId"" = p_Id;
                        DELETE FROM ""Playlist"" WHERE ""Playlist"".""Id"" = p_Id;
                        RETURN QUERY SELECT p_Id;
                    END;
                    $$ LANGUAGE plpgsql;

                    -- ObtenerPlaylist
                    CREATE OR REPLACE FUNCTION ""ObtenerPlaylist""(p_Id UUID)
                    RETURNS TABLE(""Id"" UUID, ""Nombre"" VARCHAR, ""Descripcion"" TEXT, ""PortadaUrl"" TEXT, ""UsuarioId"" UUID, ""EstaActivo"" BOOLEAN)
                    AS $$
                    BEGIN
                        RETURN QUERY SELECT pl.""Id"", pl.""Nombre"", pl.""Descripcion"", pl.""PortadaUrl"", pl.""UsuarioId"", pl.""EstaActivo""
                        FROM ""Playlist"" pl WHERE pl.""Id"" = p_Id;
                    END;
                    $$ LANGUAGE plpgsql;

                    -- ObtenerTodosPlaylistsDeUsuarios
                    CREATE OR REPLACE FUNCTION ""ObtenerTodosPlaylistsDeUsuarios""(p_UsuarioId UUID)
                    RETURNS TABLE(""Id"" UUID, ""Nombre"" VARCHAR, ""Descripcion"" TEXT, ""PortadaUrl"" TEXT, ""UsuarioId"" UUID, ""EstaActivo"" BOOLEAN)
                    AS $$
                    BEGIN
                        RETURN QUERY SELECT pl.""Id"", pl.""Nombre"", pl.""Descripcion"", pl.""PortadaUrl"", pl.""UsuarioId"", pl.""EstaActivo""
                        FROM ""Playlist"" pl WHERE pl.""UsuarioId"" = p_UsuarioId;
                    END;
                    $$ LANGUAGE plpgsql;
                ";

                using var cmd4 = new NpgsqlCommand(createMoreFunctionsSql2, connection);
                await cmd4.ExecuteNonQueryAsync();

                // Pista functions
                var createPistaFunctionsSql = @"
                    -- CrearPista
                    CREATE OR REPLACE FUNCTION ""CrearPista""(p_Id UUID, p_Nombre VARCHAR, p_FechaRegistro TIMESTAMP, p_AnoCreacion INTEGER, p_Interprete VARCHAR, p_Compositor VARCHAR, p_Productor VARCHAR, p_Reproducciones INTEGER, p_GeneroId INTEGER, p_Url TEXT, p_AlbumId UUID, p_EstaActivo BOOLEAN)
                    RETURNS TABLE(""Id"" UUID)
                    AS $$
                    BEGIN
                        INSERT INTO ""Pista""(""Id"", ""Nombre"", ""FechaRegistro"", ""AnoCreacion"", ""Interprete"", ""Compositor"", ""Productor"", ""Reproducciones"", ""GeneroId"", ""Url"", ""AlbumId"", ""EstaActivo"")
                        VALUES (p_Id, p_Nombre, p_FechaRegistro, p_AnoCreacion, p_Interprete, p_Compositor, p_Productor, p_Reproducciones, p_GeneroId, p_Url, p_AlbumId, p_EstaActivo);
                        RETURN QUERY SELECT p_Id;
                    END;
                    $$ LANGUAGE plpgsql;

                    -- EliminarPista
                    CREATE OR REPLACE FUNCTION ""EliminarPista""(p_Id UUID)
                    RETURNS TABLE(""Id"" UUID)
                    AS $$
                    BEGIN
                        DELETE FROM ""Pista"" WHERE ""Id"" = p_Id;
                        RETURN QUERY SELECT p_Id;
                    END;
                    $$ LANGUAGE plpgsql;

                    -- EditarPista
                    CREATE OR REPLACE FUNCTION ""EditarPista""(p_Id UUID, p_Nombre VARCHAR, p_FechaRegistro TIMESTAMP, p_AnoCreacion INTEGER, p_Interprete VARCHAR, p_Compositor VARCHAR, p_Productor VARCHAR, p_Reproducciones INTEGER, p_GeneroId INTEGER, p_Url TEXT, p_AlbumId UUID, p_EstaActivo BOOLEAN)
                    RETURNS TABLE(""Id"" UUID)
                    AS $$
                    BEGIN
                        UPDATE ""Pista"" SET ""Nombre"" = p_Nombre, ""FechaRegistro"" = p_FechaRegistro, ""AnoCreacion"" = p_AnoCreacion, 
                        ""Interprete"" = p_Interprete, ""Compositor"" = p_Compositor, ""Productor"" = p_Productor, ""Reproducciones"" = p_Reproducciones,
                        ""GeneroId"" = p_GeneroId, ""Url"" = p_Url, ""AlbumId"" = p_AlbumId, ""EstaActivo"" = p_EstaActivo
                        WHERE ""Id"" = p_Id;
                        RETURN QUERY SELECT p_Id;
                    END;
                    $$ LANGUAGE plpgsql;

                    -- ObtenerPista
                    DROP FUNCTION IF EXISTS ""ObtenerPista""(UUID);
                    CREATE OR REPLACE FUNCTION ""ObtenerPista""(p_Id UUID)
                    RETURNS TABLE(""Id"" UUID, ""Nombre"" VARCHAR, ""FechaRegistro"" TIMESTAMP, ""AnoCreacion"" INTEGER, ""Interprete"" VARCHAR, ""Compositor"" VARCHAR, ""Productor"" VARCHAR, ""Reproducciones"" INTEGER, ""GeneroId"" INTEGER, ""Url"" TEXT, ""AlbumId"" UUID, ""EstaActivo"" BOOLEAN, ""DuracionSegundos"" INTEGER, ""UsuarioId"" UUID)
                    AS $$
                    BEGIN
                        RETURN QUERY SELECT pi.""Id"", pi.""Nombre"", pi.""FechaRegistro"", pi.""AnoCreacion"", pi.""Interprete"", pi.""Compositor"", pi.""Productor"", pi.""Reproducciones"", pi.""GeneroId"", pi.""Url"", pi.""AlbumId"", pi.""EstaActivo"", pi.""DuracionSegundos"", pi.""UsuarioId""
                        FROM ""Pista"" pi WHERE pi.""Id"" = p_Id;
                    END;
                    $$ LANGUAGE plpgsql;

                    -- ObtenerTodosPistasDeAlbum
                    DROP FUNCTION IF EXISTS ""ObtenerTodosPistasDeAlbum""(UUID);
                    CREATE OR REPLACE FUNCTION ""ObtenerTodosPistasDeAlbum""(p_AlbumId UUID)
                    RETURNS TABLE(""Id"" UUID, ""Nombre"" VARCHAR, ""FechaRegistro"" TIMESTAMP, ""AnoCreacion"" INTEGER, ""Interprete"" VARCHAR, ""Compositor"" VARCHAR, ""Productor"" VARCHAR, ""Reproducciones"" INTEGER, ""GeneroId"" INTEGER, ""Url"" TEXT, ""AlbumId"" UUID, ""EstaActivo"" BOOLEAN, ""DuracionSegundos"" INTEGER, ""UsuarioId"" UUID)
                    AS $$
                    BEGIN
                        RETURN QUERY SELECT pi.""Id"", pi.""Nombre"", pi.""FechaRegistro"", pi.""AnoCreacion"", pi.""Interprete"", pi.""Compositor"", pi.""Productor"", pi.""Reproducciones"", pi.""GeneroId"", pi.""Url"", pi.""AlbumId"", pi.""EstaActivo"", pi.""DuracionSegundos"", pi.""UsuarioId""
                        FROM ""Pista"" pi WHERE pi.""AlbumId"" = p_AlbumId;
                    END;
                    $$ LANGUAGE plpgsql;

                    -- ObtenerTodosPistasDePlaylist
                    DROP FUNCTION IF EXISTS ""ObtenerTodosPistasDePlaylist""(UUID);
                    CREATE OR REPLACE FUNCTION ""ObtenerTodosPistasDePlaylist""(p_PlaylistId UUID)
                    RETURNS TABLE(""Id"" UUID, ""Nombre"" VARCHAR, ""FechaRegistro"" TIMESTAMP, ""AnoCreacion"" INTEGER, ""Interprete"" VARCHAR, ""Compositor"" VARCHAR, ""Productor"" VARCHAR, ""Reproducciones"" INTEGER, ""GeneroId"" INTEGER, ""Url"" TEXT, ""AlbumId"" UUID, ""EstaActivo"" BOOLEAN, ""DuracionSegundos"" INTEGER, ""UsuarioId"" UUID)
                    AS $$
                    BEGIN
                        RETURN QUERY SELECT pi.""Id"", pi.""Nombre"", pi.""FechaRegistro"", pi.""AnoCreacion"", pi.""Interprete"", pi.""Compositor"", pi.""Productor"", pi.""Reproducciones"", pi.""GeneroId"", pi.""Url"", pi.""AlbumId"", pi.""EstaActivo"", pi.""DuracionSegundos"", pi.""UsuarioId""
                        FROM ""Pista"" pi INNER JOIN ""PlaylistPista"" pp ON pi.""Id"" = pp.""PistaId"" WHERE pp.""PlaylistId"" = p_PlaylistId;
                    END;
                    $$ LANGUAGE plpgsql;

                    -- BuscarPistas
                    DROP FUNCTION IF EXISTS ""BuscarPistas""(VARCHAR);
                    CREATE OR REPLACE FUNCTION ""BuscarPistas""(p_Busqueda VARCHAR)
                    RETURNS TABLE(""Id"" UUID, ""Nombre"" VARCHAR, ""FechaRegistro"" TIMESTAMP, ""AnoCreacion"" INTEGER, ""Interprete"" VARCHAR, ""Compositor"" VARCHAR, ""Productor"" VARCHAR, ""Reproducciones"" INTEGER, ""GeneroId"" INTEGER, ""Url"" TEXT, ""AlbumId"" UUID, ""EstaActivo"" BOOLEAN, ""DuracionSegundos"" INTEGER, ""UsuarioId"" UUID)
                    AS $$
                    BEGIN
                        RETURN QUERY SELECT pi.""Id"", pi.""Nombre"", pi.""FechaRegistro"", pi.""AnoCreacion"", pi.""Interprete"", pi.""Compositor"", pi.""Productor"", pi.""Reproducciones"", pi.""GeneroId"", pi.""Url"", pi.""AlbumId"", pi.""EstaActivo"", pi.""DuracionSegundos"", pi.""UsuarioId""
                        FROM ""Pista"" pi WHERE LOWER(pi.""Nombre"") LIKE LOWER('%' || p_Busqueda || '%') OR LOWER(pi.""Interprete"") LIKE LOWER('%' || p_Busqueda || '%');
                    END;
                    $$ LANGUAGE plpgsql;

                    -- ReproducirPista
                    CREATE OR REPLACE FUNCTION ""ReproducirPista""(p_Id UUID)
                    RETURNS TABLE(""Id"" UUID)
                    AS $$
                    BEGIN
                        UPDATE ""Pista"" SET ""Reproducciones"" = ""Reproducciones"" + 1 WHERE ""Id"" = p_Id;
                        RETURN QUERY SELECT p_Id;
                    END;
                    $$ LANGUAGE plpgsql;

                    -- AgregarPistaAPlaylist (ignora duplicados: una canción puede
                    -- estar en varias playlists, y una playlist mezcla artistas)
                    CREATE OR REPLACE FUNCTION ""AgregarPistaAPlaylist""(p_PlaylistId UUID, p_PistaId UUID)
                    RETURNS TABLE(""PlaylistId"" UUID, ""PistaId"" UUID, ""FechaRegistro"" TIMESTAMP)
                    AS $$
                    BEGIN
                        INSERT INTO ""PlaylistPista""(""PlaylistId"", ""PistaId"", ""FechaRegistro"")
                        VALUES (p_PlaylistId, p_PistaId, NOW())
                        -- Se referencia la PK por nombre: los parámetros de salida de
                        -- la función hacen ambigua la lista de columnas.
                        ON CONFLICT ON CONSTRAINT ""PlaylistPista_pkey"" DO NOTHING;

                        RETURN QUERY SELECT pp.""PlaylistId"", pp.""PistaId"", pp.""FechaRegistro""
                        FROM ""PlaylistPista"" pp
                        WHERE pp.""PlaylistId"" = p_PlaylistId AND pp.""PistaId"" = p_PistaId;
                    END;
                    $$ LANGUAGE plpgsql;

                    -- QuitarPistaDePlaylist
                    CREATE OR REPLACE FUNCTION ""QuitarPistaDePlaylist""(p_PlaylistId UUID, p_PistaId UUID)
                    RETURNS INTEGER
                    AS $$
                    BEGIN
                        DELETE FROM ""PlaylistPista"" WHERE ""PlaylistId"" = p_PlaylistId AND ""PistaId"" = p_PistaId;
                        RETURN 1;
                    END;
                    $$ LANGUAGE plpgsql;
                ";

                using var cmd5 = new NpgsqlCommand(createPistaFunctionsSql, connection);
                await cmd5.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing database: {ex.Message}");
                // Don't throw to allow app to start without DB for testing
            }
        }

        /// <summary>
        /// Si la tabla de rutas esta vacia, registra las rutas definidas en
        /// appsettings.json para no perder la configuracion existente.
        /// </summary>
        private async Task SembrarRutasAudioAsync(NpgsqlConnection connection)
        {
            using (var countCmd = new NpgsqlCommand(@"SELECT COUNT(*) FROM ""RutaAudio"";", connection))
            {
                var existentes = Convert.ToInt64(await countCmd.ExecuteScalarAsync());
                if (existentes > 0)
                    return;
            }

            var ubicaciones = _settings.AudioStorageLocations ?? new System.Collections.Generic.List<AudioStorageLocation>();
            var rutaPredeterminada = _settings.DefaultAudioStoragePath;
            bool hayPredeterminada = false;

            foreach (var ubicacion in ubicaciones)
            {
                bool esPredeterminada = !hayPredeterminada &&
                    !string.IsNullOrWhiteSpace(rutaPredeterminada) &&
                    (string.Equals(ubicacion.ContainerPath, rutaPredeterminada, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(ubicacion.PhysicalPath, rutaPredeterminada, StringComparison.OrdinalIgnoreCase));

                if (esPredeterminada)
                    hayPredeterminada = true;

                await InsertarRutaInicialAsync(
                    connection,
                    Guid.NewGuid(),
                    string.IsNullOrWhiteSpace(ubicacion.Name) ? "Ruta" : ubicacion.Name,
                    ubicacion.PhysicalPath,
                    ubicacion.ContainerPath,
                    ubicacion.IsActive,
                    esPredeterminada);
            }

            if (!hayPredeterminada && !string.IsNullOrWhiteSpace(rutaPredeterminada))
            {
                await InsertarRutaInicialAsync(
                    connection,
                    Guid.NewGuid(),
                    "Principal",
                    null,
                    rutaPredeterminada,
                    true,
                    true);
                hayPredeterminada = true;
            }

            if (!hayPredeterminada)
            {
                using var defaultCmd = new NpgsqlCommand(@"
                    UPDATE ""RutaAudio""
                    SET ""EsPredeterminada"" = TRUE
                    WHERE ""Id"" = (SELECT ""Id"" FROM ""RutaAudio"" ORDER BY ""FechaRegistro"" ASC LIMIT 1);", connection);
                await defaultCmd.ExecuteNonQueryAsync();
            }
        }

        private static async Task InsertarRutaInicialAsync(
            NpgsqlConnection connection,
            Guid id,
            string nombre,
            string rutaFisica,
            string rutaContenedor,
            bool estaActiva,
            bool esPredeterminada)
        {
            const string sql = @"
                INSERT INTO ""RutaAudio""
                    (""Id"", ""Nombre"", ""RutaFisica"", ""RutaContenedor"", ""Descripcion"", ""EstaActiva"", ""EsPredeterminada"", ""FechaRegistro"", ""FechaActualizacion"")
                VALUES
                    (@Id, @Nombre, @RutaFisica, @RutaContenedor, NULL, @EstaActiva, @EsPredeterminada, @FechaRegistro, NULL);";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("Id", id);
            cmd.Parameters.AddWithValue("Nombre", nombre);
            cmd.Parameters.AddWithValue("RutaFisica", (object)rutaFisica ?? string.Empty);
            cmd.Parameters.AddWithValue("RutaContenedor", (object)rutaContenedor ?? DBNull.Value);
            cmd.Parameters.AddWithValue("EstaActiva", estaActiva);
            cmd.Parameters.AddWithValue("EsPredeterminada", esPredeterminada);
            cmd.Parameters.AddWithValue("FechaRegistro", DateTime.UtcNow);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}

