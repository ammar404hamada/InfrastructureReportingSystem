using System.IO;
using System.Threading.Tasks;

namespace InfraReportingSystem.ServiceAbstractions.Shared.Images
{
    public interface IImageService
    {
        Task<string?> UploadImageAsync(Stream fileStream, string fileName, string folderPath);
    }
}
