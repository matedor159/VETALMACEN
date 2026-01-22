// File: Services/BlobStorageService.cs
using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace SisAlmacenProductos.Services
{
    public class BlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public BlobStorageService(IConfiguration configuration)
        {
            // Lee la connection string y el container desde appsettings o variables de entorno
            var conn = configuration["AzureStorage:ConnectionString"]
                       ?? Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            _containerName = configuration["AzureStorage:ContainerName"]
                             ?? Environment.GetEnvironmentVariable("AZURE_STORAGE_CONTAINER")
                             ?? "imagenes";

            if (string.IsNullOrWhiteSpace(conn))
                throw new ArgumentException("Azure Storage connection string is not configured.");

            _blobServiceClient = new BlobServiceClient(conn);
        }

        /// <summary>
        /// Sube el archivo al contenedor y retorna la URL pública del blob.
        /// </summary>
        public async Task<string> UploadFileAsync(IFormFile file, string folder = "productos")
        {
            if (file == null || file.Length == 0) return null;

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = GenerateBlobName(folder, ext);

            var blobClient = containerClient.GetBlobClient(fileName);

            var httpHeaders = new BlobHttpHeaders
            {
                ContentType = file.ContentType
            };

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, httpHeaders);
            }

            return blobClient.Uri.AbsoluteUri;
        }

        /// <summary>
        /// Elimina un blob a partir de su URL si existe.
        /// </summary>
        public async Task DeleteFileIfExistsAsync(string blobUrl)
        {
            if (string.IsNullOrWhiteSpace(blobUrl)) return;

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

            // Extraer nombre del blob desde la URL (el path contiene /container/blobname)
            var uri = new Uri(blobUrl);
            var absolutePath = uri.AbsolutePath.TrimStart('/'); // container/blobname
            string blobName;

            if (absolutePath.StartsWith(_containerName + "/"))
                blobName = absolutePath.Substring(_containerName.Length + 1);
            else
                blobName = absolutePath; // fallback

            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }

        private string GenerateBlobName(string folder, string extension)
        {
            if (!string.IsNullOrEmpty(folder))
            {
                // Sanitize folder (no leading/trailing slashes)
                folder = folder.Trim().Trim('/');
            }
            else
            {
                folder = "productos";
            }

            return $"{folder}/{DateTime.UtcNow:yyyy}/{DateTime.UtcNow:MM}/{Guid.NewGuid()}{extension}";
        }
    }
}
