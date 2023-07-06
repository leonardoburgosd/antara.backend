using Antara.Model;
using Antara.Model.Entities;
using Antara.Repository.Repositories;
using Antara.Service;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;


namespace AntaraTest
{
    [TestClass]
    public class GestionarAlbumServiceTest
    {
        private readonly GestionarAlbumService _albumService;
        public GestionarAlbumServiceTest()
        {
            var options = Options.Create(new AppSettings());
            options.Value.ConexionString = "Server=.;Database=antaradb;Trusted_Connection=True;MultipleActiveResultSets=True";
            var dapper = new Antara.Repository.Dapper.Dapper(options);
            var albumRepo = new AlbumRepository(dapper);
            _albumService = new GestionarAlbumService(albumRepo);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        public void CrearAlbumTest(int caso)
        {
            Album esperado = new();
            esperado.Id = Guid.NewGuid();
            esperado.Nombre = "TestAlbum";
            esperado.Descripcion = "Album de prueba";
            esperado.FechaPublicacion = DateTime.Parse("1900-01-01");
            esperado.EstaPublicado = false;
            esperado.UsuarioId = Guid.Parse("ae81eaaf-f99f-4980-b782-0ea359402daf");
            esperado.EstaActivo = true;
            switch (caso)
            {
                case 0:
                    _albumService.CrearAlbum(esperado).Wait();
                    Album actual = _albumService.ObtenerAlbum(esperado.Id).Result;
                    if (actual != null)
                    {
                        Assert.IsNotNull(actual.Id);
                        Assert.AreEqual(esperado.Nombre, actual.Nombre);
                        Assert.AreEqual(esperado.Descripcion, actual.Descripcion);
                        Assert.AreEqual(esperado.UsuarioId, actual.UsuarioId);
                        _albumService.EliminarAlbum(actual.Id).Wait();
                    }
                    break;
                case 1:
                    esperado.Nombre = null;
                    Assert.ThrowsException<AggregateException>(() =>
                    {
                        _albumService.CrearAlbum(esperado).Wait();
                    });
                    break;
            }

        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        public void EditarAlbumTest(int caso)
        {
            Album esperado = new();
            esperado.Id = Guid.Parse("E849DF3C-44E7-4D18-899E-FFB29BADC8A7");
            esperado.Nombre = "TestAlbum";
            esperado.Descripcion = "Album de prueba";
            esperado.UsuarioId = Guid.Parse("AE81EAAF-F99F-4980-B782-0EA359402DAF");

            switch (caso)
            {
                case 0:
                    _albumService.EditarAlbum(esperado).Wait();
                    Album actual = _albumService.ObtenerAlbum(esperado.Id).Result;
                    if (actual != null)
                    {
                        Assert.IsNotNull(actual.Id);
                        Assert.AreEqual(esperado.Nombre, actual.Nombre);
                        Assert.AreEqual(esperado.Descripcion, actual.Descripcion);
                        Assert.AreEqual(esperado.UsuarioId, actual.UsuarioId);
                    }
                    break;
                case 1:
                    esperado.Nombre = null;
                    Assert.ThrowsException<AggregateException>(() =>
                    {
                        _albumService.EditarAlbum(esperado).Wait();
                    });
                    break;
            }
        }

        [TestMethod]
        public void EliminarAlbumTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _albumService.EliminarAlbum(Guid.Empty).Wait();
            });
        }

        [TestMethod]
        public void ObtenerAlbumTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _albumService.ObtenerAlbum(Guid.Empty).Wait();
            });
        }

        [DataRow(0)]
        [DataRow(1)]
        [TestMethod]
        public void ObtenerTodosAlbumsDeUsuarioTest(int caso)
        {
            switch(caso)
            {
                case 0:
                    Assert.ThrowsException<ArgumentNullException>(() =>
                    {
                        List<Album> agrupacionList = _albumService.ObtenerTodosAlbumesDeUsuario(Guid.Empty).Result;
                    });
                    break;
                case 1:
                    List<Album> agrupacionList = _albumService.ObtenerTodosAlbumesDeUsuario(Guid.Parse("AE81EAAF-F99F-4980-B782-0EA359402DAF")).Result;
                    Assert.IsNotNull(agrupacionList);
                    break;
            }
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        public void PublicarAlbumTest(int caso)
        {
            Album album = _albumService.ObtenerAlbum(Guid.Parse("E849DF3C-44E7-4D18-899E-FFB29BADC8A7")).Result;

            switch (caso)
            {
                case 0:
                    if(album != null)
                    {
                        bool estaPublicado = _albumService.PublicarAlbum(album).Result;
                        Assert.IsTrue(estaPublicado);
                    }
                    break;
                case 1:
                    if (album != null)
                    {
                        Assert.ThrowsException<AggregateException>(() =>
                        {
                            bool estaPublicado = _albumService.PublicarAlbum(album).Result;
                        });
                    }
                    break;

            }
        }
    }
}
