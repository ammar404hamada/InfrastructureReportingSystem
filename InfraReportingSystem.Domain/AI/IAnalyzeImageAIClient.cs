namespace InfraReportingSystem.Domain.AI
{
    public interface IAnalyzeImageAIClient
    {
        Task<AnalyzeImageResult> AnalyzeImageAsync(Stream imageStream, string fileName);
    }
}
