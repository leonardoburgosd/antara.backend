using AntaraSoft.Domain.Entities;
using System;
using Xunit;

namespace AntaraSoft.UnitTests
{
    public class DomainEntityTests
    {
        [Fact]
        public void Pista_AumentarReproduccion_DebeIncrementarContador()
        {
            var pista = new Pista
            {
                Id = Guid.NewGuid(),
                Nombre = "Test Song",
                Reproducciones = 5
            };

            pista.AumentarReproduccion();

            Assert.Equal(6, pista.Reproducciones);
        }

        [Theory]
        [InlineData(1990, true)]
        [InlineData(2020, true)]
        [InlineData(1899, false)]
        public void Pista_EsValidoAnoCreacion_DebeValidarRango(int ano, bool esperado)
        {
            var pista = new Pista
            {
                Id = Guid.NewGuid(),
                Nombre = "Test",
                AnoCreacion = ano
            };

            Assert.Equal(esperado, pista.EsValidoAnoCreacion());
        }
    }
}

