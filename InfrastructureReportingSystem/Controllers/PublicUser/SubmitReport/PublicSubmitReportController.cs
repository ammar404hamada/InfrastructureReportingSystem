using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.ServiceAbstractions.Users.PublicUser.SubmitReport;
using InfraReportingSystem.Shared.DTOs.Users.PublicUser.SubmitReport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/public/report")]
[Authorize(Roles = "PublicUser")]
public class PublicSubmitReportController : ControllerBase
{
    private readonly IPublicSubmitReportService _service;

    public PublicSubmitReportController(IPublicSubmitReportService service)
    {
        _service = service;
    }

    /// <summary>
    /// Sends the images to the AI service and returns a list of suggested categories and descriptions.
    /// Nothing is saved to the database.
    /// </summary>
    [HttpPost("analyze-image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AnalyzeImage([FromForm] List<IFormFile> images)
    {
        if (images == null || images.Count == 0)
            return BadRequest("No images provided.");

        var imageStreams = images.Select(img => (img.OpenReadStream(), img.FileName)).ToList();
        
        try
        {
            var result = await _service.AnalyzeImagesAsync(imageStreams);
            return Ok(result);
        }
        finally
        {
            foreach (var stream in imageStreams)
            {
                stream.Item1.Dispose();
            }
        }
    }

    /// <summary>
    /// Final submission. Uploads multiple images to Cloudinary, creates Report,
    /// ReportPics, and AuditLog. Only persistence step in the entire flow.
    /// </summary>
    [HttpPost("submit")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SubmitReport(
        [FromForm] SubmitReportRequestDto request,
        [FromForm] List<IFormFile> images)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        if (images == null || images.Count == 0)
            return BadRequest("At least one image is required to submit a report.");

        var imageStreams = images.Select(img => (img.OpenReadStream(), img.FileName)).ToList();

        try
        {
            var result = await _service.SubmitReportAsync(request, imageStreams, userId);
            return Ok(result);
        }
        finally
        {
            foreach (var stream in imageStreams)
            {
                stream.Item1.Dispose();
            }
        }
    }
}