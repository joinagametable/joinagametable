using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Minio;

namespace JoinAGameTable.Services
{
    /// <summary>
    /// Manipulate files stored on the file storage.
    /// </summary>
    public class FileStorageService : IFileStorageService
    {
        /// <summary>
        /// Handle to MinIO client instance.
        /// </summary>
        private readonly MinioClient _minio;

        /// <summary>
        /// Build a new instance.
        /// </summary>
        public FileStorageService(IConfiguration configuration)
        {
            // Load right configuration section
            var configurationSection = configuration.GetSection("FileStorageService");

            // Configure MinIO client
            _minio = new MinioClient(
                configurationSection["Endpoint"],
                configurationSection["AccessKey"],
                configurationSection["SecretKey"]
            );

            // Name of the bucket to use
            BucketName = configurationSection["BucketName"];

            // Public URL
            EndpointPublicUrl = configurationSection["PublicUrl"].ToLowerInvariant();
            if (!EndpointPublicUrl.EndsWith("/"))
            {
                EndpointPublicUrl += "/";
            }

            if (!EndpointPublicUrl.StartsWith("http"))
            {
                if (EndpointPublicUrl.Contains("127.0.0.1") || EndpointPublicUrl.Contains("localhost"))
                {
                    EndpointPublicUrl = "http://" + EndpointPublicUrl;
                }
                else
                {
                    EndpointPublicUrl = "https://" + EndpointPublicUrl;
                }
            }
        }

        public string BucketName { get; }

        public string EndpointPublicUrl { get; }

        public string GenerateFilePublicUrl(string fileName) => EndpointPublicUrl + BucketName + "/" + fileName;

        public Task StoreFileAsync(string fileName, string contentType, long contentSize, Stream contentStream) =>
            _minio.PutObjectAsync(
                BucketName,
                fileName,
                contentStream,
                contentSize,
                contentType
            );

        public Task RetrieveFileAsync(string fileName, Action<Stream> callback) =>
            _minio.GetObjectAsync(
                BucketName,
                fileName,
                callback
            );

        public async Task<Stream> RetrieveFileAsync(string fileName)
        {
            Stream memoryStream = new MemoryStream();
            await _minio.GetObjectAsync(
                BucketName,
                fileName,
                (stream) => stream.CopyToAsync(memoryStream)
            );
            memoryStream.Position = 0;
            return memoryStream;
        }

        public Task DeleteFileAsync(string fileName) => _minio.RemoveObjectAsync(BucketName, fileName);

        public Task DeleteFileAsync(string directory, string fileName) => _minio.RemoveObjectAsync(
            BucketName,
            directory + "/" + fileName
        );
    }
}
