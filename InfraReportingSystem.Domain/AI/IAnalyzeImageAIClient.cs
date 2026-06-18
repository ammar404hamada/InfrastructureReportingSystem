namespace InfraReportingSystem.Domain.AI
{
    public interface IAnalyzeImageAIClient
    {
        Task<AnalyzeImageResult> AnalyzeImagesAsync(IEnumerable<(Stream Stream, string FileName)> images);
    }
}
