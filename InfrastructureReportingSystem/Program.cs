using System.Text;
using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Persistence.Data;
using InfraReportingSystem.Persistence.Repositories.Admin.AuditLogsScreen;
using InfraReportingSystem.Persistence.Repositories.Admin.UsersManagementScreen;
using InfraReportingSystem.Persistence.Repositories.Auth;
using InfraReportingSystem.Persistence.Repositories.Authority.AssignWorker;
using InfraReportingSystem.Persistence.Repositories.Authority.IncomingReports;
using InfraReportingSystem.Persistence.Repositories.Authority.Workers;
using InfraReportingSystem.Persistence.Repositories.PublicUser.AlsoSuffer;
using InfraReportingSystem.Persistence.Repositories.PublicUser.Map;
using InfraReportingSystem.Persistence.Repositories.PublicUser.MarkerPopup;
using InfraReportingSystem.Persistence.Repositories.PublicUser.NearbyReports;
using InfraReportingSystem.Persistence.Repositories.Worker.CurrentTaskScreen;
using InfraReportingSystem.Persistence.Repositories.Worker.TasksHistoryScreen;
using InfraReportingSystem.Persistence.Repositories.Worker.TasksScreen;
using InfraReportingSystem.Persistence.Seed;
using InfraReportingSystem.ServiceAbstractions.Admin.AuditLogsScreen;
using InfraReportingSystem.ServiceAbstractions.Admin.UserCreationScreen;
using InfraReportingSystem.ServiceAbstractions.Admin.UsersManagementScreen;
using InfraReportingSystem.ServiceAbstractions.Auth;
using InfraReportingSystem.ServiceAbstractions.Authority;
using InfraReportingSystem.ServiceAbstractions.Authority.AssignWorker;
using InfraReportingSystem.ServiceAbstractions.Authority.IncomingReports;
using InfraReportingSystem.ServiceAbstractions.Email;
using InfraReportingSystem.ServiceAbstractions.PublicUser.AlsoSuffer;
using InfraReportingSystem.ServiceAbstractions.PublicUser.Map;
using InfraReportingSystem.ServiceAbstractions.PublicUser.MarkerPopup;
using InfraReportingSystem.ServiceAbstractions.PublicUser.NearbyReports;
using InfraReportingSystem.ServiceAbstractions.Repositories;
using InfraReportingSystem.ServiceAbstractions.Repositories.Admin.AuditLogsScreen;
using InfraReportingSystem.ServiceAbstractions.Repositories.Admin.UsersManagementScreen;
using InfraReportingSystem.ServiceAbstractions.Repositories.Auth;
using InfraReportingSystem.ServiceAbstractions.Repositories.Authority.AssignWorker;
using InfraReportingSystem.ServiceAbstractions.Repositories.Authority.Workers;
using InfraReportingSystem.ServiceAbstractions.Repositories.PublicUser.AlsoSuffer;
using InfraReportingSystem.ServiceAbstractions.Repositories.PublicUser.Map;
using InfraReportingSystem.ServiceAbstractions.Repositories.PublicUser.MarkerPopup;
using InfraReportingSystem.ServiceAbstractions.Repositories.PublicUser.NearbyReports;
using InfraReportingSystem.ServiceAbstractions.Repositories.Worker.CurrentTaskScreen;
using InfraReportingSystem.ServiceAbstractions.Repositories.Worker.TasksHistoryScreen;
using InfraReportingSystem.ServiceAbstractions.Repositories.Worker.TasksScreen;
using InfraReportingSystem.ServiceAbstractions.Worker.CurrentTaskScreen;
using InfraReportingSystem.ServiceAbstractions.Worker.TasksHistoryScreen;
using InfraReportingSystem.ServiceAbstractions.Worker.TasksScreen;
using InfraReportingSystem.Services.Admin.AuditLogsScreen;
using InfraReportingSystem.Services.Admin.UserCreationScreen;
using InfraReportingSystem.Services.Admin.UsersManagementScreen;
using InfraReportingSystem.Services.Auth;
using InfraReportingSystem.Services.Authority;
using InfraReportingSystem.Services.Authority.AssignWorker;
using InfraReportingSystem.Services.Authority.IncomingReports;
using InfraReportingSystem.Services.Email;
using InfraReportingSystem.Services.PublicUser.AlsoSuffer;
using InfraReportingSystem.Services.PublicUser.Map;
using InfraReportingSystem.Services.PublicUser.MarkerPopup;
using InfraReportingSystem.Services.PublicUser.NearbyReports;
using InfraReportingSystem.Services.Worker.CurrentTaskScreen;
using InfraReportingSystem.Services.Worker.TasksHistoryScreen;
using InfraReportingSystem.Services.Worker.TasksScreen;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

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
            
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header
                });
            
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                sqloptions =>
                {
                    sqloptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null
                        );
                }).LogTo(Console.WriteLine, LogLevel.Warning)
                );

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
            var key = jwtSettings["Key"];
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("JWT settings key is missing. Configure JwtSettings:Key before running the API.");

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
            builder.Services.AddScoped<IAdminUserCreationService, AdminUserCreationService>();
            builder.Services.AddScoped<IAuthorityIncomingReportsRepository, AuthorityIncomingReportsRepository>();
            builder.Services.AddScoped<IAuthorityIncomingReportsService, AuthorityIncomingReportsService>();
            builder.Services.AddScoped<IAuthorityWorkersRepository, AuthorityWorkersRepository>();
            builder.Services.AddScoped<IAuthorityWorkersService, AuthorityWorkersService>();
            builder.Services.AddScoped<IAuthorityAssignWorkerRepository, AuthorityAssignWorkerRepository>();
            builder.Services.AddScoped<IAuthorityAssignWorkerService, AuthorityAssignWorkerService>();
            builder.Services.AddScoped<IAdminAuditLogsRepository, AdminAuditLogsRepository>();
            builder.Services.AddScoped<IAdminAuditLogsService, AdminAuditLogsService>();
            builder.Services.AddScoped<IPublicMapRepository, PublicMapRepository>();
            builder.Services.AddScoped<IPublicMapService, PublicMapService>();
            builder.Services.AddScoped<IPublicMarkerPopupRepository, PublicMarkerPopupRepository>();
            builder.Services.AddScoped<IPublicMarkerPopupService, PublicMarkerPopupService>();
            builder.Services.AddScoped<IOtpRepository, OtpRepository>();
            builder.Services.AddScoped<IOtpService, OtpService>();
            builder.Services.AddScoped<IPublicNearbyReportsRepository, PublicNearbyReportsRepository>();
            builder.Services.AddScoped<IPublicNearbyReportsService, PublicNearbyReportsService>();
            builder.Services.AddScoped<IPublicAlsoSufferRepository, PublicAlsoSufferRepository>();
            builder.Services.AddScoped<IPublicAlsoSufferService, PublicAlsoSufferService>();

            // Register DataSeeder
            builder.Services.AddScoped<DataSeeder>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:4200")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            // Database Migration and Seeding Pipeline
            using (var scope = app.Services.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

                try
                {
                    logger.LogInformation("Starting database migration and seeding...");

                    // Run Migrations (via your existing DataSeeder)
                    var dbSeeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
                    await dbSeeder.SeedAsync(); // Assuming this calls context.Database.MigrateAsync()
                    logger.LogInformation("DataSeeder completed successfully.");

                    await UserSeeder.SeedAsync(app.Services);
                    logger.LogInformation("UserSeeder completed successfully.");


                    await TestAuditLogSeeder.SeedAsync(app.Services);
                    logger.LogInformation("TestAuditLogSeeder completed successfully.");


                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred during database migration or seeding. App will continue running.");
                }
            }

            // make the swagger UI public for testing (temporary)
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Infra Reporting API V1");
                c.RoutePrefix = string.Empty; // serve at app root
            });

            app.UseHttpsRedirection();
            app.UseCors("AllowFrontend");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
