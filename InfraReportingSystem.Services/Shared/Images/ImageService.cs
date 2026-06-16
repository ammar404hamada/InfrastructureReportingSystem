using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using InfraReportingSystem.ServiceAbstractions.Shared.Images;
using System;
using System.IO;
using System.Threading.Tasks;

namespace InfraReportingSystem.Services.Shared.Images
{
    public class ImageService : IImageService
    {
        private readonly ICloudinaryClient _cloudinaryClient;

        public ImageService(ICloudinaryClient cloudinaryClient)
        {
            _cloudinaryClient = cloudinaryClient;
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

            var uploadResult = await _cloudinaryClient.UploadImageAsync(uploadParams);

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
