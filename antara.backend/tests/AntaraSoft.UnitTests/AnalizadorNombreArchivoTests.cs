using AntaraSoft.Infrastructure.Metadata;
using Xunit;

namespace AntaraSoft.UnitTests
{
    public class AnalizadorNombreArchivoTests
    {
        [Theory]
        [InlineData("C:/Music/Sia - Elastic Heart feat. Shia LaBeouf & Maddie Ziegler (Official Video).mp3", "Sia", "Elastic Heart")]
        [InlineData("01 - Daft Punk - One More Time.mp3", "Daft Punk", "One More Time")]
        [InlineData("Daft Punk - Get Lucky.mp3", "Daft Punk", "Get Lucky")]
        [InlineData("Cancion.mp3", null, "Cancion")]
        public void AnalizarNombre_DebeExtraerArtistaYTituloCorrectamente(string ruta, string artistaEsperado, string tituloEsperado)
        {
            var (artista, titulo) = AnalizadorNombreArchivo.Analizar(ruta);

            Assert.Equal(artistaEsperado, artista);
            Assert.Equal(tituloEsperado, titulo);
        }
    }
}

