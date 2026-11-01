using System;
using System.Collections.Generic;

namespace AntaraSoft.Application.Contracts.Services
{
    public class MetadatosAudio
    {
        public bool Encontrado { get; set; }
        public string Fuente { get; set; }
        public string IdExterno { get; set; }
        public string Titulo { get; set; }
        public List<string> Artistas { get; set; } = new();
        public string Album { get; set; }
        public string IdAlbumExterno { get; set; }
        public string PortadaUrl { get; set; }
        public string ImagenArtistaUrl { get; set; }
        public int Ano { get; set; }
        public DateTime? FechaPublicacion { get; set; }
        public string Genero { get; set; }
        public string Sello { get; set; }
        public int DuracionSegundos { get; set; }
        public string Letra { get; set; }
        public string Isrc { get; set; }
        public string Json { get; set; }

        public string ArtistaPrincipal =>
            Artistas != null && Artistas.Count > 0 ? Artistas[0] : null;
    }
}

