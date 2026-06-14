using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
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


        public async Task<string> GenerateDescriptionAsync(Stream imageStream, string fileName)
        {
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(imageStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            content.Add(fileContent, "file", fileName);

            var response = await _httpClient.PostAsync("/analyze", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

            if (!root.TryGetProperty("image_description", out var descriptionElement))
                throw new InvalidOperationException(
                    $"Description AI unexpected response: {responseBody}");

            return descriptionElement.GetString()
                ?? throw new InvalidOperationException(
                    "Description AI returned a null description value.");
        }
    }
}
