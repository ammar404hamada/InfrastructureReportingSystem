using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.AI;
using System.Net.Http.Headers;

namespace InfraReportingSystem.Services.AI
{
    public class CategoryAIClient : ICategoryAIClient
    {
        private readonly HttpClient _httpClient;

        public CategoryAIClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        public async Task<string> SuggestCategoryAsync(Stream imageStream, string fileName)
        {
            using var content = new MultipartFormDataContent();

            var fileContent = new StreamContent(imageStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            content.Add(fileContent, "file", fileName);

            var response = await _httpClient.PostAsync("/predict", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

            if (!root.TryGetProperty("category", out var categoryElement))
                throw new InvalidOperationException(
                    $"Category AI response did not contain 'category' field. Response: {responseBody}");

            return categoryElement.GetString()
                ?? throw new InvalidOperationException(
                    "Category AI returned a null category value.");
        }
    }
}
