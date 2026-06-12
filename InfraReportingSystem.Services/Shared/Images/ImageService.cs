using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using InfraReportingSystem.ServiceAbstractions.Shared.Images;
using InfraReportingSystem.Shared.Settings;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace InfraReportingSystem.Services.Shared.Images
{
    public class ImageService : IImageService
    {
        private readonly Cloudinary _cloudinary;

        public ImageService(IOptions<CloudinarySettings> config)
        {
            var account = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string?> UploadImageAsync(Stream fileStream, string fileName, string folderPath)
        {
            if (fileStream == null || fileStream.Length == 0)
                return null;

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(fileName, fileStream),
                Folder = folderPath,
                Transformation = new Transformation()
                    .Width(400)
                    .Height(400)
                    .Crop("fill")
                    .Quality("auto"),
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new Exception($"Cloudinary error: {uploadResult.Error.Message}");
            }

            if (uploadResult.SecureUrl == null)
            {
                return null;
            }

            return uploadResult.SecureUrl.ToString();
        }
    }
}