using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Antara.API
{
    public abstract class Extensions : Controller
    {
        public static async Task<string> SubirArchivo(IFormFile archivo, string directorioId, string contentType = "image/jpeg")
        {
            //Obtener el path del archivo de credenciales
            string credencialesDirectorio = Path.Combine(Directory.GetCurrentDirectory(), @"credentials.json");

            //Cargar las credenciales de la cuenta de servicio y definir el alcance
            var credential = GoogleCredential.FromFile(credencialesDirectorio)
                .CreateScoped(DriveService.ScopeConstants.Drive);
            //Crear un servicio drive
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential
            });
            //Subir metadata del archivo
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = archivo.FileName,
                Parents = new List<String>() { directorioId }
            };
            string fileUrl;
            //Crear nuevo archivo en Google Drive
            await using (var fsSource = new MemoryStream())
            {
                //Crear un nuevo archivo, con metadata y stream.
                await archivo.CopyToAsync(fsSource);
                var request = service.Files.Create(fileMetadata, fsSource, contentType);
                request.Fields = "*";
                var results = await request.UploadAsync(CancellationToken.None);
                if (results.Status == UploadStatus.Failed)
                {
                    Console.WriteLine($"Error subiendo el archivo: {results.Exception.Message}");
                }
                fileUrl = request.ResponseBody?.WebContentLink;
            }
            return fileUrl;
        }
    }
}
