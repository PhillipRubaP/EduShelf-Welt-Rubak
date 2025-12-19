using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace EduShelf.Api.Services.FileStorage
{
    public class MinioStorageService : IFileStorageService
    {
        private readonly IMinioClient _minioClient;
        private readonly string _bucketName;

        public MinioStorageService(IConfiguration configuration)
        {
            var endpoint = configuration["Minio:Endpoint"];
            var accessKey = configuration["Minio:AccessKey"];
            var secretKey = configuration["Minio:SecretKey"];
            _bucketName = configuration["Minio:BucketName"] ?? "edushelf-documents";
            var secure = configuration.GetValue<bool>("Minio:Secure", false);

            _minioClient = new MinioClient()
                                .WithEndpoint(endpoint)
                                .WithCredentials(accessKey, secretKey)
                                .WithSSL(secure)
                                .Build();
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            try
            {
                bool found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucketName));
                if (!found)
                {
                    await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucketName));
                }

                // PutObjectArgs expects stream length or -1 if unknown? 
                // Minio .NET client usually prefers known length if possible.
                // Assuming fileStream is at position 0 or we reset it.
                if (fileStream.CanSeek)
                {
                    fileStream.Position = 0;
                }
                
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(fileName)
                    .WithStreamData(fileStream)
                    .WithObjectSize(fileStream.Length)
                    .WithContentType(contentType);

                await _minioClient.PutObjectAsync(putObjectArgs);
                
                return fileName; // Return object key
            }
            catch (MinioException e)
            {
                throw new Exception($"File upload failed: {e.Message}", e);
            }
        }

        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            try
            {
                var memoryStream = new MemoryStream();
                
                var getObjectArgs = new GetObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(fileName)
                    .WithCallbackStream((stream) => {
                         stream.CopyTo(memoryStream);
                    });

                await _minioClient.GetObjectAsync(getObjectArgs);
                
                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (MinioException e)
            {
                 throw new Exception($"File download failed: {e.Message}", e);
            }
        }

        public async Task DeleteFileAsync(string fileName)
        {
            try
            {
                var removeObjectArgs = new RemoveObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(fileName);

                await _minioClient.RemoveObjectAsync(removeObjectArgs);
            }
            catch (MinioException e)
            {
                throw new Exception($"File deletion failed: {e.Message}", e);
            }
        }

        public async Task<bool> FileExistsAsync(string fileName)
        {
            try
            {
                var statObjectArgs = new StatObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(fileName);

                await _minioClient.StatObjectAsync(statObjectArgs);
                return true;
            }
            catch (MinioException)
            {
                return false;
            }
        }

        public string GetFileUrl(string fileName)
        {
            // If Minio is public or we use presigned URLs, returning a direct URL is possible.
            // For now, API acts as proxy via Download, so this might not be needed or returns internal ID.
            return fileName; 
        }
    }
}
