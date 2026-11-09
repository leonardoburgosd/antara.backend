using AntaraSoft.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AntaraSoft.Application.Contracts.Services
{
    public interface IRegistrarUsuarioService
    {
        Task CrearUsuario(Usuario usuario);
        Task<Usuario> ObtenerUsuario(Guid id);
        Task<List<Usuario>> ObtenerTodosUsuarios();
        Task<Usuario> ActualizarUsuario(Usuario usuario);
        Task EliminarUsuario(Guid id);
        Task CambiarPassword(Guid id, string passwordActual, string passwordNueva);
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
        Task<List<PlaylistResumenDto>> ObtenerResumenes();
        Task<List<PistaBibliotecaDto>> ObtenerPistasDePlaylist(Guid playlistId);
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

        /// <summary>
        /// Registra un archivo de audio concreto reutilizando el enriquecido de
        /// metadatos del escaneo (alta de album, genero, portada y letra).
        /// </summary>
        Task<EscaneoAudioResultadoDto> ImportarArchivoAsync(string archivo, Guid usuarioId, CancellationToken cancellationToken = default);
    }

    public interface ISubidaPistaService
    {
        /// <summary>Rutas activas que pueden usarse como destino de una subida.</summary>
        Task<List<RutaSubidaDto>> ObtenerRutasAsync();

        /// <summary>
        /// Guarda el archivo dentro de la ruta indicada (o la predeterminada) y lo
        /// registra en la biblioteca. Solo acepta extensiones de audio.
        /// </summary>
        Task<SubidaPistaDto> SubirAsync(
            Guid? rutaId,
            string subcarpeta,
            string nombreArchivo,
            Stream contenido,
            Guid usuarioId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Descarga el audio de un enlace de YouTube en la ruta indicada (o la
        /// predeterminada) y lo registra en la biblioteca.
        /// </summary>
        Task<SubidaPistaDto> SubirDesdeUrlAsync(
            Guid? rutaId,
            string subcarpeta,
            string url,
            Guid usuarioId,
            CancellationToken cancellationToken = default);
    }

    /// <summary>Resultado de descargar el audio de un enlace externo.</summary>
    public class DescargaAudio
    {
        public string Archivo { get; set; }
        public string Titulo { get; set; }
    }

    public interface IDescargadorYouTube
    {
        /// <summary>Indica si la URL pertenece a un dominio de YouTube admitido.</summary>
        bool EsUrlYouTube(string url);

        /// <summary>
        /// Descarga el audio del enlace dentro del directorio indicado, siempre en
        /// un formato de audio, y devuelve la ruta final del archivo.
        /// </summary>
        Task<DescargaAudio> DescargarAsync(string url, string directorio, CancellationToken cancellationToken = default);
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

        /// <summary>
        /// Letra en formato LRC de la pista, o <c>null</c> si el proveedor no
        /// tiene una version sincronizada para esa grabacion.
        /// </summary>
        Task<string> ObtenerLetraSincronizadaAsync(Guid pistaId, CancellationToken cancellationToken);

        /// <summary>Proveedores habilitados, tal como quedaron configurados.</summary>
        List<ProveedorMetadatosDto> ObtenerProveedores();
    }

    public interface IMetadataProvider
    {
        /// <summary>Identificador del proveedor (viene de la configuración).</summary>
        string Nombre { get; }

        /// <summary>Nombre visible en el panel (viene de la configuración).</summary>
        string Etiqueta { get; }
        Task<MetadatosAudio> BuscarAsync(string artista, string titulo, string carpeta, CancellationToken cancellationToken);
        Task<List<MetadatosAudio>> BuscarCandidatosAsync(string consulta, CancellationToken cancellationToken);
        Task<MetadatosAudio> ObtenerPorIdAsync(string idExterno, CancellationToken cancellationToken);
    }

    public interface ILetrasProvider
    {
        Task<string> ObtenerAsync(string artista, string titulo, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Letra con marcas de tiempo en formato LRC. Permite resaltar la linea que
    /// suena en cada momento; no todas las pistas tienen una version sincronizada.
    /// </summary>
    public interface ILetrasSincronizadasProvider
    {
        Task<string> ObtenerAsync(
            string artista,
            string titulo,
            string album,
            int duracionSegundos,
            CancellationToken cancellationToken);
    }
}
