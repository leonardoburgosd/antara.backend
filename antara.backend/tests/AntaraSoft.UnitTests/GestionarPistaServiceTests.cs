using AntaraSoft.Application.Contracts.Repository;
using AntaraSoft.Application.Services;
using AntaraSoft.Domain.Entities;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AntaraSoft.UnitTests
{
    public class GestionarPistaServiceTests
    {
        private readonly Mock<IPistaRepository> _pistaRepoMock;
        private readonly Mock<IAlbumRepository> _albumRepoMock;
        private readonly GestionarPistaService _service;

        public GestionarPistaServiceTests()
        {
            _pistaRepoMock = new Mock<IPistaRepository>();
            _albumRepoMock = new Mock<IAlbumRepository>();
            _service = new GestionarPistaService(_pistaRepoMock.Object, _albumRepoMock.Object);
        }

        [Fact]
        public async Task EliminarPista_ConGuidVacio_DebeLanzarExcepcion()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.EliminarPista(Guid.Empty));
        }

        [Fact]
        public async Task ObtenerPista_ConGuidVacio_DebeLanzarExcepcion()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ObtenerPista(Guid.Empty));
        }

        [Fact]
        public async Task BuscarPistas_ConCadenaNula_DebeLanzarExcepcion()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.BuscarPistas(null!));
        }

        [Fact]
        public async Task ReproducirPista_DebeIncrementarReproduccionYGuardar()
        {
            var pista = new Pista
            {
                Id = Guid.NewGuid(),
                Nombre = "Test Track",
                Reproducciones = 2
            };

            await _service.ReproducirPista(pista);

            Assert.Equal(3, pista.Reproducciones);
            _pistaRepoMock.Verify(r => r.ReproducirPista(pista.Id, 3), Times.Once);
        }
    }
}

