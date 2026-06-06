using InfraReportingSystem.Domain.Entities;
using InfraReportingSystem.Persistence.Data;
using InfraReportingSystem.Persistence.Repositories.Shared.Auth;
using InfraReportingSystem.Persistence.Repositories.Shared.Profile;
using InfraReportingSystem.Persistence.Repositories.Shared.Profile.ProfileManagement;
using InfraReportingSystem.Persistence.Repositories.Users.Admin.AuditLogsScreen;
using InfraReportingSystem.Persistence.Repositories.Users.Admin.UsersManagementScreen;
using InfraReportingSystem.Persistence.Repositories.Users.Authority.AssignWorker;
using InfraReportingSystem.Persistence.Repositories.Users.Authority.IncomingReports;
using InfraReportingSystem.Persistence.Repositories.Users.Authority.Map;
using InfraReportingSystem.Persistence.Repositories.Users.Authority.MarkerPopup;
using InfraReportingSystem.Persistence.Repositories.Users.Authority.Workers;
using InfraReportingSystem.Persistence.Repositories.Users.PublicUser.AlsoSuffer;
using InfraReportingSystem.Persistence.Repositories.Users.PublicUser.Map;
using InfraReportingSystem.Persistence.Repositories.Users.PublicUser.MarkerPopup;
using InfraReportingSystem.Persistence.Repositories.Users.PublicUser.NearbyReports;
using InfraReportingSystem.Persistence.Repositories.Users.Worker.CurrentTaskScreen;
using InfraReportingSystem.Persistence.Repositories.Users.Worker.TasksHistoryScreen;
using InfraReportingSystem.Persistence.Repositories.Users.Worker.TasksScreen;
using InfraReportingSystem.Persistence.Seed;
using InfraReportingSystem.ServiceAbstractions.Repositories.Shared;
using InfraReportingSystem.ServiceAbstractions.Repositories.Shared.Auth;
using InfraReportingSystem.ServiceAbstractions.Repositories.Shared.Profile;
using InfraReportingSystem.ServiceAbstractions.Repositories.Shared.Profile.ProfileManagement;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Admin.AuditLogsScreen;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Admin.UsersManagementScreen;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Authority.AssignWorker;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Authority.IncomingReports;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Authority.Map;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Authority.MarkerPopup;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Authority.Workers;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.PublicUser.AlsoSuffer;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.PublicUser.Map;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.PublicUser.MarkerPopup;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.PublicUser.NearbyReports;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Worker.CurrentTaskScreen;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Worker.TasksHistoryScreen;
using InfraReportingSystem.ServiceAbstractions.Repositories.Users.Worker.TasksScreen;
using InfraReportingSystem.ServiceAbstractions.Shared.Auth;
using InfraReportingSystem.ServiceAbstractions.Shared.Email;
using InfraReportingSystem.ServiceAbstractions.Shared.Images;
using InfraReportingSystem.ServiceAbstractions.Profile;
using InfraReportingSystem.ServiceAbstractions.Shared.Profile.ProfileManagement;
using InfraReportingSystem.ServiceAbstractions.Users.Admin.AuditLogsScreen;
using InfraReportingSystem.ServiceAbstractions.Users.Admin.UserCreationScreen;
using InfraReportingSystem.ServiceAbstractions.Users.Admin.UsersManagementScreen;
using InfraReportingSystem.ServiceAbstractions.Users.Authority.AssignWorker;
using InfraReportingSystem.ServiceAbstractions.Users.Authority.IncomingReports;
using InfraReportingSystem.ServiceAbstractions.Users.Authority.Map;
using InfraReportingSystem.ServiceAbstractions.Users.Authority.MarkerPopup;
using InfraReportingSystem.ServiceAbstractions.Users.Authority.Workers;
using InfraReportingSystem.ServiceAbstractions.Users.PublicUser.AlsoSuffer;
using InfraReportingSystem.ServiceAbstractions.Users.PublicUser.Map;
using InfraReportingSystem.ServiceAbstractions.Users.PublicUser.MarkerPopup;
using InfraReportingSystem.ServiceAbstractions.Users.PublicUser.NearbyReports;
using InfraReportingSystem.ServiceAbstractions.Users.Worker.CurrentTaskScreen;
using InfraReportingSystem.ServiceAbstractions.Users.Worker.TasksHistoryScreen;
using InfraReportingSystem.ServiceAbstractions.Users.Worker.TasksScreen;
using InfraReportingSystem.Services.Shared.Auth;
using InfraReportingSystem.Services.Shared.Email;
using InfraReportingSystem.Services.Shared.Images;
using InfraReportingSystem.Services.Profile;
using InfraReportingSystem.Services.Shared.Profile.ProfileManagement;
using InfraReportingSystem.Services.Users.Admin.AuditLogsScreen;
using InfraReportingSystem.Services.Users.Admin.UserCreationScreen;
using InfraReportingSystem.Services.Users.Admin.UsersManagementScreen;
using InfraReportingSystem.Services.Users.Authority.AssignWorker;
using InfraReportingSystem.Services.Users.Authority.IncomingReports;
using InfraReportingSystem.Services.Users.Authority.Map;
using InfraReportingSystem.Services.Users.Authority.MarkerPopup;
using InfraReportingSystem.Services.Users.Authority.Workers;
using InfraReportingSystem.Services.Users.PublicUser.AlsoSuffer;
using InfraReportingSystem.Services.Users.PublicUser.Map;
using InfraReportingSystem.Services.Users.PublicUser.MarkerPopup;
using InfraReportingSystem.Services.Users.PublicUser.NearbyReports;
using InfraReportingSystem.Services.Users.Worker.CurrentTaskScreen;
using InfraReportingSystem.Services.Users.Worker.TasksHistoryScreen;
using InfraReportingSystem.Services.Users.Worker.TasksScreen;
using InfraReportingSystem.Shared.Settings;
using InfrastructureReportingSystem.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
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

            
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 10 * 1024 * 1024;
            });


            // Add services to the container.
            builder.Services.AddControllers();

            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 10485760;
            });


            // add mybelove swagger
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Infra Reporting API", Version = "v1" });
                var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                c.OperationFilter<AuthResponseExamplesOperationFilter>();
            
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
            builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));
            builder.Services.AddScoped<IImageService, ImageService>();
            builder.Services.AddScoped<IProfileManagementRepository, ProfileManagementRepository>();
            builder.Services.AddScoped<IProfileManagementService, ProfileManagementService>();

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
            builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
            builder.Services.AddScoped<IProfileService, ProfileService>();

            
            builder.Services.AddScoped<IAuthorityMapRepository, AuthorityMapRepository>();
            builder.Services.AddScoped<IAuthorityMapService, AuthorityMapService>();
            builder.Services.AddScoped<IAuthorityMarkerPopupRepository, AuthorityMarkerPopupRepository>();
            builder.Services.AddScoped<IAuthorityMarkerPopupService, AuthorityMarkerPopupService>();



            // Register DataSeeder
            builder.Services.AddScoped<DataSeeder>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:4200")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
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
