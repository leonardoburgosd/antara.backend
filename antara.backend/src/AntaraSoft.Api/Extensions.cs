using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AntaraSoft.Api
{
    public abstract class Extensions : Controller
    {
        public static async Task<string> SubirArchivo(IFormFile archivo, string directorioId, string contentType = "image/jpeg")
        {
            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(archivo.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await archivo.CopyToAsync(fileStream);
                }

                return $"/uploads/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error subiendo el archivo: {ex.Message}");
                return null;
            }
        }
    }
}

