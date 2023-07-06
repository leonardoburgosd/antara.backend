using Antara.Model;
using Antara.Model.Entities;
using Antara.Repository.Repositories;
using Antara.Security;
using Antara.Service;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using BCryptNet = BCrypt.Net.BCrypt;

namespace AntaraTest
{
    [TestClass]
    public class RegistrarUsuarioServiceTest
    {
        public RegistrarUsuarioServiceTest()
        {

        }
        [TestMethod]
        [DataRow("testCreate@correo.com", 0)]
        [DataRow("test@correo.com", 1)]
        [DataRow(null, 1)]
        [DataRow("testCreate@correo.com", 2)]
        public void CrearUsuarioTest(string email, int caso)
        {
            Usuario esperado = new()
            {
                Id = Guid.NewGuid(),
                Email = email,
                Password = "test123",
                Nombre = "Test",
                FechaNacimiento = new(1999, 12, 31),
                Genero = 'M',
                EstaActivo = true,
                FechaRegistro = DateTime.Now,
                Pais = "Peru",
                Tipo = "antara"
            };

            var options = Options.Create(new AppSettings());
            options.Value.ConexionString = "Server=.;Database=antaradb;Trusted_Connection=True;MultipleActiveResultSets=True";
            var dapper = new Antara.Repository.Dapper.Dapper(options);
            var usuarioRepo = new UsuarioRepository(dapper);
            var mockEncrypter = new Mock<IEncryptText>();
            mockEncrypter.Setup(x => x.GeneratePasswordHash(esperado.Password)).Returns(BCryptNet.HashPassword(esperado.Password));
            var servicio = new RegistrarUsuarioService(usuarioRepo,mockEncrypter.Object);

            switch (caso)
            {
                case 0:
                    servicio.CrearUsuario(esperado).Wait();
                    Usuario actual = servicio.ObtenerUsuario(esperado.Id).Result;
                    if (actual != null)
                    {
                        Assert.IsNotNull(actual.Id);
                        Assert.AreEqual(esperado.Email, actual.Email);
                        Assert.IsTrue(BCryptNet.Verify("test123", actual.Password));
                        Assert.AreEqual(esperado.Nombre, actual.Nombre);
                        Assert.AreEqual(esperado.FechaNacimiento, actual.FechaNacimiento);
                        Assert.AreEqual(esperado.Genero, actual.Genero);
                        Assert.IsNotNull(actual.FechaRegistro);
                        Assert.IsTrue(actual.EstaActivo);
                        Assert.AreEqual(esperado.Pais, actual.Pais);

                        usuarioRepo.EliminarFisicoUsuario(actual.Id).Wait();
                    }
                    break;

                case 1:
                    Assert.ThrowsException<AggregateException>(() =>
                    {
                        servicio.CrearUsuario(esperado).Wait();
                    });
                    break;
                case 2:
                    esperado.Nombre = null;
                    Assert.ThrowsException<AggregateException>(() =>
                    {
                        servicio.CrearUsuario(esperado).Wait();
                    });
                    break;
            }   
        }

        [TestMethod]
        public void ObtenerUsuarioTests()
        {
            var options = Options.Create(new AppSettings());
            options.Value.ConexionString = "Server=.;Database=antaradb;Trusted_Connection=True;MultipleActiveResultSets=True";
            var dapper = new Antara.Repository.Dapper.Dapper(options);
            var usuarioRepo = new UsuarioRepository(dapper);
            var mockEncrypter = new Mock<IEncryptText>();
            var servicio = new RegistrarUsuarioService(usuarioRepo, mockEncrypter.Object);

            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                var actual = servicio.ObtenerUsuario(Guid.Empty).Result;
            });
            
        }

        [TestMethod]
        public void PhysicalDeleteUsuarioTest()
        {
            var options = Options.Create(new AppSettings());
            options.Value.ConexionString = "Server=.;Database=antaradb;Trusted_Connection=True;MultipleActiveResultSets=True";
            var dapper = new Antara.Repository.Dapper.Dapper(options);
            var usuarioRepo = new UsuarioRepository(dapper);

            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                usuarioRepo.EliminarFisicoUsuario(Guid.Empty).Wait();
            });
        }
    }
}
