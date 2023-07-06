using Antara.Model;
using Antara.Model.Entities;
using Antara.Repository.Repositories;
using Antara.Service;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntaraTest
{
    [TestClass]
    public class GestionarPlaylistServiceTest
    {
        private readonly GestionarPlaylistService _playlistService;
        public GestionarPlaylistServiceTest()
        {
            var options = Options.Create(new AppSettings());
            options.Value.ConexionString = "Server=.;Database=antaradb;Trusted_Connection=True;MultipleActiveResultSets=True";
            var dapper = new Antara.Repository.Dapper.Dapper(options);
            var playlistRepo = new PlaylistRepository(dapper);
            _playlistService = new GestionarPlaylistService(playlistRepo);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        public void CrearPlaylistTest(int caso)
        {
            Playlist esperado = new();
            esperado.Id = Guid.NewGuid();
            esperado.Nombre = "TestPlaylist";
            esperado.Descripcion = "Playlist de prueba";
            esperado.UsuarioId = Guid.Parse("ae81eaaf-f99f-4980-b782-0ea359402daf");
            esperado.EstaActivo = true;
            switch (caso)
            {
                case 0:
                    _playlistService.CrearPlaylist(esperado).Wait();
                    Playlist actual = _playlistService.ObtenerPlaylist(esperado.Id).Result;
                    if (actual != null)
                    {
                        Assert.IsNotNull(actual.Id);
                        Assert.AreEqual(esperado.Nombre, actual.Nombre);
                        Assert.AreEqual(esperado.Descripcion, actual.Descripcion);
                        Assert.AreEqual(esperado.UsuarioId, actual.UsuarioId);
                        Assert.IsTrue(actual.EstaActivo);
                        _playlistService.EliminarPlaylist(actual.Id).Wait();
                    }
                    break;
                case 1:
                    esperado.Nombre = null;
                    Assert.ThrowsException<AggregateException>(() =>
                    {
                        _playlistService.CrearPlaylist(esperado).Wait();
                    });
                    break;
            }

        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        public void EditarPlaylistTest(int caso)
        {
            Playlist esperado = new();
            esperado.Id = Guid.Parse("C8BE77D0-87A0-4758-9748-5B3A89EA3973");
            esperado.Nombre = "TestPlaylistUpdate";
            esperado.Descripcion = "Playlist de prueba update";

            switch (caso)
            {
                case 0:
                    _playlistService.EditarPlaylist(esperado).Wait();
                    Playlist actual = _playlistService.ObtenerPlaylist(esperado.Id).Result;
                    if (actual != null)
                    {
                        Assert.IsNotNull(actual.Id);
                        Assert.AreEqual(esperado.Nombre, actual.Nombre);
                        Assert.AreEqual(esperado.Descripcion, actual.Descripcion);
                    }
                    break;
                case 1:
                    esperado.Nombre = null;
                    Assert.ThrowsException<AggregateException>(() =>
                    {
                        _playlistService.EditarPlaylist(esperado).Wait();
                    });
                    break;
            }
        }

        [TestMethod]
        public void EliminarPlaylistTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _playlistService.EliminarPlaylist(Guid.Empty).Wait();
            });
        }

        [TestMethod]
        public void ObtenerPlaylistTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _playlistService.ObtenerPlaylist(Guid.Empty).Wait();
            });
        }

        [DataRow(0)]
        [DataRow(1)]
        [TestMethod]
        public void ObtenerTodosPlaylistsDeUsuarioTest(int caso)
        {
            switch (caso)
            {
                case 0:
                    Assert.ThrowsException<ArgumentNullException>(() =>
                    {
                        List<Playlist> agrupacionList = _playlistService.ObtenerTodosPlaylistDeUsuario(Guid.Empty).Result;
                    });
                    break;
                case 1:
                    List<Playlist> agrupacionList = _playlistService.ObtenerTodosPlaylistDeUsuario(Guid.Parse("AE81EAAF-F99F-4980-B782-0EA359402DAF")).Result;
                    Assert.IsNotNull(agrupacionList);
                    break;
            }
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        public void AgregarPistaAPlaylistTest(int caso)
        {
            PlaylistPista esperado = new();
            esperado.PistaId = Guid.Parse("3CC1C7CD-5522-4AD3-A5EC-515DAECE6AA5");
            esperado.PlaylistId = Guid.Parse("C8BE77D0-87A0-4758-9748-5B3A89EA3973");
            if (caso == 0)
            {
                esperado.PlaylistId = Guid.Empty;
                Assert.ThrowsException<ArgumentNullException>(() =>
                {
                    _playlistService.AgregarPistaAPlaylist(esperado);
                });
            }
            else
            {
                bool respuesta = _playlistService.AgregarPistaAPlaylist(esperado).Result;
                Assert.IsTrue(respuesta);
                _playlistService.QuitarPistaDePlaylist(esperado).Wait();
            }

        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        public void QuitarPistaDePlaylist(int caso)
        {
            PlaylistPista playlistPista = new();
            switch (caso)
            {
                case 0:
                    playlistPista.PlaylistId = Guid.Parse("C8BE77D0-87A0-4758-9748-5B3A89EA3973");
                    playlistPista.PistaId = Guid.Empty;
                    Assert.ThrowsException<ArgumentNullException>(() =>
                    {
                        _playlistService.QuitarPistaDePlaylist(playlistPista).Wait();
                    });
                    break;
                case 1:
                    playlistPista.PlaylistId = Guid.Empty; ;
                    playlistPista.PistaId = Guid.Parse("3CC1C7CD-5522-4AD3-A5EC-515DAECE6AA5");
                    Assert.ThrowsException<ArgumentNullException>(() =>
                    {
                        _playlistService.QuitarPistaDePlaylist(playlistPista).Wait();
                    });
                    break;
            }
        }
    }
}
