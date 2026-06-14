using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.AI;
<<<<<<< HEAD
using System.Net.Http.Headers;
=======

>>>>>>> 63ae425cacb8f085d875228d89a74a932d83f2df
namespace InfraReportingSystem.Services.AI
{
    public class CategoryAIClient : ICategoryAIClient
    {
        private readonly HttpClient _httpClient;

        public CategoryAIClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

<<<<<<< HEAD
        public async Task<string> SuggestCategoryAsync(Stream imageStream, string fileName)
=======
        public async Task<string> SuggestCategoryAsync(Stream imageStream, string fileName, string contentType)
>>>>>>> 63ae425cacb8f085d875228d89a74a932d83f2df
        {
            using var content = new MultipartFormDataContent();

            var fileContent = new StreamContent(imageStream);
<<<<<<< HEAD
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
=======
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
>>>>>>> 63ae425cacb8f085d875228d89a74a932d83f2df
            content.Add(fileContent, "file", fileName);

            var response = await _httpClient.PostAsync("/predict", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
<<<<<<< HEAD

=======
>>>>>>> 63ae425cacb8f085d875228d89a74a932d83f2df
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

            if (!root.TryGetProperty("category", out var categoryElement))
                throw new InvalidOperationException(
                    $"Category AI response did not contain 'category' field. Response: {responseBody}");

            return categoryElement.GetString()
<<<<<<< HEAD
                ?? throw new InvalidOperationException(
                    "Category AI returned a null category value.");
=======
                ?? throw new InvalidOperationException("Category AI returned a null category value.");
>>>>>>> 63ae425cacb8f085d875228d89a74a932d83f2df
        }
    }
}
