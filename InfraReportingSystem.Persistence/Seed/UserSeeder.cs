using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Persistence.Seed
{
    public static class UserSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            // Guard — check for a known test user
            if (await userManager.FindByEmailAsync("testworker1@test.com") != null)
                return;

            // Helper to create any user type (Worker, Authority, base User)
            async Task CreateTestUser(User user, string password)
            {
                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create test user {user.Email}: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
                // No role assignment needed — TPH discriminator used for roles
            }

            // === Workers ===
            await CreateTestUser(new Worker
            {
                UserName = "testworker1@test.com",
                Email = "testworker1@test.com",
                Name = "Ahmed Ali",
                PhoneNumber = "01011112222",
                Status = UserStatus.Active,
                CreatedAt = new DateTime(2025, 12, 1, 10, 0, 0, DateTimeKind.Utc),
                EmailConfirmed = true
            }, "Pass@123");

            await CreateTestUser(new Worker
            {
                UserName = "testworker2@test.com",
                Email = "testworker2@test.com",
                Name = "Ahmed Omar",
                PhoneNumber = "01011113333",
                Status = UserStatus.Active,
                CreatedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                EmailConfirmed = true
            }, "Pass@123");

            await CreateTestUser(new Worker
            {
                UserName = "testworker3@test.com",
                Email = "testworker3@test.com",
                Name = "Ahmed Hassan",
                PhoneNumber = "01011114444",
                Status = UserStatus.Inactive,
                CreatedAt = new DateTime(2025, 6, 20, 0, 0, 0, DateTimeKind.Utc),
                EmailConfirmed = true
            }, "Pass@123");

            // === Authorities ===
            await CreateTestUser(new Authority
            {
                UserName = "testauth1@test.com",
                Email = "testauth1@test.com",
                Name = "Fatima Ahmed",
                PhoneNumber = "01022221111",
                Status = UserStatus.Active,
                CreatedAt = new DateTime(2024, 3, 10, 0, 0, 0, DateTimeKind.Utc),
                EmailConfirmed = true
            }, "Pass@123");

            await CreateTestUser(new Authority
            {
                UserName = "testauth2@test.com",
                Email = "testauth2@test.com",
                Name = "Fatima Zahra",
                PhoneNumber = "01022222222",
                Status = UserStatus.Suspended,
                CreatedAt = new DateTime(2025, 8, 5, 0, 0, 0, DateTimeKind.Utc),
                EmailConfirmed = true
            }, "Pass@123");

            // === Public Users (base User) ===
            await CreateTestUser(new User
            {
                UserName = "testpub1@test.com",
                Email = "testpub1@test.com",
                Name = "Ahmed Samir",
                PhoneNumber = "01033331111",
                Status = UserStatus.Active,
                CreatedAt = new DateTime(2026, 2, 28, 0, 0, 0, DateTimeKind.Utc),
                EmailConfirmed = true
            }, "Pass@123");

            await CreateTestUser(new User
            {
                UserName = "testpub2@test.com",
                Email = "testpub2@test.com",
                Name = "Ahmed Khaled",
                PhoneNumber = "01033332222",
                Status = UserStatus.Inactive,
                CreatedAt = new DateTime(2023, 5, 1, 0, 0, 0, DateTimeKind.Utc),
                EmailConfirmed = true
            }, "Pass@123");

            await CreateTestUser(new User
            {
                UserName = "testpub3@test.com",
                Email = "testpub3@test.com",
                Name = "Fatima Nour",
                PhoneNumber = "01033333333",
                Status = UserStatus.Active,
                CreatedAt = new DateTime(2024, 11, 12, 0, 0, 0, DateTimeKind.Utc),
                EmailConfirmed = true
            }, "Pass@123");

            // Duplicate name with different join date for sorting tests
            await CreateTestUser(new Worker
            {
                UserName = "testworker4@test.com",
                Email = "testworker4@test.com",
                Name = "Ahmed Ali",
                PhoneNumber = "01011115555",
                Status = UserStatus.Active,
                CreatedAt = new DateTime(2025, 11, 1, 0, 0, 0, DateTimeKind.Utc),
                EmailConfirmed = true
            }, "Pass@123");
        }
    }
}
