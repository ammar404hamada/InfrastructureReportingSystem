using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Domain.Enums;
using InfraReportingSystem.Persistence.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Persistence.Seed
{
    public static class TestAuditLogSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            if (await db.AuditLog.AnyAsync(al => al.Details != null && al.Details.StartsWith("[TEST]")))
                return;

            var admin = await userManager.FindByEmailAsync("admin@infracare.eg");
            var authority = await userManager.FindByEmailAsync("m.hassan@gov.eg");
            var worker1 = await userManager.FindByEmailAsync("o.youssef@gov.eg");
            var worker2 = await userManager.FindByEmailAsync("k.ali@gov.eg");
            var publicUser = await userManager.FindByEmailAsync("ahmed.tariq@gmail.com");

            var now = DateTime.UtcNow;

            AuditLog CreateLog(string? userId, AuditActionType action, string entityName, string entityId,
                string details, DateTime timestamp)
            {
                return new AuditLog
                {
                    UserId = userId,
                    ActionType = action,
                    EntityName = entityName,
                    EntityId = entityId,
                    Details = $"[TEST] {details}",
                    Timestamp = timestamp
                };
            }

            var logs = new List<AuditLog>();

            // 1–5: Admin actions
            logs.Add(CreateLog(admin.Id, AuditActionType.AccountCreated, "User", "U100",
                "Admin created Worker account", now.AddDays(-60)));
            logs.Add(CreateLog(admin.Id, AuditActionType.AccountCreated, "User", "U101",
                "Admin created Authority account", now.AddDays(-58)));
            logs.Add(CreateLog(admin.Id, AuditActionType.ReportCreated, "Report", "R1",
                "Report created by admin", now.AddDays(-55)));
            logs.Add(CreateLog(admin.Id, AuditActionType.ReportAssigned, "Report", "R2",
                "Admin assigned report", now.AddDays(-50)));
            logs.Add(CreateLog(admin.Id, AuditActionType.AccountCreated, "User", "U102",
                "Admin created another Worker", now.AddDays(-40)));

            // 6–10: Authority actions
            logs.Add(CreateLog(authority.Id, AuditActionType.ReportAssigned, "Report", "R3",
                "Assigned to worker", now.AddDays(-45)));
            logs.Add(CreateLog(authority.Id, AuditActionType.ReportCreated, "Report", "R4",
                "Authority submitted report", now.AddDays(-40)));
            logs.Add(CreateLog(authority.Id, AuditActionType.ReportAssigned, "Report", "R5",
                "Assigned second report", now.AddDays(-35)));
            logs.Add(CreateLog(authority.Id, AuditActionType.ReportResolved, "Report", "R6",
                "Authority resolved", now.AddDays(-30)));
            logs.Add(CreateLog(authority.Id, AuditActionType.ReportCreated, "Report", "R7",
                "Authority created again", now.AddDays(-25)));

            // 11–15: Worker 1 actions (Omar)
            logs.Add(CreateLog(worker1.Id, AuditActionType.WorkerAcceptedTask, "Task", "T1",
                "Worker accepted task", now.AddDays(-42)));
            logs.Add(CreateLog(worker1.Id, AuditActionType.WorkerRejectedTask, "Task", "T2",
                "Worker rejected task", now.AddDays(-38)));
            logs.Add(CreateLog(worker1.Id, AuditActionType.WorkerMarkedTaskAsFixed, "Task", "T3",
                "Worker fixed task", now.AddDays(-32)));
            logs.Add(CreateLog(worker1.Id, AuditActionType.ReportBlocked, "Task", "T4",
                "Worker blocked task", now.AddDays(-28)));
            logs.Add(CreateLog(worker1.Id, AuditActionType.WorkerAcceptedTask, "Task", "T5",
                "Accepted another", now.AddDays(-22)));

            // 16–20: Worker 2 actions (Karim)
            logs.Add(CreateLog(worker2.Id, AuditActionType.WorkerAcceptedTask, "Task", "T6",
                "Worker2 accepted", now.AddDays(-44)));
            logs.Add(CreateLog(worker2.Id, AuditActionType.WorkerMarkedTaskAsFixed, "Task", "T7",
                "Worker2 fixed", now.AddDays(-39)));
            logs.Add(CreateLog(worker2.Id, AuditActionType.WorkerRejectedTask, "Task", "T8",
                "Worker2 rejected", now.AddDays(-33)));
            logs.Add(CreateLog(worker2.Id, AuditActionType.WorkerAcceptedTask, "Task", "T9",
                "Worker2 accepted", now.AddDays(-27)));
            logs.Add(CreateLog(worker2.Id, AuditActionType.ReportBlocked, "Task", "T10",
                "Worker2 blocked", now.AddDays(-20)));

            // 21–25: Public user actions
            logs.Add(CreateLog(publicUser.Id, AuditActionType.ReportCreated, "Report", "R8",
                "Public submitted", now.AddDays(-48)));
            logs.Add(CreateLog(publicUser.Id, AuditActionType.ReportCreated, "Report", "R9",
                "Public submitted again", now.AddDays(-43)));
            logs.Add(CreateLog(publicUser.Id, AuditActionType.ReportCreated, "Report", "R10",
                "Public third report", now.AddDays(-36)));
            logs.Add(CreateLog(publicUser.Id, AuditActionType.ReportCreated, "Report", "R11",
                "Public fourth", now.AddDays(-30)));
            logs.Add(CreateLog(publicUser.Id, AuditActionType.ReportCreated, "Report", "R12",
                "Public fifth", now.AddDays(-24)));

            // 26–30: System events (UserId null)
            logs.Add(CreateLog(null, AuditActionType.LoginFailed, "System", "SYS1",
                "System auto-generated", now.AddDays(-57)));
            logs.Add(CreateLog(null, AuditActionType.PasswordReset, "System", "SYS2",
                "System reset", now.AddDays(-52)));
            logs.Add(CreateLog(null, AuditActionType.Login, "System", "SYS3",
                "System login event", now.AddDays(-46)));
            logs.Add(CreateLog(null, AuditActionType.ReportResolved, "System", "SYS4",
                "System auto-resolve", now.AddDays(-34)));
            logs.Add(CreateLog(null, AuditActionType.AccountCreated, "System", "SYS5",
                "System account creation", now.AddDays(-26)));

            // 31–35: Recent mixed logs
            logs.Add(CreateLog(admin.Id, AuditActionType.ReportCreated, "Report", "R13",
                "Recent admin report", now.AddDays(-10)));
            logs.Add(CreateLog(authority.Id, AuditActionType.ReportAssigned, "Report", "R14",
                "Recent authority assign", now.AddDays(-8)));
            logs.Add(CreateLog(worker1.Id, AuditActionType.WorkerAcceptedTask, "Task", "T11",
                "Recent worker accept", now.AddDays(-6)));
            logs.Add(CreateLog(worker2.Id, AuditActionType.WorkerMarkedTaskAsFixed, "Task", "T12",
                "Recent worker fix", now.AddDays(-4)));
            logs.Add(CreateLog(publicUser.Id, AuditActionType.ReportCreated, "Report", "R15",
                "Recent public report", now.AddDays(-2)));

            db.AuditLog.AddRange(logs);
            await db.SaveChangesAsync();
        }
    }
}
