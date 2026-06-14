using System.Security.Claims;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.ServiceAbstractions.Users.PublicUser.SubmitReport;
using InfraReportingSystem.Shared.DTOs.Users.PublicUser.SubmitReport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfrastructureReportingSystem.Controllers.PublicUser.SubmitReport
{
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

        [HttpPost("generate-category")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> GenerateCategory([FromForm] IFormFile image)
        {
            var result = await _service.GenerateCategoryAsync(image);
            return Ok(result);
        }

        [HttpPost("generate-description")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> GenerateDescription([FromForm] IFormFile image)
        {
            var result = await _service.GenerateDescriptionAsync(image);
            return Ok(result);
        }

        [HttpPost("submit")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SubmitReport([FromForm] SubmitReportRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var result = await _service.SubmitReportAsync(request, userId);
            return Ok(result);
        }
    }
}
