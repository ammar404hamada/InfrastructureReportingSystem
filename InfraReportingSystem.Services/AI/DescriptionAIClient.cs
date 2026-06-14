using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.AI;

namespace InfraReportingSystem.Services.AI
{
    public class DescriptionAIClient : IDescriptionAIClient
    {
        private readonly HttpClient _httpClient;

        public DescriptionAIClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GenerateDescriptionAsync(Stream imageStream, string fileName, string contentType)
        {
            using var content = new MultipartFormDataContent();

            var fileContent = new StreamContent(imageStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(fileContent, "file", fileName);

            var response = await _httpClient.PostAsync("/analyze", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

            if (!root.TryGetProperty("description", out var descriptionElement))
                throw new InvalidOperationException(
                    $"Description AI response did not contain 'description' field. Response: {responseBody}");

            return descriptionElement.GetString()
                ?? throw new InvalidOperationException("Description AI returned a null description value.");
        }
    }
}
