using System.Net.Http.Headers;
using System.Text.Json;
using InfraReportingSystem.Domain.AI;

namespace InfraReportingSystem.Services.AI
{
    public class AnalyzeImageAIClient : IAnalyzeImageAIClient
    {
        private readonly HttpClient _httpClient;

        public AnalyzeImageAIClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AnalyzeImageResult> AnalyzeImagesAsync(IEnumerable<(Stream Stream, string FileName)> images)
        {
            using var content = new MultipartFormDataContent();
            foreach (var img in images)
            {
                var fileContent = new StreamContent(img.Stream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                content.Add(fileContent, "files", img.FileName);
            }

            var response = await _httpClient.PostAsync("/classify-image", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

            if (!root.TryGetProperty("classification", out var predictionElement))
                throw new InvalidOperationException(
                    $"AnalyzeImage AI unexpected response (missing 'prediction'): {responseBody}");

            if (!root.TryGetProperty("description", out var descriptionElement))
                throw new InvalidOperationException(
                    $"AnalyzeImage AI unexpected response (missing 'image_description'): {responseBody}");

            return new AnalyzeImageResult
            {
                Prediction = predictionElement.GetString()
                    ?? throw new InvalidOperationException("AnalyzeImage AI returned a null prediction value."),
                ImageDescription = descriptionElement.GetString()
                    ?? throw new InvalidOperationException("AnalyzeImage AI returned a null image_description value.")
            };
        }
    }
}
