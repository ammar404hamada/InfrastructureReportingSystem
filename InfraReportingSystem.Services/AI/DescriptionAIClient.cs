using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
<<<<<<< HEAD
using System.Net.Http.Headers;
=======
>>>>>>> 63ae425cacb8f085d875228d89a74a932d83f2df
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

<<<<<<< HEAD
        public async Task<string> GenerateDescriptionAsync(Stream imageStream, string fileName)
=======
        public async Task<string> GenerateDescriptionAsync(Stream imageStream, string fileName, string contentType)
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

            var response = await _httpClient.PostAsync("/analyze", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
<<<<<<< HEAD

=======
>>>>>>> 63ae425cacb8f085d875228d89a74a932d83f2df
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

            if (!root.TryGetProperty("description", out var descriptionElement))
                throw new InvalidOperationException(
                    $"Description AI response did not contain 'description' field. Response: {responseBody}");

            return descriptionElement.GetString()
<<<<<<< HEAD
                ?? throw new InvalidOperationException(
                    "Description AI returned a null description value.");
=======
                ?? throw new InvalidOperationException("Description AI returned a null description value.");
>>>>>>> 63ae425cacb8f085d875228d89a74a932d83f2df
        }
    }
}
