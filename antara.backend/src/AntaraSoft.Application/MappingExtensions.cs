using AntaraSoft.Domain.Entities;

namespace AntaraSoft.Application
{
    public static class MappingExtensions
    {
        public static UsuarioDto AsDto(this Usuario usuario)
        {
            return new UsuarioDto
            {
                Id = usuario.Id,
                Email = usuario.Email,
                Nombre = usuario.Nombre,
                FechaNacimiento = usuario.FechaNacimiento,
                Genero = usuario.Genero,
                EstaActivo = usuario.EstaActivo,
                FechaRegistro = usuario.FechaRegistro,
                Pais = usuario.Pais,
                FotoPerfil = usuario.FotoPerfil,
                Tipo = usuario.Tipo
            };
        }

        public static PistaDto AsDto(this Pista pista)
        {
            return new PistaDto
            {
                Id = pista.Id,
                Nombre = pista.Nombre,
                FechaRegistro = pista.FechaRegistro,
                AnoCreacion = pista.AnoCreacion,
                Interprete = pista.Interprete,
                Compositor = pista.Compositor,
                Productor = pista.Productor,
                Reproducciones = pista.Reproducciones,
                GeneroId = pista.GeneroId,
                Url = pista.Url,
                AlbumId = pista.AlbumId,
                EstaActivo = pista.EstaActivo,
                Letra = pista.Letra,
                ImagenArtista = pista.ImagenArtista,
                DuracionSegundos = pista.DuracionSegundos,
                FuenteMetadatos = pista.FuenteMetadatos,
                IdExternoMetadatos = pista.IdExternoMetadatos
            };
        }

        public static AlbumDto AsDto(this Album album)
        {
            return new AlbumDto
            {
                Id = album.Id,
                Nombre = album.Nombre,
                Descripcion = album.Descripcion,
                FechaPublicacion = album.FechaPublicacion,
                EstaPublicado = album.EstaPublicado,
                UsuarioId = album.UsuarioId,
                PortadaUrl = album.PortadaUrl,
                EstaActivo = album.EstaActivo,
                Artista = album.Artista,
                FuenteMetadatos = album.FuenteMetadatos,
                IdExternoMetadatos = album.IdExternoMetadatos
            };
        }

        public static RutaAudioDto AsDto(this RutaAudio ruta)
        {
            return new RutaAudioDto
            {
                Id = ruta.Id,
                Nombre = ruta.Nombre,
                RutaFisica = ruta.RutaFisica,
                RutaContenedor = ruta.RutaContenedor,
                Descripcion = ruta.Descripcion,
                EstaActiva = ruta.EstaActiva,
                EsPredeterminada = ruta.EsPredeterminada,
                FechaRegistro = ruta.FechaRegistro,
                FechaActualizacion = ruta.FechaActualizacion,
                RutaEfectiva = string.IsNullOrWhiteSpace(ruta.RutaContenedor) ? ruta.RutaFisica : ruta.RutaContenedor
            };
        }

        public static PlaylistDto AsDto(this Playlist playlist)
        {
            return new PlaylistDto
            {
                Id = playlist.Id,
                Nombre = playlist.Nombre,
                Descripcion = playlist.Descripcion,
                UsuarioId = playlist.UsuarioId,
                PortadaUrl = playlist.PortadaUrl,
                EstaActivo = playlist.EstaActivo
            };
        }
    }
}

