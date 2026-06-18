using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using InfraReportingSystem.Services.AI;
using Moq;
using Moq.Protected;
using Xunit;

namespace InfrastructureReportingSystem.Tests.Unit.AI;

public class AnalyzeImageAIClientTests
{
    [Fact]
    public async Task AnalyzeImagesAsync_WhenResponseIsSuccessful_ReturnsMappedResult()
    {
        var json = JsonSerializer.Serialize(new
        {
            classification = "Roads",
            description = "Pothole on main road"
        });
        var handlerMock = CreateHandlerMock(HttpStatusCode.OK, json);
        using var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost") };

        var client = new AnalyzeImageAIClient(httpClient);
        using var stream1 = new MemoryStream([1, 2, 3]);
        using var stream2 = new MemoryStream([4, 5, 6]);
        var images = new[] { (Stream: (Stream)stream1, FileName: "photo1.jpg"), (Stream: (Stream)stream2, FileName: "photo2.jpg") };

        var result = await client.AnalyzeImagesAsync(images);

        result.Prediction.Should().Be("Roads");
        result.ImageDescription.Should().Be("Pothole on main road");
    }

    [Fact]
    public async Task AnalyzeImagesAsync_WhenHttpResponseFails_ThrowsHttpRequestException()
    {
        var handlerMock = CreateHandlerMock(HttpStatusCode.InternalServerError, "Error");
        using var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost") };

        var client = new AnalyzeImageAIClient(httpClient);
        using var stream = new MemoryStream([1, 2, 3]);
        var images = new[] { (Stream: (Stream)stream, FileName: "photo.jpg") };

        Func<Task> action = async () => await client.AnalyzeImagesAsync(images);

        await action.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task AnalyzeImagesAsync_WhenResponseMissingClassification_ThrowsInvalidOperationException()
    {
        var json = JsonSerializer.Serialize(new { description = "Some description" });
        var handlerMock = CreateHandlerMock(HttpStatusCode.OK, json);
        using var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost") };

        var client = new AnalyzeImageAIClient(httpClient);
        using var stream = new MemoryStream([1, 2, 3]);
        var images = new[] { (Stream: (Stream)stream, FileName: "photo.jpg") };

        Func<Task> action = async () => await client.AnalyzeImagesAsync(images);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*missing*'prediction'*");
    }

    [Fact]
    public async Task AnalyzeImagesAsync_WhenResponseMissingDescription_ThrowsInvalidOperationException()
    {
        var json = JsonSerializer.Serialize(new { classification = "Roads" });
        var handlerMock = CreateHandlerMock(HttpStatusCode.OK, json);
        using var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost") };

        var client = new AnalyzeImageAIClient(httpClient);
        using var stream = new MemoryStream([1, 2, 3]);
        var images = new[] { (Stream: (Stream)stream, FileName: "photo.jpg") };

        Func<Task> action = async () => await client.AnalyzeImagesAsync(images);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*missing*'image_description'*");
    }

    [Fact]
    public async Task AnalyzeImagesAsync_WhenPredictionIsNull_ThrowsInvalidOperationException()
    {
        var json = JsonSerializer.Serialize(new
        {
            classification = (string?)null,
            description = "Some description"
        });
        var handlerMock = CreateHandlerMock(HttpStatusCode.OK, json);
        using var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost") };

        var client = new AnalyzeImageAIClient(httpClient);
        using var stream = new MemoryStream([1, 2, 3]);
        var images = new[] { (Stream: (Stream)stream, FileName: "photo.jpg") };

        Func<Task> action = async () => await client.AnalyzeImagesAsync(images);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*null prediction*");
    }

    [Fact]
    public async Task AnalyzeImagesAsync_WhenDescriptionIsNull_ThrowsInvalidOperationException()
    {
        var json = JsonSerializer.Serialize(new
        {
            classification = "Roads",
            description = (string?)null
        });
        var handlerMock = CreateHandlerMock(HttpStatusCode.OK, json);
        using var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost") };

        var client = new AnalyzeImageAIClient(httpClient);
        using var stream = new MemoryStream([1, 2, 3]);
        var images = new[] { (Stream: (Stream)stream, FileName: "photo.jpg") };

        Func<Task> action = async () => await client.AnalyzeImagesAsync(images);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*null image_description*");
    }

    private static Mock<HttpMessageHandler> CreateHandlerMock(HttpStatusCode statusCode, string content)
    {
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            });

        return handlerMock;
    }
}
