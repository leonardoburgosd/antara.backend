using Antara.Model.Dtos;
using Antara.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Antara.Model
{
    public static class Extensions
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
                FechaRegistro = usuario.FechaRegistro,
                Pais = usuario.Pais,
                FotoPerfil = usuario.FotoPerfil
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
                EstaActivo = pista.EstaActivo
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
                EstaActivo = album.EstaActivo
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
