using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using InfraReportingSystem.ServiceAbstractions.Shared.Images;
using InfraReportingSystem.Shared.Settings;
using Microsoft.Extensions.Options;

namespace InfraReportingSystem.Services.Shared.Images
{
    public class CloudinaryClient : ICloudinaryClient
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryClient(IOptions<CloudinarySettings> config)
        {
            var account = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(account);
        }

        public async Task<ImageUploadResult> UploadImageAsync(ImageUploadParams uploadParams)
        {
            return await _cloudinary.UploadAsync(uploadParams);
        }
    }
}
