using System;
using System.Collections.Generic;

namespace AntaraSoft.Application
{
    public class UsuarioDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public char Genero { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Pais { get; set; }
        public string FotoPerfil { get; set; }
        public string Tipo { get; set; }
    }

    public class CrearUsuarioDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public char Genero { get; set; }
        public string Pais { get; set; }
        public string Tipo { get; set; }
    }

    public class LoginUsuarioDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class PistaDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int AnoCreacion { get; set; }
        public string Interprete { get; set; }
        public string Compositor { get; set; }
        public string Productor { get; set; }
        public int Reproducciones { get; set; }
        public int GeneroId { get; set; }
        public string Url { get; set; }
        public Guid AlbumId { get; set; }
        public bool EstaActivo { get; set; }
        public string Letra { get; set; }
        public string ImagenArtista { get; set; }
        public int DuracionSegundos { get; set; }
        public string FuenteMetadatos { get; set; }
        public string IdExternoMetadatos { get; set; }
    }

    public class PistaBibliotecaDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Interprete { get; set; }
        public Guid AlbumId { get; set; }
        public string AlbumNombre { get; set; }
        public string PortadaUrl { get; set; }
        public int AnoCreacion { get; set; }
        public int GeneroId { get; set; }
        public string GeneroNombre { get; set; }
        public int DuracionSegundos { get; set; }
        public int Reproducciones { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Url { get; set; }
        public bool TieneLetra { get; set; }
        public string Letra { get; set; }
        public string ImagenArtista { get; set; }
        public string FuenteMetadatos { get; set; }
        public bool EstaActivo { get; set; }
    }

    public class CrearPistaDto
    {
        public int AnoCreacion { get; set; }
        public string Interprete { get; set; }
        public string Compositor { get; set; }
        public string Productor { get; set; }
        public int GeneroId { get; set; }
        public Guid AlbumId { get; set; }
        public string Url { get; set; }
        public string RutaFisica { get; set; }
    }

    public class EditarPistaDto
    {
        public string Nombre { get; set; }
        public int AnoCreacion { get; set; }
        public string Interprete { get; set; }
        public string Compositor { get; set; }
        public string Productor { get; set; }
        public int GeneroId { get; set; }
    }

    public class ActualizarMetadatosPistaDto
    {
        public string IdExterno { get; set; }
        public string Proveedor { get; set; }
    }

    public class MetadatosCandidatoDto
    {
        public string IdExterno { get; set; }
        public string Titulo { get; set; }
        public string Artista { get; set; }
        public string Album { get; set; }
        public string PortadaUrl { get; set; }
        public string ImagenArtista { get; set; }
        public int Ano { get; set; }
        public int DuracionSegundos { get; set; }
        public string Fuente { get; set; }
    }

    public class AlbumDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool EstaPublicado { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public string PortadaUrl { get; set; }
        public Guid UsuarioId { get; set; }
        public bool EstaActivo { get; set; }
        public string Artista { get; set; }
        public string FuenteMetadatos { get; set; }
        public string IdExternoMetadatos { get; set; }
    }

    public class CrearAgrupacionDto
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public Guid UsuarioId { get; set; }
    }

    public class EditarAgrupacionDto
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string PortadaUrl { get; set; }
    }

    public class PlaylistDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public Guid UsuarioId { get; set; }
        public string PortadaUrl { get; set; }
        public bool EstaActivo { get; set; }
    }

    public class RutaAudioDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string RutaFisica { get; set; }
        public string RutaContenedor { get; set; }
        public string Descripcion { get; set; }
        public bool EstaActiva { get; set; }
        public bool EsPredeterminada { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public string RutaEfectiva { get; set; }
    }

    public class CrearRutaAudioDto
    {
        public string Nombre { get; set; }
        public string RutaFisica { get; set; }
        public string RutaContenedor { get; set; }
        public string Descripcion { get; set; }
        public bool EstaActiva { get; set; } = true;
        public bool EsPredeterminada { get; set; }
    }

    public class ActualizarRutaAudioDto
    {
        public string Nombre { get; set; }
        public string RutaFisica { get; set; }
        public string RutaContenedor { get; set; }
        public string Descripcion { get; set; }
        public bool EstaActiva { get; set; }
        public bool EsPredeterminada { get; set; }
    }

    public class PruebaRutaAudioDto
    {
        public string RutaEvaluada { get; set; }
        public bool RutaFisicaConfigurada { get; set; }
        public bool RutaFisicaVisible { get; set; }
        public bool Existe { get; set; }
        public bool EsDirectorio { get; set; }
        public bool Escribible { get; set; }
        public int ArchivosAudio { get; set; }
        public int Subdirectorios { get; set; }
        public string Mensaje { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class DashboardDto
    {
        public int UsuariosTotales { get; set; }
        public int UsuariosActivos { get; set; }
        public int AlbumesTotales { get; set; }
        public int PistasTotales { get; set; }
        public int PlaylistsTotales { get; set; }
        public int ReproduccionesTotales { get; set; }
        public List<DashboardPistaDto> PistasRecientes { get; set; } = new();
        public List<DashboardPistaDto> PistasMasReproducidas { get; set; } = new();
        public List<DashboardUsuarioDto> UsuariosRecientes { get; set; } = new();
        public List<DashboardAlbumDto> AlbumesRecientes { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }

    public class DashboardPistaDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Interprete { get; set; }
        public string AlbumNombre { get; set; }
        public int Reproducciones { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    public class DashboardUsuarioDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Tipo { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    public class DashboardAlbumDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string PortadaUrl { get; set; }
        public bool EstaPublicado { get; set; }
        public DateTime? FechaPublicacion { get; set; }
    }

    public class EscaneoAudioDto
    {
        public Guid Id { get; set; }
        public string Estado { get; set; }
        public bool Activo { get; set; }
        public DateTime IniciadoEn { get; set; }
        public DateTime? FinalizadoEn { get; set; }
        public int TotalArchivos { get; set; }
        public int Pendientes { get; set; }
        public int Procesados { get; set; }
        public int Nuevos { get; set; }
        public int SinMetadatos { get; set; }
        public int Omitidos { get; set; }
        public int Errores { get; set; }
        public string ArchivoActual { get; set; }
        public string Mensaje { get; set; }
        public int MaxParalelo { get; set; }
        public List<EscaneoAudioResultadoDto> Resultados { get; set; } = new();
    }

    public class EscaneoAudioResultadoDto
    {
        public string Archivo { get; set; }
        public string Nombre { get; set; }
        public string Interprete { get; set; }
        public string Album { get; set; }
        public string PortadaUrl { get; set; }
        public string ImagenArtista { get; set; }
        public int Ano { get; set; }
        public bool TieneLetra { get; set; }
        public string Estado { get; set; }
        public string Mensaje { get; set; }
        public DateTime Fecha { get; set; }
    }

    public class SistemaDto
    {
        public bool Activo { get; set; }
        public string Version { get; set; }
        public DateTime Timestamp { get; set; }
        public int UsuariosTotales { get; set; }
        public bool AdminsRegistrados { get; set; }
        public string Estado { get; set; }
    }
}
