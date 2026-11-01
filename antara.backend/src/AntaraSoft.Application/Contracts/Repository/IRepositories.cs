using AntaraSoft.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AntaraSoft.Application.Contracts.Repository
{
    public interface IUsuarioRepository
    {
        Task<Usuario> ObtenerUsuario(Guid id);
        Task<List<Usuario>> ObtenerTodosUsuarios();
        Task CrearUsuario(Usuario usuario);
        Task<bool> VerificarEmailUnico(string email);
        Task<bool> ExisteAdministrador();
        Task<Usuario> Login(string email);
        Task EliminarFisicoUsuario(Guid id);
    }

    public interface IPistaRepository
    {
        Task CrearPista(Pista pista);
        Task EliminarPista(Guid id);
        Task EditarPista(Pista pista);
        Task<Pista> ObtenerPista(Guid id);
        Task<List<Pista>> ObtenerTodosPistasDeAlbum(Guid albumId);
        Task<List<Pista>> ObtenerTodosPistasDePlaylist(Guid playlistId);
        Task<List<Pista>> BuscarPistass(string cadena);
        Task ReproducirPista(Guid id, int reproducciones);
    }

    public interface IAlbumRepository
    {
        Task<Album> ObtenerAlbum(Guid id);
        Task<List<Album>> ObtenerTodosAlbumes();
        Task<List<Album>> ObtenerTodosAlbumesDeUsuario(Guid userId);
        Task CrearAlbum(Album album);
        Task EliminarAlbum(Guid id);
        Task EditarAlbum(Album album);
        Task<bool> PublicarAlbum(Album album);
    }

    public interface IPlaylistRepository
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

    public interface IRutaAudioRepository
    {
        Task<RutaAudio> ObtenerPorId(Guid id);
        Task<List<RutaAudio>> ObtenerTodas();
        Task<bool> ExisteNombre(string nombre, Guid? excluirId = null);
        Task<bool> ExistePredeterminada();
        Task Crear(RutaAudio ruta);
        Task Actualizar(RutaAudio ruta);
        Task Eliminar(Guid id);
        Task QuitarPredeterminada(Guid? excluirId = null);
        Task EstablecerPredeterminada(Guid id);
    }

    public interface IDashboardRepository
    {
        Task<DashboardDto> ObtenerDashboard();
    }

    public interface IBibliotecaRepository
    {
        Task<HashSet<string>> ObtenerRutasRegistradas();
        Task<List<PistaBibliotecaDto>> ObtenerPistas();
        Task<PistaBibliotecaDto> ObtenerPistaBiblioteca(Guid id);
        Task ActualizarMetadatosPista(Pista pista);
        Task<Guid> ObtenerOCrearAlbum(Album album);
        Task<int> ObtenerOCrearGenero(string nombre);
        Task CrearPista(Pista pista);
    }
}
