using System.Net.Http.Headers;
using System.Text.Json;
using InfraReportingSystem.Domain.AI;

namespace InfraReportingSystem.Services.AI
{
    public class AnalyzeImageAIClient : IAnalyzeImageAIClient
    {
        private static readonly Dictionary<string, string> ExtensionMimeMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png", "image/png" },
            { ".webp", "image/webp" },
            { ".gif", "image/gif" },
            { ".heic", "image/heic" },
            { ".heif", "image/heif" },
        };

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
                var ext = Path.GetExtension(img.FileName);
                var mimeType = ExtensionMimeMap.TryGetValue(ext, out var mapped) ? mapped : "image/jpeg";
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
                content.Add(fileContent, "images", img.FileName);
            }

            var response = await _httpClient.PostAsync("/classify-image", content);
            if (!response.IsSuccessStatusCode)
            {
                if ((int)response.StatusCode == 503)
                    throw new InvalidOperationException("AI service is temporarily unavailable. Please try again later.");

                throw new InvalidOperationException($"AI service returned an error: {response.StatusCode}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

            if (!root.TryGetProperty("category", out var predictionElement))
                throw new InvalidOperationException(
                    $"AnalyzeImage AI unexpected response (missing 'category'): {responseBody}");

            if (!root.TryGetProperty("description", out var descriptionElement))
                throw new InvalidOperationException(
                    $"AnalyzeImage AI unexpected response (missing 'description'): {responseBody}");

            return new AnalyzeImageResult
            {
                Prediction = predictionElement.GetString()
                    ?? throw new InvalidOperationException("AnalyzeImage AI returned a null classification value."),
                ImageDescription = descriptionElement.GetString()
                    ?? throw new InvalidOperationException("AnalyzeImage AI returned a null description value.")
            };
        }
    }
}
