using AntaraSoft.Application.Contracts.Services;
using AntaraSoft.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AntaraSoft.Application.Services
{
    /// <summary>
    /// Busca coincidencias en los proveedores de metadatos configurados y aplica la elegida
    /// a una pista existente, actualizando titulo, artista, album, portada, año,
    /// genero, duracion y letra.
    /// </summary>
    public class MetadatosPistaService : IMetadatosPistaService
    {
        private readonly List<IMetadataProvider> _providers;
        private readonly ILetrasProvider _letrasProvider;
        private readonly Contracts.Repository.IBibliotecaRepository _bibliotecaRepository;

        public MetadatosPistaService(
            IEnumerable<IMetadataProvider> metadataProviders,
            ILetrasProvider letrasProvider,
            Contracts.Repository.IBibliotecaRepository bibliotecaRepository)
        {
            _providers = metadataProviders?.ToList() ?? new List<IMetadataProvider>();
            _letrasProvider = letrasProvider;
            _bibliotecaRepository = bibliotecaRepository;
        }

        public List<ProveedorMetadatosDto> ObtenerProveedores() =>
            _providers
                .Select(prov => new ProveedorMetadatosDto
                {
                    Nombre = prov.Nombre,
                    Etiqueta = string.IsNullOrWhiteSpace(prov.Etiqueta) ? prov.Nombre : prov.Etiqueta
                })
                .ToList();

        public Task<List<MetadatosCandidatoDto>> BuscarAsync(string consulta, CancellationToken cancellationToken) =>
            BuscarAsync(consulta, null, cancellationToken);

        public async Task<List<MetadatosCandidatoDto>> BuscarAsync(string consulta, string proveedor, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(consulta))
                throw new ArgumentException("Ingresa un termino de busqueda.", nameof(consulta));

            var listaProveedores = _providers;
            if (!string.IsNullOrWhiteSpace(proveedor) && !string.Equals(proveedor, "todos", StringComparison.OrdinalIgnoreCase))
            {
                var nombre = proveedor.Trim();
                var prov = _providers.FirstOrDefault(p => string.Equals(p.Nombre, nombre, StringComparison.OrdinalIgnoreCase))
                    ?? _providers.FirstOrDefault(p => string.Equals(p.Etiqueta, nombre, StringComparison.OrdinalIgnoreCase));

                // No se cae a "todos": un proveedor deshabilitado o mal escrito en la
                // configuracion debe ser visible, no devolver resultados de otro.
                if (prov == null)
                    throw new ArgumentException(ProveedoresDisponibles(nombre));

                listaProveedores = new List<IMetadataProvider> { prov };
            }

            if (listaProveedores.Count == 0)
                throw new ArgumentException(
                    "No hay proveedores de metadatos habilitados. Revisa AppSettings:MetadatosProveedores.");

            var tareas = listaProveedores.Select(p => p.BuscarCandidatosAsync(consulta.Trim(), cancellationToken));
            var resultados = await Task.WhenAll(tareas);

            var todos = new List<MetadatosAudio>();
            foreach (var conjunto in resultados)
                if (conjunto != null)
                    todos.AddRange(conjunto);

            return todos
                .Where(c => c != null && c.Encontrado)
                .Select(c => new MetadatosCandidatoDto
                {
                    IdExterno = c.IdExterno,
                    Titulo = c.Titulo,
                    Artista = string.Join(", ", c.Artistas ?? new List<string>()),
                    Album = c.Album,
                    PortadaUrl = c.PortadaUrl,
                    ImagenArtista = c.ImagenArtistaUrl,
                    Ano = c.Ano,
                    DuracionSegundos = c.DuracionSegundos,
                    Fuente = c.Fuente
                })
                .ToList();
        }

        public Task<PistaBibliotecaDto> ActualizarAsync(Guid pistaId, string idExterno, Guid usuarioId, CancellationToken cancellationToken) =>
            ActualizarAsync(pistaId, idExterno, null, usuarioId, cancellationToken);

        public async Task<PistaBibliotecaDto> ActualizarAsync(Guid pistaId, string idExterno, string proveedor, Guid usuarioId, CancellationToken cancellationToken)
        {
            if (pistaId == Guid.Empty)
                throw new ArgumentException("La pista no es valida.", nameof(pistaId));
            if (string.IsNullOrWhiteSpace(idExterno))
                throw new ArgumentException("Selecciona una coincidencia de metadatos.", nameof(idExterno));
            if (usuarioId == Guid.Empty)
                throw new ArgumentException("No se pudo identificar al usuario que actualiza los metadatos.");

            var actual = await _bibliotecaRepository.ObtenerPistaBiblioteca(pistaId);
            if (actual == null)
                throw new KeyNotFoundException("La pista no existe.");

            var pista = new Pista
            {
                Id = actual.Id,
                Nombre = actual.Nombre,
                Interprete = actual.Interprete,
                AnoCreacion = actual.AnoCreacion,
                GeneroId = actual.GeneroId,
                DuracionSegundos = actual.DuracionSegundos,
                AlbumId = actual.AlbumId,
                Url = actual.Url,
                ImagenArtista = actual.ImagenArtista,
                FuenteMetadatos = actual.FuenteMetadatos,
                Letra = actual.Letra,
                EstaActivo = actual.EstaActivo
            };

            MetadatosAudio metadatos = null;

            if (!string.IsNullOrWhiteSpace(proveedor))
            {
                var prov = _providers.FirstOrDefault(p => string.Equals(p.Nombre, proveedor.Trim(), StringComparison.OrdinalIgnoreCase));
                if (prov != null)
                    metadatos = await prov.ObtenerPorIdAsync(idExterno.Trim(), cancellationToken);
            }

            if (metadatos == null || !metadatos.Encontrado)
            {
                foreach (var prov in _providers)
                {
                    var obtenido = await prov.ObtenerPorIdAsync(idExterno.Trim(), cancellationToken);
                    if (obtenido != null && obtenido.Encontrado)
                    {
                        metadatos = obtenido;
                        break;
                    }
                }
            }

            if (metadatos == null || !metadatos.Encontrado)
                throw new InvalidOperationException("No se encontraron metadatos para la coincidencia seleccionada.");

            if (_letrasProvider != null &&
                !string.IsNullOrWhiteSpace(metadatos.ArtistaPrincipal) &&
                !string.IsNullOrWhiteSpace(metadatos.Titulo))
            {
                try
                {
                    metadatos.Letra = await _letrasProvider.ObtenerAsync(metadatos.ArtistaPrincipal, metadatos.Titulo, cancellationToken);
                }
                catch (OperationCanceledException) { throw; }
                catch { /* La letra es opcional */ }
            }

            var interprete = Truncar(
                metadatos.Artistas != null && metadatos.Artistas.Count > 0
                    ? string.Join(", ", metadatos.Artistas)
                    : (string.IsNullOrWhiteSpace(metadatos.ArtistaPrincipal) ? pista.Interprete : metadatos.ArtistaPrincipal),
                255);

            var albumNombre = Truncar(!string.IsNullOrWhiteSpace(metadatos.Album) ? metadatos.Album : "Sin album", 255);

            var albumId = await _bibliotecaRepository.ObtenerOCrearAlbum(new Album
            {
                Id = Guid.NewGuid(),
                Nombre = albumNombre,
                Descripcion = $"Album de {metadatos.ArtistaPrincipal ?? interprete}",
                EstaPublicado = false,
                FechaPublicacion = metadatos.FechaPublicacion ?? default,
                PortadaUrl = metadatos.PortadaUrl,
                UsuarioId = usuarioId,
                EstaActivo = true,
                Artista = Truncar(metadatos.ArtistaPrincipal ?? interprete, 255),
                FuenteMetadatos = Truncar(metadatos.Fuente, 50),
                IdExternoMetadatos = Truncar(metadatos.IdAlbumExterno, 100)
            });

            var generoId = 0;
            if (!string.IsNullOrWhiteSpace(metadatos.Genero))
                generoId = await _bibliotecaRepository.ObtenerOCrearGenero(Truncar(metadatos.Genero, 100));

            pista.Nombre = Truncar(string.IsNullOrWhiteSpace(metadatos.Titulo) ? pista.Nombre : metadatos.Titulo, 255);
            pista.Interprete = interprete;
            pista.AnoCreacion = metadatos.Ano > 0 ? metadatos.Ano : pista.AnoCreacion;
            if (generoId > 0)
                pista.GeneroId = generoId;
            pista.DuracionSegundos = metadatos.DuracionSegundos > 0 ? metadatos.DuracionSegundos : pista.DuracionSegundos;
            pista.ImagenArtista = metadatos.ImagenArtistaUrl;
            pista.FuenteMetadatos = Truncar(metadatos.Fuente, 50);
            pista.IdExternoMetadatos = Truncar(metadatos.IdExterno, 100);
            pista.MetadatosJson = metadatos.Json;
            pista.AlbumId = albumId;
            pista.Letra = metadatos.Letra;

            await _bibliotecaRepository.ActualizarMetadatosPista(pista);
            return await _bibliotecaRepository.ObtenerPistaBiblioteca(pistaId);
        }

        private string ProveedoresDisponibles(string solicitado)
        {
            if (_providers.Count == 0)
                return "No hay proveedores de metadatos habilitados. Revisa AppSettings:MetadatosProveedores.";

            return $"El proveedor '{solicitado}' no esta habilitado. Disponibles: {string.Join(", ", _providers.Select(p => p.Nombre))}.";
        }

        private static string Truncar(string valor, int maximo)
        {
            if (string.IsNullOrEmpty(valor)) return valor;
            return valor.Length <= maximo ? valor : valor.Substring(0, maximo);
        }
    }
}

