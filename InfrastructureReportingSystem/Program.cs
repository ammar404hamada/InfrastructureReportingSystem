using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Persistence.Data;
using InfraReportingSystem.Persistence.Repositories.Admin.UsersManagementScreen;
using InfraReportingSystem.Persistence.Repositories.Worker.CurrentTaskScreen;
using InfraReportingSystem.Persistence.Repositories.Worker.TasksHistoryScreen;
using InfraReportingSystem.Persistence.Repositories.Worker.TasksScreen;
using InfraReportingSystem.Persistence.Seed;
using InfraReportingSystem.ServiceAbstractions.Admin.UsersManagementScreen;
using InfraReportingSystem.ServiceAbstractions.Auth;
using InfraReportingSystem.ServiceAbstractions.Email;
using InfraReportingSystem.ServiceAbstractions.Repositories.Admin.UsersManagementScreen;
using InfraReportingSystem.ServiceAbstractions.Repositories.Worker.CurrentTaskScreen;
using InfraReportingSystem.ServiceAbstractions.Repositories.Worker.TasksHistoryScreen;
using InfraReportingSystem.ServiceAbstractions.Repositories.Worker.TasksScreen;
using InfraReportingSystem.ServiceAbstractions.Worker.CurrentTaskScreen;
using InfraReportingSystem.ServiceAbstractions.Worker.TasksHistoryScreen;
using InfraReportingSystem.ServiceAbstractions.Worker.TasksScreen;
using InfraReportingSystem.Services.Admin.UsersManagementScreen;
using InfraReportingSystem.Services.Auth;
using InfraReportingSystem.Services.Email;
using InfraReportingSystem.Services.Worker.CurrentTaskScreen;
using InfraReportingSystem.Services.Worker.TasksHistoryScreen;
using InfraReportingSystem.Services.Worker.TasksScreen;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace InfrastructureReportingSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            // add mybelove swagger
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Infra Reporting API", Version = "v1" });
            });

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            var jwtSettings = builder.Configuration.GetSection("JwtSettings");

            // Added validation to ensure the JWT key exists in configuration
            var key = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key missing");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                };
            });

            // Dependency Injection Registration
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IWorkerTasksRepository, WorkerTasksRepository>();
            builder.Services.AddScoped<IWorkerTasksService, WorkerTasksService>();
            builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            builder.Services.AddScoped<IWorkerTaskActionsRepository, WorkerTaskActionsRepository>();
            builder.Services.AddScoped<IWorkerTaskActionsService, WorkerTaskActionsService>();
            builder.Services.AddScoped<IWorkerCurrentTaskRepository, WorkerCurrentTaskRepository>();
            builder.Services.AddScoped<IWorkerCurrentTaskService, WorkerCurrentTaskService>();
            builder.Services.AddScoped<IWorkerCurrentTaskActionsRepository, WorkerCurrentTaskActionsRepository>();
            builder.Services.AddScoped<IWorkerCurrentTaskActionsService, WorkerCurrentTaskActionsService>();
            builder.Services.AddScoped<IWorkerHistoryRepository, WorkerHistoryRepository>();
            builder.Services.AddScoped<IWorkerHistoryService, WorkerHistoryService>();
            builder.Services.AddScoped<IAdminUsersRepository, AdminUsersRepository>();
            builder.Services.AddScoped<IAdminUsersService, AdminUsersService>();

            // Register DataSeeder
            builder.Services.AddScoped<DataSeeder>();

            var app = builder.Build();

            // Database Migration and Seeding Pipeline
            using (var scope = app.Services.CreateScope())
            {
                // 1. Run Migrations (via your existing DataSeeder)
                var dbSeeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
                await dbSeeder.SeedAsync(); // Assuming this calls context.Database.MigrateAsync()

                await UserSeeder.SeedAsync(app.Services);
            }

            // make the swagger UI public for testing (temporary)
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Infra Reporting API V1");
                c.RoutePrefix = string.Empty; // serve at app root
            });

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}