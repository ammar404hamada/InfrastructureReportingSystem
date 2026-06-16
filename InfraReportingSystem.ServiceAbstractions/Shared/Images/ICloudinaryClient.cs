using CloudinaryDotNet.Actions;

namespace InfraReportingSystem.ServiceAbstractions.Shared.Images
{
    public interface ICloudinaryClient
    {
        Task<ImageUploadResult> UploadImageAsync(ImageUploadParams uploadParams);
    }
}
