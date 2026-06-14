using System.Security.Claims;
using InfraReportingSystem.ServiceAbstractions.Users.PublicUser.SubmitReport;
using InfraReportingSystem.Shared.DTOs.Users.PublicUser.SubmitReport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfrastructureReportingSystem.Controllers.PublicUser.SubmitReport;

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
    /// Sends the image to the AI service and returns a suggested category.
    /// Nothing is saved to the database.
    /// </summary>
    [HttpPost("generate-category")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> GenerateCategory([FromForm] IFormFile image)
    {
        await using var stream = image.OpenReadStream();
        var result = await _service.GenerateCategoryAsync(stream, image.FileName);
        return Ok(result);
    }

    /// <summary>
    /// Sends the image to the AI service and returns a generated description.
    /// Nothing is saved to the database.
    /// </summary>
    [HttpPost("generate-description")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> GenerateDescription([FromForm] IFormFile image)
    {
        await using var stream = image.OpenReadStream();
        var result = await _service.GenerateDescriptionAsync(stream, image.FileName);
        return Ok(result);
    }

    /// <summary>
    /// Final submission. Uploads image to Cloudinary, creates Report,
    /// ReportPic, and AuditLog. Only persistence step in the entire flow.
    /// </summary>
    [HttpPost("submit")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SubmitReport(
        [FromForm] SubmitReportRequestDto request,
        [FromForm] IFormFile image)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        await using var stream = image.OpenReadStream();
        var result = await _service.SubmitReportAsync(request, stream, image.FileName, userId);
        return Ok(result);
    }
}