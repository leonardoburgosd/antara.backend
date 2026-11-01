using AntaraSoft.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AntaraSoft.Application.Contracts.Services
{
    public interface IRegistrarUsuarioService
    {
        Task CrearUsuario(Usuario usuario);
        Task<Usuario> ObtenerUsuario(Guid id);
        Task<List<Usuario>> ObtenerTodosUsuarios();
        Task<bool> ExisteAdministrador();
        Task<bool> TieneAdministrador();
        Task<int> ContarUsuarios();
        Task<bool> EsEmailValido(string email);
    }

    public interface ILoginService
    {
        Task<Usuario> Login(string email, string password);
    }

    public interface ITokenService
    {
        string GenerarToken(Usuario usuario);
        TokenValidationResult ValidarToken(string token);
    }

    public class TokenValidationResult
    {
        public bool EsValido { get; set; }
        public Guid UsuarioId { get; set; }
        public string Email { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public DateTime ExpiraEn { get; set; }
        public string Error { get; set; }
    }

    public interface IEncryptText
    {
        string Encrypt(string text);
        bool Verify(string text, string hash);
        string GeneratePasswordHash(string texto);
        bool CompararHash(string textoNoEncriptado, string textoEncriptado);
    }

    public interface IGestionarPistaService
    {
        Task CrearPista(Pista pista);
        Task EditarPista(Pista pista);
        Task EliminarPista(Guid id);
        Task<Pista> ObtenerPista(Guid id);
        Task<List<Pista>> ObtenerTodosPistasDeAlbum(Guid albumId);
        Task<List<Pista>> ObtenerTodosPistasDePlaylist(Guid playlistId);
        Task<List<Pista>> BuscarPistas(string cadena);
        Task ReproducirPista(Pista pista);
        bool SonDatosValidos(Pista pista);
    }

    public interface IGestionarAlbumService
    {
        Task<Album> ObtenerAlbum(Guid id);
        Task<List<Album>> ObtenerTodosAlbumes();
        Task<List<Album>> ObtenerTodosAlbumesDeUsuario(Guid userId);
        Task CrearAlbum(Album album);
        Task EliminarAlbum(Guid id);
        Task EditarAlbum(Album album);
        Task<bool> PublicarAlbum(Album album);
        bool SonDatosValidos(Album album);
    }

    public interface IGestionarPlaylistService
    {
        Task<Playlist> ObtenerPlaylist(Guid id);
        Task<List<Playlist>> ObtenerTodasPlaylists(Guid usuarioId);
        Task<List<Playlist>> ObtenerTodosPlaylistDeUsuario(Guid userId);
        Task CrearPlaylist(Playlist playlist);
        Task EliminarPlaylist(Guid id);
        Task EditarPlaylist(Playlist playlist);
        Task<bool> AgregarPistaAPlaylist(PlaylistPista playlistPista);
        Task<bool> QuitarPistaDePlaylist(PlaylistPista playlistPista);
    }

    public interface IRutaAudioService
    {
        Task<List<RutaAudio>> ObtenerTodas();
        Task<RutaAudio> ObtenerPorId(Guid id);
        Task<RutaAudio> Crear(CrearRutaAudioDto dto);
        Task<RutaAudio> Actualizar(Guid id, ActualizarRutaAudioDto dto);
        Task Eliminar(Guid id);
        Task<PruebaRutaAudioDto> Probar(Guid id);
    }

    public interface IAudioStorage
    {
        string ResolvePhysicalPath(string relativeOrStoredPath);
        bool FileExists(string relativeOrStoredPath);
    }

    public interface IEscaneoAudioService
    {
        EscaneoAudioDto Iniciar(Guid usuarioId);
        EscaneoAudioDto ObtenerActual();
        EscaneoAudioDto Obtener(Guid id);
        bool Cancelar(Guid id);
    }

    public interface ISistemaService
    {
        Task<bool> TieneAdministrador();
        Task<int> ContarUsuarios();
        Task<Usuario> ObtenerUsuario(Guid id);
        Task CrearUsuario(Usuario usuario);
    }

    public interface IMetadatosPistaService
    {
        Task<List<MetadatosCandidatoDto>> BuscarAsync(string consulta, CancellationToken cancellationToken);
        Task<List<MetadatosCandidatoDto>> BuscarAsync(string consulta, string proveedor, CancellationToken cancellationToken);
        Task<PistaBibliotecaDto> ActualizarAsync(Guid pistaId, string idExterno, Guid usuarioId, CancellationToken cancellationToken);
        Task<PistaBibliotecaDto> ActualizarAsync(Guid pistaId, string idExterno, string proveedor, Guid usuarioId, CancellationToken cancellationToken);
    }

    public interface IMetadataProvider
    {
        string Nombre { get; }
        Task<MetadatosAudio> BuscarAsync(string artista, string titulo, string carpeta, CancellationToken cancellationToken);
        Task<List<MetadatosAudio>> BuscarCandidatosAsync(string consulta, CancellationToken cancellationToken);
        Task<MetadatosAudio> ObtenerPorIdAsync(string idExterno, CancellationToken cancellationToken);
    }

    public interface ILetrasProvider
    {
        Task<string> ObtenerAsync(string artista, string titulo, CancellationToken cancellationToken);
    }
}
