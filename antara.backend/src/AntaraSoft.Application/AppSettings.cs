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
        public string DeezerApiBase { get; set; } = "https://api.deezer.com";
        public string TheAudioDbApiBase { get; set; } = "https://www.theaudiodb.com/api/v1/json/123";
        public string TheAudioDbApiKey { get; set; } = "123";
        public string LetrasApiBase { get; set; } = "https://api.lyrics.ovh";
        public int MetadatosTimeoutSegundos { get; set; } = 20;
        public string HostMountRoot { get; set; } = "/host";
    }

    public class AudioStorageLocation
    {
        public string Name { get; set; }
        public string PhysicalPath { get; set; }
        public string ContainerPath { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

