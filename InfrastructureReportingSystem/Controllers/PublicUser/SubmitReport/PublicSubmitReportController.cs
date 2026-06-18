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
    /// <param name="images">A list of image files to analyze.</param>
    /// <response code="200">Returns the AI-generated category and description suggestions.</response>
    /// <response code="400">No images were provided.</response>
    /// <response code="401">The request does not contain a valid JWT access token.</response>
    /// <response code="403">The authenticated user does not have the PublicUser role.</response>
    /// <response code="503">The AI service is temporarily unavailable.</response>
    [HttpPost("analyze-image")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(AISuggestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
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
        catch (InvalidOperationException ex)
        {
            return StatusCode(503, new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An unexpected error occurred." });
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
    /// <param name="request">The report details including description, category, and location.</param>
    /// <param name="images">A list of image files to upload as evidence.</param>
    /// <response code="200">The report was submitted successfully. Returns the submission result with report ID.</response>
    /// <response code="400">No images were provided or the request is invalid.</response>
    /// <response code="401">The request does not contain a valid JWT access token.</response>
    /// <response code="403">The authenticated user does not have the PublicUser role.</response>
    [HttpPost("submit")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(SubmitReportResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SubmitReport(
    [FromForm] SubmitReportRequestDto request,
    [FromForm] List<IFormFile> images)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        if (images == null || images.Count == 0)
            return BadRequest("At least one image is required to submit a report.");


        var imageStreams = new List<(Stream Data, string FileName)>();
        foreach (var img in images)
        {
            var ms = new MemoryStream();
            await img.CopyToAsync(ms);
            ms.Position = 0;
            imageStreams.Add((ms, img.FileName));
        }

        try
        {
            var result = await _service.SubmitReportAsync(request, imageStreams, userId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(503, new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
        finally
        {
            foreach (var stream in imageStreams)
            {
                stream.Data.Dispose();
            }
        }
    }
}