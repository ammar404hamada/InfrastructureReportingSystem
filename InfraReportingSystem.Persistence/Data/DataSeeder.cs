using System;
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
			}

			
			await SeedRolesAsync();

			
			await SeedAdminAuthorityAsync();

			
			await SeedSampleWorkerAsync();
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

	private async Task SeedAdminAuthorityAsync()
	{
		if (!await _context.Authorities.AnyAsync())
		{
			var admin = new Authority
			{
				UserName = "admin@system.com",
				Email = "admin@system.com",
				Name = "System Administrator",
				Status = UserStatus.Active, 
				EmailConfirmed = true
			};

			var result = await _userManager.CreateAsync(admin, "Admin@123!");

			if (result.Succeeded)
			{
				await _userManager.AddToRoleAsync(admin, "Admin");
				await _userManager.AddToRoleAsync(admin, "Authority");
			}
		}
	}

	private async Task SeedSampleWorkerAsync()
	{
		if (!await _context.Workers.AnyAsync())
		{
			var worker = new Worker
			{
				UserName = "worker1@system.com",
				Email = "worker1@system.com",
				Name = "John Doe",
				Specialization = "General Maintenance", 
				Status = UserStatus.Active,
				EmailConfirmed = true
			};

			var result = await _userManager.CreateAsync(worker, "Worker@123!");

			if (result.Succeeded)
			{
				await _userManager.AddToRoleAsync(worker, "Worker");
			}
		}
	}
}