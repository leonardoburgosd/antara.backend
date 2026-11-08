// Settings model shared across the application
using System.Collections.Generic;

namespace AntaraSoft.Application
{
    public class AppSettings
    {
        public string ConexionString { get; set; }
        public List<AudioStorageLocation> AudioStorageLocations { get; set; } = new();
        public string DefaultAudioStoragePath { get; set; }
        public string TokenSecret { get; set; }
        public int TokenExpiracionHoras { get; set; } = 8;
        public int EscaneoAudioMaxParalelo { get; set; } = 4;
        public int MetadatosTimeoutSegundos { get; set; } = 20;
        public string HostMountRoot { get; set; } = "/host";

        /// <summary>Ejecutable de yt-dlp usado para descargar audio de YouTube.</summary>
        public string YtDlpPath { get; set; } = "yt-dlp";

        /// <summary>Tiempo máximo para descargar y convertir una canción, en segundos.</summary>
        public int DescargaYouTubeTimeoutSegundos { get; set; } = 600;

        /// <summary>
        /// Cliente interno de YouTube que usa yt-dlp (por ejemplo "android", "tv").
        /// El cliente "android" evita el bloqueo "Sign in to confirm you're not a bot"
        /// en muchas redes. Vacío = usa el valor predeterminado de yt-dlp.
        /// </summary>
        public string YtDlpPlayerClient { get; set; } = "android";

        /// <summary>
        /// Archivo de cookies de YouTube (formato Netscape) para redes donde yt-dlp
        /// exige sesión. Si está vacío o el archivo no existe, se descarga sin cookies.
        /// </summary>
        public string YtDlpCookiesPath { get; set; }

        /// <summary>
        /// Proveedores de metadatos activos. Se definen por configuración
        /// (AppSettings:MetadatosProveedores) y se registran en el contenedor de
        /// dependencias en el orden de <see cref="ProveedorMetadatosConfig.Prioridad"/>.
        /// Agregar, deshabilitar o reordenar proveedores no requiere tocar código.
        /// </summary>
        /// <remarks>
        /// Se inicializa vacía a propósito: el binder de configuración agrega
        /// elementos a la lista existente, así que precargarla duplicaría los
        /// proveedores definidos en appsettings o en variables de entorno.
        /// Cuando queda vacía, se usa <see cref="Predeterminados"/>.
        /// </remarks>
        public List<ProveedorMetadatosConfig> MetadatosProveedores { get; set; } = new();

        /// <summary>
        /// Proveedor de letras. Es opcional: si está deshabilitado, las pistas se
        /// guardan sin letra.
        /// </summary>
        public ProveedorLetrasConfig LetrasProveedor { get; set; } = new();

        /// <summary>
        /// Proveedores que se usan cuando la configuración no define ninguno.
        /// </summary>
        /// <remarks>Es un método (y no un valor por defecto de la propiedad)
        /// para no mezclarse con lo que agregue el binder de configuración.</remarks>
        public static List<ProveedorMetadatosConfig> Predeterminados() => new()
        {
            new ProveedorMetadatosConfig
            {
                Nombre = "deezer",
                Etiqueta = "Deezer",
                Tipo = "deezer",
                BaseUrl = "https://api.deezer.com",
                Prioridad = 0
            },
            new ProveedorMetadatosConfig
            {
                Nombre = "theaudiodb",
                Etiqueta = "TheAudioDB",
                Tipo = "theaudiodb",
                BaseUrl = "https://www.theaudiodb.com/api/v1/json/123",
                ApiKey = "123",
                Prioridad = 1
            },
            new ProveedorMetadatosConfig
            {
                Nombre = "musicbrainz",
                Etiqueta = "MusicBrainz",
                Tipo = "musicbrainz",
                BaseUrl = "https://musicbrainz.org",
                Prioridad = 2
            },
            new ProveedorMetadatosConfig
            {
                Nombre = "itunes",
                Etiqueta = "iTunes",
                Tipo = "itunes",
                BaseUrl = "https://itunes.apple.com",
                Prioridad = 3
            }
        };
    }

    /// <summary>
    /// Configuración de un proveedor de metadatos. <see cref="Tipo"/> selecciona
    /// el adaptador del API (el formato de respuesta) y el resto de campos ajusta
    /// la instancia concreta.
    /// </summary>
    public class ProveedorMetadatosConfig
    {
        /// <summary>Identificador usado por la API y el panel (por ejemplo "deezer").</summary>
        public string Nombre { get; set; }

        /// <summary>Nombre visible en el panel; si falta, se usa <see cref="Nombre"/>.</summary>
        public string Etiqueta { get; set; }

        /// <summary>Adaptador a usar: "deezer" o "theaudiodb".</summary>
        public string Tipo { get; set; }

        /// <summary>URL base del API del proveedor.</summary>
        public string BaseUrl { get; set; }

        /// <summary>Clave del API, si el proveedor la requiere.</summary>
        public string ApiKey { get; set; }

        /// <summary>Orden de consulta: menor prioridad se consulta primero.</summary>
        public int Prioridad { get; set; }

        /// <summary>Si es false, el proveedor no se carga.</summary>
        public bool Habilitado { get; set; } = true;
    }

    /// <summary>
    /// Configuración del proveedor de letras. Actualmente hay un adaptador
    /// ("lyricsovh"); la URL es configurable para usar un espejo.
    /// </summary>
    public class ProveedorLetrasConfig
    {
        public string Tipo { get; set; } = "lyricsovh";
        public string BaseUrl { get; set; } = "https://api.lyrics.ovh";
        public bool Habilitado { get; set; } = true;
    }

    public class AudioStorageLocation
    {
        public string Name { get; set; }
        public string PhysicalPath { get; set; }
        public string ContainerPath { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
