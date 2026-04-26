using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InfraReportingSystem.Persistence.Data;

public class DataSeeder
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(
        AppDbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<DataSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            if (_context.Database.IsSqlServer() && (await _context.Database.GetPendingMigrationsAsync()).Any())
            {
                await _context.Database.MigrateAsync();
                _logger.LogInformation("Applied pending database migrations.");
            }

            await SeedRolesAsync();
            await SeedUsersAsync();
            await SeedCategoriesAsync();
            await SeedReportsAndLogsAsync();

            _logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[] { "Admin", "Authority", "Worker", "PublicUser" };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private async Task SeedUsersAsync()
    {
        // Admin
        if (await _userManager.FindByEmailAsync("admin@infracare.eg") == null)
        {
            var admin = new User
            {
                UserName = "admin@infracare.eg",
                Email = "admin@infracare.eg",
                Name = "System Administrator",
                Status = UserStatus.Active,
                EmailConfirmed = true,
                PhoneNumber = "01000000000",
               CreatedAt = DateTime.UtcNow,
            };
            await _userManager.CreateAsync(admin, "Admin@123!");
            await _userManager.AddToRoleAsync(admin, "Admin");
        }

        // Public User
        if (await _userManager.FindByEmailAsync("ahmed.tariq@gmail.com") == null)
        {
            var publicUser = new User
            {
                UserName = "ahmed.tariq@gmail.com",
                Email = "ahmed.tariq@gmail.com",
                Name = "Ahmed Tariq",
                Status = UserStatus.Active,
                EmailConfirmed = true,
                PhoneNumber = "01012345678",
                CreatedAt = DateTime.UtcNow,
            };
            await _userManager.CreateAsync(publicUser, "User@123!");
            await _userManager.AddToRoleAsync(publicUser, "PublicUser");
        }

        // Authority
        if (await _userManager.FindByEmailAsync("m.hassan@gov.eg") == null)
        {
            var authority = new Authority
            {
                UserName = "m.hassan@gov.eg",
                Email = "m.hassan@gov.eg",
                Name = "Mahmoud Hassan",
                Status = UserStatus.Active,
                EmailConfirmed = true,
                PhoneNumber = "01212345678",
                CreatedAt = DateTime.UtcNow,
            };
            await _userManager.CreateAsync(authority, "Auth@123!");
            await _userManager.AddToRoleAsync(authority, "Authority");
        }

        // Worker 1 - Omar (existing)
        if (await _userManager.FindByEmailAsync("o.youssef@gov.eg") == null)
        {
            var worker1 = new Worker
            {
                UserName = "o.youssef@gov.eg",
                Email = "o.youssef@gov.eg",
                Name = "Omar Youssef",
                Specialization = "Water & Plumbing",
                Status = UserStatus.Active,
                EmailConfirmed = true,
                PhoneNumber = "01512345678",
                CreatedAt = DateTime.UtcNow,
            };
            await _userManager.CreateAsync(worker1, "Worker@123!");
            await _userManager.AddToRoleAsync(worker1, "Worker");
        }

        // Worker 2 - Karim (Electrical)
        if (await _userManager.FindByEmailAsync("k.ali@gov.eg") == null)
        {
            var worker2 = new Worker
            {
                UserName = "k.ali@gov.eg",
                Email = "k.ali@gov.eg",
                Name = "Karim Ali",
                Specialization = "Electrical Systems",
                Status = UserStatus.Active,
                EmailConfirmed = true,
                PhoneNumber = "01123456789",
                CreatedAt = DateTime.UtcNow,
            };
            await _userManager.CreateAsync(worker2, "Worker@123!");
            await _userManager.AddToRoleAsync(worker2, "Worker");
        }

        // Worker 3 - Sara (Roads)
        if (await _userManager.FindByEmailAsync("s.mahmoud@gov.eg") == null)
        {
            var worker3 = new Worker
            {
                UserName = "s.mahmoud@gov.eg",
                Email = "s.mahmoud@gov.eg",
                Name = "Sara Mahmoud",
                Specialization = "Roads & Asphalt",
                Status = UserStatus.Active,
                EmailConfirmed = true,
                PhoneNumber = "01234567890",
                CreatedAt = DateTime.UtcNow,
            };
            await _userManager.CreateAsync(worker3, "Worker@123!");
            await _userManager.AddToRoleAsync(worker3, "Worker");
        }

        // Worker 4 - Hossam (Water & Sewage)
        if (await _userManager.FindByEmailAsync("h.ibrahim@gov.eg") == null)
        {
            var worker4 = new Worker
            {
                UserName = "h.ibrahim@gov.eg",
                Email = "h.ibrahim@gov.eg",
                Name = "Hossam Ibrahim",
                Specialization = "Water & Sewage",
                Status = UserStatus.Active,
                EmailConfirmed = true,
                PhoneNumber = "01598765432",
                CreatedAt = DateTime.UtcNow,
            };
            await _userManager.CreateAsync(worker4, "Worker@123!");
            await _userManager.AddToRoleAsync(worker4, "Worker");
        }

        // Worker 5 - No assignments (for testing)
        if (await _userManager.FindByEmailAsync("n.assignments@gov.eg") == null)
        {
            var worker5 = new Worker
            {
                UserName = "n.assignments@gov.eg",
                Email = "n.assignments@gov.eg",
                Name = "No Assignments Tester",
                Specialization = "General",
                Status = UserStatus.Active,
                EmailConfirmed = true,
                PhoneNumber = "01987654321",
                CreatedAt = DateTime.UtcNow,
            };
            await _userManager.CreateAsync(worker5, "Worker@123!");
            await _userManager.AddToRoleAsync(worker5, "Worker");
        }
    }

    private async Task SeedCategoriesAsync()
    {
        if (!await _context.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new Category { Name = "Roads & Potholes" },
                new Category { Name = "Street Lighting" },
                new Category { Name = "Water Leaks & Sewage" },
                new Category { Name = "Electricity Outages" }
            };

            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded categories.");
        }
    }

    private async Task SeedReportsAndLogsAsync()
    {
        // Get categories
        var waterCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Water Leaks & Sewage");
        var roadsCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Roads & Potholes");
        var lightingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Street Lighting");
        var electricityCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Electricity Outages");

        if (waterCategory == null || roadsCategory == null || lightingCategory == null || electricityCategory == null)
        {
            _logger.LogWarning("Required categories not found. Skipping report seeding.");
            return;
        }

        // Get users
        var ahmed = await _userManager.FindByEmailAsync("ahmed.tariq@gmail.com");
        var authority = await _userManager.FindByEmailAsync("m.hassan@gov.eg") as Authority;
        var omar = await _userManager.FindByEmailAsync("o.youssef@gov.eg") as Worker;
        var karim = await _userManager.FindByEmailAsync("k.ali@gov.eg") as Worker;
        var sara = await _userManager.FindByEmailAsync("s.mahmoud@gov.eg") as Worker;
        var hossam = await _userManager.FindByEmailAsync("h.ibrahim@gov.eg") as Worker;

        if (ahmed == null || authority == null || omar == null || karim == null || sara == null || hossam == null)
        {
            _logger.LogWarning("Required users not found. Skipping report seeding.");
            return;
        }

        // Helper to add report if not exists (by unique combination of description + lat + lng)
        async Task AddReportIfNotExists(Report report)
        {
            var exists = await _context.Reports.AnyAsync(r =>
                r.Description == report.Description &&
                Math.Abs(r.Latitude - report.Latitude) < 0.0001 &&
                Math.Abs(r.Longitude - report.Longitude) < 0.0001);

            if (!exists)
            {
                await _context.Reports.AddAsync(report);
                _logger.LogInformation($"Adding report: {report.Description}");
            }
            else
            {
                _logger.LogInformation($"Report already exists, skipping: {report.Description}");
            }
        }

        // Define all reports with different statuses
        var reportsToAdd = new List<Report>
        {
            // 1. Submitted (unassigned)
            new Report
            {
                Description = "Major water pipe burst flooding El-Horreya street, causing heavy traffic.",
                CategoryId = waterCategory.Id,
                SubmittedById = ahmed.Id,
                Status = ReportStatus.Submitted,
                Latitude = 31.2001,
                Longitude = 29.9187,
                UploadedAt = DateTime.UtcNow.AddDays(-1),
                ReportPics = new List<ReportPic> { new ReportPic { PicUrl = "https://placehold.co/600x400/png?text=Water+Leak" } }
            },

            // 2. Assigned to Omar
            new Report
            {
                Description = "Deep pothole causing vehicle damage on the Ring Road near Maadi exit.",
                CategoryId = roadsCategory.Id,
                SubmittedById = ahmed.Id,
                AssignedByAuthorityId = authority.Id,
                AssignedWorkerId = omar.Id,
                Status = ReportStatus.Assigned,
                AssignedAt = DateTime.UtcNow.AddHours(-5),
                Latitude = 29.9602,
                Longitude = 31.2769,
                UploadedAt = DateTime.UtcNow.AddDays(-2),
                ReportPics = new List<ReportPic> { new ReportPic { PicUrl = "https://placehold.co/600x400/png?text=Pothole" } }
            },

            // 3. Assigned to Karim (street lighting)
            new Report
            {
                Description = "Street lights not working on entire Nasr City street - waiting for electrical parts.",
                CategoryId = lightingCategory.Id,
                SubmittedById = ahmed.Id,
                AssignedByAuthorityId = authority.Id,
                AssignedWorkerId = karim.Id,
                Status = ReportStatus.Assigned,
                AssignedAt = DateTime.UtcNow.AddDays(-1),
                Latitude = 30.0444,
                Longitude = 31.2357,
                UploadedAt = DateTime.UtcNow.AddDays(-3),
                ReportPics = new List<ReportPic> { new ReportPic { PicUrl = "https://placehold.co/600x400/png?text=Street+Light+Out" } }
            },

            // 4. OnHold (assigned to Omar)
            new Report
            {
                Description = "Multiple potholes on 6th October Bridge - on hold due to material shortage.",
                CategoryId = roadsCategory.Id,
                SubmittedById = ahmed.Id,
                AssignedByAuthorityId = authority.Id,
                AssignedWorkerId = omar.Id,
                Status = ReportStatus.OnHold,
                AssignedAt = DateTime.UtcNow.AddDays(-3),
                Latitude = 30.0450,
                Longitude = 31.2250,
                UploadedAt = DateTime.UtcNow.AddDays(-5),
                ReportPics = new List<ReportPic> { new ReportPic { PicUrl = "https://placehold.co/600x400/png?text=Potholes+Bridge" } }
            },

            // 5. Blocked (assigned to Karim)
            new Report
            {
                Description = "Power outage - building owner refuses access to electrical room.",
                CategoryId = electricityCategory.Id,
                SubmittedById = ahmed.Id,
                AssignedByAuthorityId = authority.Id,
                AssignedWorkerId = karim.Id,
                Status = ReportStatus.Blocked,
                AssignedAt = DateTime.UtcNow.AddDays(-2),
                Latitude = 30.0900,
                Longitude = 31.3240,
                UploadedAt = DateTime.UtcNow.AddDays(-4),
                ReportPics = new List<ReportPic> { new ReportPic { PicUrl = "https://placehold.co/600x400/png?text=Power+Outage" } }
            },

            // 6. PendingConfirmation (assigned to Sara)
            new Report
            {
                Description = "Pothole filled on 6th October Bridge - waiting for user confirmation.",
                CategoryId = roadsCategory.Id,
                SubmittedById = ahmed.Id,
                AssignedByAuthorityId = authority.Id,
                AssignedWorkerId = sara.Id,
                Status = ReportStatus.PendingConfirmation,
                AssignedAt = DateTime.UtcNow.AddDays(-4),
                Latitude = 30.0500,
                Longitude = 31.2300,
                UploadedAt = DateTime.UtcNow.AddDays(-6),
                ReportPics = new List<ReportPic> { new ReportPic { PicUrl = "https://placehold.co/600x400/png?text=Pothole+Fixed" } }
            },

            // 7. Resolved (assigned to Hossam)
            new Report
            {
                Description = "Water leak fixed in Mohandessin - user confirmed resolved.",
                CategoryId = waterCategory.Id,
                SubmittedById = ahmed.Id,
                AssignedByAuthorityId = authority.Id,
                AssignedWorkerId = hossam.Id,
                Status = ReportStatus.Resolved,
                AssignedAt = DateTime.UtcNow.AddDays(-5),
                Latitude = 30.0530,
                Longitude = 31.2050,
                UploadedAt = DateTime.UtcNow.AddDays(-7),
                ReportPics = new List<ReportPic> { new ReportPic { PicUrl = "https://placehold.co/600x400/png?text=Water+Leak+Fixed" } }
            },

            // 8. Rejected (assigned to Omar)
            new Report
            {
                Description = "Street light repair rejected by user - still not working properly.",
                CategoryId = lightingCategory.Id,
                SubmittedById = ahmed.Id,
                AssignedByAuthorityId = authority.Id,
                AssignedWorkerId = omar.Id,
                Status = ReportStatus.Rejected,
                AssignedAt = DateTime.UtcNow.AddDays(-6),
                Latitude = 30.0150,
                Longitude = 31.2100,
                UploadedAt = DateTime.UtcNow.AddDays(-8),
                ReportPics = new List<ReportPic> { new ReportPic { PicUrl = "https://placehold.co/600x400/png?text=Light+Rejected" } }
            },

            // Second assigned report for Karim Ali (k.ali@gov.eg) – to test multiple assignments
            new Report
            {
                Description = "Faulty traffic light at Tahrir Square intersection, causing confusion for drivers.",
                CategoryId = lightingCategory.Id, // Street Lighting
                SubmittedById = ahmed.Id,
                AssignedByAuthorityId = authority.Id,
                AssignedWorkerId = karim.Id,
                Status = ReportStatus.Assigned,
                AssignedAt = DateTime.UtcNow.AddDays(-1),
                Latitude = 30.0440,
                Longitude = 31.2350,
                UploadedAt = DateTime.UtcNow.AddDays(-3),
                ReportPics = new List<ReportPic>
                {
                    new ReportPic { PicUrl = "https://placehold.co/600x400/png?text=Traffic+Light" }
                }
            },

            // 2nd assigned report for Karim – faulty traffic light
            new Report
            {
                Description = "Traffic light at Tahrir Square intersection stuck on red for over 2 hours.",
                CategoryId = lightingCategory.Id,
                SubmittedById = ahmed.Id,
                AssignedByAuthorityId = authority.Id,
                AssignedWorkerId = karim.Id,
                Status = ReportStatus.Assigned,
                AssignedAt = DateTime.UtcNow.AddDays(-1),
                Latitude = 30.0440,
                Longitude = 31.2350,
                UploadedAt = DateTime.UtcNow.AddDays(-3),
                ReportPics = new List<ReportPic>
                {
                    new ReportPic { PicUrl = "https://placehold.co/600x400/png?text=Traffic+Light+Red" }
                }
            },

            // 3rd assigned report for Karim – another electrical issue
            new Report
            {
                Description = "Street lamp pole damaged and leaning dangerously on El-Merghany Street.",
                CategoryId = lightingCategory.Id,
                SubmittedById = ahmed.Id,
                AssignedByAuthorityId = authority.Id,
                AssignedWorkerId = karim.Id,
                Status = ReportStatus.Assigned,
                AssignedAt = DateTime.UtcNow.AddDays(-2),
                Latitude = 30.0740,
                Longitude = 31.2850,
                UploadedAt = DateTime.UtcNow.AddDays(-4),
                ReportPics = new List<ReportPic>
                {
                    new ReportPic { PicUrl = "https://placehold.co/600x400/png?text=Damaged+Lamp+Pole" }
                }
            },

            // 4th assigned report for Karim – power fluctuation
            new Report
            {
                Description = "Power fluctuations damaging home appliances in Heliopolis district.",
                CategoryId = electricityCategory.Id,
                SubmittedById = ahmed.Id,
                AssignedByAuthorityId = authority.Id,
                AssignedWorkerId = karim.Id,
                Status = ReportStatus.Assigned,
                AssignedAt = DateTime.UtcNow.AddDays(-3),
                Latitude = 30.0890,
                Longitude = 31.3220,
                UploadedAt = DateTime.UtcNow.AddDays(-5),
                ReportPics = new List<ReportPic>
                {
                    new ReportPic { PicUrl = "https://placehold.co/600x400/png?text=Power+Fluctuation" }
                }
            }

        };

        // Add reports (idempotent)
        foreach (var report in reportsToAdd)
        {
            await AddReportIfNotExists(report);
        }

        await _context.SaveChangesAsync();

        // Now add audit logs only for reports that don't have logs
        var allReports = await _context.Reports.ToListAsync();

        foreach (var report in allReports)
        {
            // Check if creation log exists
            var creationLogExists = await _context.AuditLog.AnyAsync(l =>
                l.EntityId == report.Id.ToString() && l.ActionType == AuditActionType.ReportCreated);

            if (!creationLogExists && report.Status == ReportStatus.Submitted && report.AssignedWorkerId == null)
            {
                var log = new AuditLog
                {
                    UserId = report.SubmittedById,
                    ActionType = AuditActionType.ReportCreated,
                    EntityName = "Report",
                    EntityId = report.Id.ToString(),
                    Details = $"Report created: {report.Description}",
                    Timestamp = report.UploadedAt
                };
                await _context.AuditLog.AddAsync(log);
            }

            // Check if assignment log exists
            var assignLogExists = await _context.AuditLog.AnyAsync(l =>
                l.EntityId == report.Id.ToString() && l.ActionType == AuditActionType.ReportAssigned);

            if (!assignLogExists && report.AssignedWorkerId != null && report.AssignedAt.HasValue)
            {
                var assignLog = new AuditLog
                {
                    UserId = report.AssignedByAuthorityId,
                    ActionType = AuditActionType.ReportAssigned,
                    EntityName = "Report",
                    EntityId = report.Id.ToString(),
                    Details = $"Assigned to worker {report.AssignedWorkerId}",
                    Timestamp = report.AssignedAt.Value
                };
                await _context.AuditLog.AddAsync(assignLog);
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded reports and audit logs (idempotent).");
    }
}