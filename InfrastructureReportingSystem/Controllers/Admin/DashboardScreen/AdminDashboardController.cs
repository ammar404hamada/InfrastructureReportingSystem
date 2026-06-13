using InfraReportingSystem.ServiceAbstractions.Users.Admin.DashboardScreen;
using InfraReportingSystem.Shared.DTOs.UserServices.Admin.DashboardScreen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureReportingSystem.Controllers.Admin.DashboardScreen;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminDashboardService _dashboardService;
    private readonly ILogger<AdminDashboardController> _logger;

    public AdminDashboardController(
        IAdminDashboardService dashboardService,
        ILogger<AdminDashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Returns the complete admin dashboard data including stats, weekly traffic,
    /// category statistics, live issues, and recent worker actions — all in one response.
    /// </summary>
    /// <response code="200">Returns the combined dashboard response with all five data sections.</response>
    /// <response code="401">The request does not contain a valid JWT access token.</response>
    /// <response code="403">The authenticated user does not have the Admin role.</response>
    [HttpGet]
    [ProducesResponseType(typeof(DashboardResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DashboardResponseDto>> GetDashboard()
    {
        try
        {
            var result = await _dashboardService.GetDashboardAsync();
            return Ok(result);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error in {ActionName}: {Message}", nameof(GetDashboard), ex.Message);
            return StatusCode(500, new { message = "Database error occurred while retrieving dashboard data." });
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Error in {ActionName}: {Message}", nameof(GetDashboard), ex.Message);
            return StatusCode(500, new { message = "Database error occurred while retrieving dashboard data." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error in {ActionName}: {Message}", nameof(GetDashboard), ex.Message);
            return StatusCode(500, new { message = "Database error occurred while retrieving dashboard data." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {ActionName}: {Message}", nameof(GetDashboard), ex.Message);
            return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
        }
    }

    /// <summary>
    /// Returns key summary statistics for the admin dashboard: total registered users,
    /// active in‑field workers, pending/open issues, and resolved issues.
    /// </summary>
    /// <response code="200">Returns the four dashboard stats counters.</response>
    /// <response code="401">The request does not contain a valid JWT access token.</response>
    /// <response code="403">The authenticated user does not have the Admin role.</response>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DashboardStatsDto>> GetStats()
    {
        try
        {
            var result = await _dashboardService.GetStatsAsync();
            return Ok(result);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error in {ActionName}: {Message}", nameof(GetStats), ex.Message);
            return StatusCode(500, new { message = "Database error occurred while retrieving dashboard stats." });
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Error in {ActionName}: {Message}", nameof(GetStats), ex.Message);
            return StatusCode(500, new { message = "Database error occurred while retrieving dashboard stats." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error in {ActionName}: {Message}", nameof(GetStats), ex.Message);
            return StatusCode(500, new { message = "Database error occurred while retrieving dashboard stats." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {ActionName}: {Message}", nameof(GetStats), ex.Message);
            return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
        }
    }

    /// <summary>
    /// Returns an array of 7 days (Monday through Sunday) with reported and resolved
    /// report counts for the current UTC week.
    /// </summary>
    /// <response code="200">Returns a list of daily reported and resolved counts for the current week.</response>
    /// <response code="401">The request does not contain a valid JWT access token.</response>
    /// <response code="403">The authenticated user does not have the Admin role.</response>
    [HttpGet("weekly-traffic")]
    [ProducesResponseType(typeof(List<WeeklyTrafficDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<WeeklyTrafficDto>>> GetWeeklyTraffic()
    {
        try
        {
            var result = await _dashboardService.GetWeeklyTrafficAsync();
            return Ok(result);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error in {ActionName}: {Message}", nameof(GetWeeklyTraffic), ex.Message);
            return StatusCode(500, new { message = "Database error occurred while retrieving weekly traffic data." });
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Error in {ActionName}: {Message}", nameof(GetWeeklyTraffic), ex.Message);
            return StatusCode(500, new { message = "Database error occurred while retrieving weekly traffic data." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error in {ActionName}: {Message}", nameof(GetWeeklyTraffic), ex.Message);
            return StatusCode(500, new { message = "Database error occurred while retrieving weekly traffic data." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {ActionName}: {Message}", nameof(GetWeeklyTraffic), ex.Message);
            return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
        }
    }

    /// <summary>
    /// Returns per‑category statistics including the total number of reported issues
    /// and the number of resolved issues for each category.
    /// </summary>
    /// <response code="200">Returns a list of category stats with reported and resolved counts.</response>
    /// <response code="401">The request does not contain a valid JWT access token.</response>
    /// <response code="403">The authenticated user does not have the Admin role.</response>
    [HttpGet("category-stats")]
    [ProducesResponseType(typeof(List<CategoryStatDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<CategoryStatDto>>> GetCategoryStats()
    {
        try
        {
            var result = await _dashboardService.GetCategoryStatsAsync();
            return Ok(result);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error in {ActionName}: {Message}", nameof(GetCategoryStats), ex.Message);
            return StatusCode(500, new { message = "Database error occurred while retrieving category statistics." });
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Error in {ActionName}: {Message}", nameof(GetCategoryStats), ex.Message);
            return StatusCode(500, new { message = "Database error occurred while retrieving category statistics." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error in {ActionName}: {Message}", nameof(GetCategoryStats), ex.Message);
            return StatusCode(500, new { message = "Database error occurred while retrieving category statistics." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {ActionName}: {Message}", nameof(GetCategoryStats), ex.Message);
            return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
        }
    }

    /// <summary>
    /// Returns all unresolved reports (status not equal to Resolved) with their category,
    /// reporter name, status, submission date, and a computed priority (High if older
    /// than 7 days, otherwise Normal).
    /// </summary>
    /// <response code="200">Returns a list of live unresolved issues with computed priority.</response>
    /// <response code="401">The request does not contain a valid JWT access token.</response>
    /// <response code="403">The authenticated user does not have the Admin role.</response>
    [HttpGet("live-issues")]
    [ProducesResponseType(typeof(List<LiveIssueDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<LiveIssueDto>>> GetLiveIssues()
    {
        try
        {
            var result = await _dashboardService.GetLiveIssuesAsync();
            return Ok(result);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error in {ActionName}: {Message}", nameof(GetLiveIssues), ex.Message);
            return StatusCode(500, new { message = "Database error occurred while retrieving live issues." });
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Error in {ActionName}: {Message}", nameof(GetLiveIssues), ex.Message);
            return StatusCode(500, new { message = "Database error occurred while retrieving live issues." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error in {ActionName}: {Message}", nameof(GetLiveIssues), ex.Message);
            return StatusCode(500, new { message = "Database error occurred while retrieving live issues." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {ActionName}: {Message}", nameof(GetLiveIssues), ex.Message);
            return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
        }
    }

    /// <summary>
    /// Returns the last 10 audit log entries for worker‑related actions
    /// (WorkerAcceptedTask, WorkerRejectedTask, WorkerMarkedTaskAsFixed),
    /// including the worker name, action type, target entity, and timestamp.
    /// </summary>
    /// <response code="200">Returns a list of the most recent worker action audit log entries.</response>
    /// <response code="401">The request does not contain a valid JWT access token.</response>
    /// <response code="403">The authenticated user does not have the Admin role.</response>
    [HttpGet("recent-actions")]
    [ProducesResponseType(typeof(List<RecentActionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<RecentActionDto>>> GetRecentActions()
    {
        try
        {
            var result = await _dashboardService.GetRecentActionsAsync();
            return Ok(result);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error in {ActionName}: {Message}", nameof(GetRecentActions), ex.Message);
            return StatusCode(500, new { message = "Database error occurred while retrieving recent actions." });
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Error in {ActionName}: {Message}", nameof(GetRecentActions), ex.Message);
            return StatusCode(500, new { message = "Database error occurred while retrieving recent actions." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error in {ActionName}: {Message}", nameof(GetRecentActions), ex.Message);
            return StatusCode(500, new { message = "Database error occurred while retrieving recent actions." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {ActionName}: {Message}", nameof(GetRecentActions), ex.Message);
            return StatusCode(500, new { message = "An unexpected error occurred. Please try again later." });
        }
    }
}
