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
    public class GestionarPistaServiceTest
    {
        private readonly GestionarPistaService _pistaService;
        public GestionarPistaServiceTest()
        {
            var options = Options.Create(new AppSettings());
            options.Value.ConexionString = "Server=.;Database=antaradb;Trusted_Connection=True;MultipleActiveResultSets=True";
            var dapper = new Antara.Repository.Dapper.Dapper(options);
            var pistaRepo = new PistaRepository(dapper);
            var albumRepo = new AlbumRepository(dapper);
            _pistaService = new GestionarPistaService(pistaRepo, albumRepo);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        public void CrearPistaTest(int caso)
        {
            Pista esperado = new();
            esperado.Id = Guid.NewGuid();
            esperado.Nombre = "TestAudio0";
            esperado.FechaRegistro = DateTime.Now;
            esperado.AnoCreacion = 1992;
            esperado.Interprete = "Guns N' Roses";
            esperado.Compositor = "Axl Rose";
            esperado.Productor = "Diego";
            esperado.Reproducciones = 0;
            esperado.GeneroId = 27;
            esperado.Url = "youtube.com";
            esperado.AlbumId = Guid.Parse("E849DF3C-44E7-4D18-899E-FFB29BADC8A7");
            esperado.EstaActivo = true;
            switch (caso)
            {
                case 0:
                    _pistaService.CrearPista(esperado).Wait();
                    Pista actual = _pistaService.ObtenerPista(esperado.Id).Result;
                    if (actual != null)
                    {
                        Assert.IsNotNull(actual.Id);
                        Assert.AreEqual(esperado.Nombre, actual.Nombre);
                        Assert.AreEqual(esperado.Url, actual.Url);
                        Assert.AreEqual(esperado.AnoCreacion, actual.AnoCreacion);
                        Assert.AreEqual(esperado.Interprete, actual.Interprete);
                        Assert.AreEqual(esperado.Compositor, actual.Compositor);
                        Assert.AreEqual(esperado.Productor, actual.Productor);
                        Assert.IsNotNull(actual.FechaRegistro);
                        Assert.AreEqual(esperado.GeneroId, actual.GeneroId);
                        Assert.AreEqual(esperado.AlbumId, actual.AlbumId);

                        _pistaService.EliminarPista(actual.Id).Wait();
                    }
                    break;
                case 1:
                    esperado.Nombre = null;
                    Assert.ThrowsException<AggregateException>(() =>
                    {
                        _pistaService.CrearPista(esperado).Wait();
                    });
                    break;
            }

        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        public void EditarPistaTest(int caso)
        {
            Pista esperado = new();
            esperado.Id = Guid.Parse("3CC1C7CD-5522-4AD3-A5EC-515DAECE6AA5");
            esperado.Nombre = "TestAudio";
            esperado.AnoCreacion = 1992;
            esperado.Interprete = "Guns N' Roses";
            esperado.Compositor = "Axl Rose";
            esperado.Productor = "Diego";
            esperado.GeneroId = 27;
            esperado.Url = "wikipedia.com";

            switch (caso)
            {
                case 0:
                    _pistaService.EditarPista(esperado).Wait();
                    Pista actual = _pistaService.ObtenerPista(esperado.Id).Result;
                    if (actual != null)
                    {
                        Assert.AreEqual(esperado.Id,actual.Id);
                        Assert.AreEqual(esperado.Nombre, actual.Nombre);
                        Assert.AreEqual(esperado.Url, actual.Url);
                        Assert.AreEqual(esperado.AnoCreacion, actual.AnoCreacion);
                        Assert.AreEqual(esperado.Interprete, actual.Interprete);
                        Assert.AreEqual(esperado.Compositor, actual.Compositor);
                        Assert.AreEqual(esperado.Productor, actual.Productor);
                        Assert.IsNotNull(actual.FechaRegistro);
                        Assert.AreEqual(esperado.GeneroId, actual.GeneroId);
                    }
                    break;
                case 1:
                    esperado.Nombre = null;
                    Assert.ThrowsException<AggregateException>(() =>
                    {
                        _pistaService.EditarPista(esperado).Wait();
                    });
                    break;
            }
        }

        [TestMethod]
        public void EliminarPistaTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _pistaService.EliminarPista(Guid.Empty).Wait();
            });
            
        }
        [TestMethod]
        public void ObtenerPistaTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                _pistaService.ObtenerPista(Guid.Empty).Wait();
            });

        }

        [DataRow(0)]
        [DataRow(1)]
        [TestMethod]
        public void ObtenerTodosPistasDeAlbumTest(int caso)
        {
            switch (caso)
            {
                case 0:
                    List<Pista> pistasList = _pistaService.ObtenerTodosPistasDeAlbum(Guid.Parse("E849DF3C-44E7-4D18-899E-FFB29BADC8A7")).Result;
                    Assert.IsNotNull(pistasList);
                    break;
                case 1:
                    Assert.ThrowsException<ArgumentNullException>(() =>
                    {
                        _pistaService.ObtenerTodosPistasDeAlbum(Guid.Empty).Wait();
                    });
                    break;
            }
        }

        [DataRow(0)]
        [DataRow(1)]
        [TestMethod]
        public void ObtenerTodosPistasDePlaylistTest(int caso)
        {
            switch (caso)
            {
                case 0:
                    List<Pista> pistasList = _pistaService.ObtenerTodosPistasDePlaylist(Guid.Parse("C8BE77D0-87A0-4758-9748-5B3A89EA3973")).Result;
                    Assert.IsNotNull(pistasList);
                    break;
                case 1:
                    Assert.ThrowsException<ArgumentNullException>(() =>
                    {
                        _pistaService.ObtenerTodosPistasDeAlbum(Guid.Empty).Wait();
                    });
                    break;
            }
        }
        [TestMethod]
        [DataRow("Test", 0)]
        [DataRow(null, 1)]
        public void SearchAudioTest(string cadena,int caso)
        {
            switch (caso)
            {
                case 0:
                    List<Pista> pistasList = _pistaService.BuscarPistas(cadena).Result;
                    Assert.IsNotNull(pistasList);
                    break;
                case 1:
                    Assert.ThrowsException<ArgumentNullException>(() =>
                    {
                        _pistaService.BuscarPistas(cadena).Wait();
                    });
                    break;
            }
        }
        
        [TestMethod]
        public void ReproducirPista()
        {
            Pista pista = _pistaService.ObtenerPista(Guid.Parse("3CC1C7CD-5522-4AD3-A5EC-515DAECE6AA5")).Result;
            int esperado = pista.Reproducciones + 1;
            _pistaService.ReproducirPista(pista).Wait();
            int actual = (_pistaService.ObtenerPista(Guid.Parse("3CC1C7CD-5522-4AD3-A5EC-515DAECE6AA5")).Result).Reproducciones;
            Assert.AreEqual(esperado, actual);
        }
    }

}
