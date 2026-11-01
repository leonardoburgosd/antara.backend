-- Esquema inicial de Base de Datos PostgreSQL para Antara
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE IF NOT EXISTS "Usuario" (
    "Id" UUID PRIMARY KEY,
    "Email" VARCHAR(255) NOT NULL UNIQUE,
    "Password" VARCHAR(255) NOT NULL,
    "Nombre" VARCHAR(255) NOT NULL,
    "FechaNacimiento" TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    "Genero" CHAR(1) NOT NULL,
    "EstaActivo" BOOLEAN NOT NULL DEFAULT TRUE,
    "FechaRegistro" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    "Pais" VARCHAR(100),
    "FotoPerfil" VARCHAR(500),
    "Tipo" VARCHAR(50) NOT NULL DEFAULT 'usuario'
);

CREATE TABLE IF NOT EXISTS "Album" (
    "Id" UUID PRIMARY KEY,
    "Nombre" VARCHAR(255) NOT NULL,
    "Descripcion" TEXT,
    "EstaPublicado" BOOLEAN NOT NULL DEFAULT FALSE,
    "FechaPublicacion" TIMESTAMP WITHOUT TIME ZONE,
    "PortadaUrl" VARCHAR(500),
    "UsuarioId" UUID REFERENCES "Usuario"("Id"),
    "EstaActivo" BOOLEAN NOT NULL DEFAULT TRUE,
    "Artista" VARCHAR(255),
    "FuenteMetadatos" VARCHAR(50),
    "IdExternoMetadatos" VARCHAR(100)
);

CREATE TABLE IF NOT EXISTS "Genero" (
    "Id" SERIAL PRIMARY KEY,
    "Nombre" VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS "Pista" (
    "Id" UUID PRIMARY KEY,
    "Nombre" VARCHAR(255) NOT NULL,
    "FechaRegistro" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    "AnoCreacion" INT,
    "Interprete" VARCHAR(255),
    "Compositor" VARCHAR(255),
    "Productor" VARCHAR(255),
    "Reproducciones" INT NOT NULL DEFAULT 0,
    "GeneroId" INT REFERENCES "Genero"("Id"),
    "Url" VARCHAR(1000) NOT NULL,
    "AlbumId" UUID REFERENCES "Album"("Id"),
    "EstaActivo" BOOLEAN NOT NULL DEFAULT TRUE,
    "Letra" TEXT,
    "ImagenArtista" VARCHAR(500),
    "DuracionSegundos" INT DEFAULT 0,
    "FuenteMetadatos" VARCHAR(50),
    "IdExternoMetadatos" VARCHAR(100),
    "MetadatosJson" TEXT
);

CREATE TABLE IF NOT EXISTS "Playlist" (
    "Id" UUID PRIMARY KEY,
    "Nombre" VARCHAR(255) NOT NULL,
    "Descripcion" TEXT,
    "PortadaUrl" VARCHAR(500),
    "UsuarioId" UUID REFERENCES "Usuario"("Id"),
    "EstaActivo" BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS "PlaylistPista" (
    "PlaylistId" UUID REFERENCES "Playlist"("Id") ON DELETE CASCADE,
    "PistaId" UUID REFERENCES "Pista"("Id") ON DELETE CASCADE,
    "FechaRegistro" TIMESTAMP WITHOUT TIME ZONE DEFAULT NOW(),
    PRIMARY KEY ("PlaylistId", "PistaId")
);

CREATE TABLE IF NOT EXISTS "RutaAudio" (
    "Id" UUID PRIMARY KEY,
    "Nombre" VARCHAR(255) NOT NULL,
    "RutaFisica" VARCHAR(1000),
    "RutaContenedor" VARCHAR(1000),
    "Descripcion" TEXT,
    "EstaActiva" BOOLEAN NOT NULL DEFAULT TRUE,
    "EsPredeterminada" BOOLEAN NOT NULL DEFAULT FALSE,
    "FechaRegistro" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT NOW(),
    "FechaActualizacion" TIMESTAMP WITHOUT TIME ZONE
);

