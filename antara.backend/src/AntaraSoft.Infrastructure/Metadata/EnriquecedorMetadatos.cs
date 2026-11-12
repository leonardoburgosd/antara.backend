using AntaraSoft.Application.Contracts.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AntaraSoft.Infrastructure.Metadata
{
    /// <summary>
    /// Combina los metadatos musicales (consultando proveedores como Deezer y TheAudioDB en orden)
    /// con la letra de la cancion. Cada fuente es opcional: si la letra falla, la pista se registra igualmente.
    /// </summary>
    public class EnriquecedorMetadatos
    {
        private readonly List<IMetadataProvider> _metadataProviders;
        private readonly ILetrasProvider _letrasProvider;

        public EnriquecedorMetadatos(IEnumerable<IMetadataProvider> metadataProviders, ILetrasProvider letrasProvider)
        {
            _metadataProviders = metadataProviders?.ToList() ?? new List<IMetadataProvider>();
            _letrasProvider = letrasProvider;
        }

        public async Task<MetadatosAudio> ObtenerAsync(
            string artista,
            string titulo,
            string carpeta,
            CancellationToken cancellationToken)
        {
            MetadatosAudio metadatos = null;

            foreach (var provider in _metadataProviders)
            {
                try
                {
                    var obtenido = await provider.BuscarAsync(artista, titulo, carpeta, cancellationToken);
                    if (obtenido != null && obtenido.Encontrado)
                    {
                        metadatos = obtenido;
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch
                {
                    // Si un proveedor falla por red o formato, continuar con el siguiente
                }
            }

            if (metadatos == null)
            {
                metadatos = new MetadatosAudio
                {
                    Encontrado = false,
                    Fuente = _metadataProviders.FirstOrDefault()?.Nombre ?? "desconocido"
                };
            }

            if (_letrasProvider == null)
                return metadatos;

            var artistaParaLetra = metadatos.ArtistaPrincipal ?? artista;
            var tituloParaLetra = string.IsNullOrWhiteSpace(metadatos.Titulo) ? titulo : metadatos.Titulo;

            if (string.IsNullOrWhiteSpace(artistaParaLetra) || string.IsNullOrWhiteSpace(tituloParaLetra))
                return metadatos;

            try
            {
                // No borrar una letra que ya trajo el proveedor de metadatos
                // (LRCLIB la incluye) cuando lyrics.ovh no encuentra nada.
                var letra = await _letrasProvider.ObtenerAsync(artistaParaLetra, tituloParaLetra, cancellationToken);
                if (!string.IsNullOrWhiteSpace(letra))
                    metadatos.Letra = letra;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                // La letra es informacion opcional.
            }

            return metadatos;
        }
    }
}

