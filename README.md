# Infrastructure Reporting System API

## Overview
The **Infrastructure Reporting System** is a comprehensive .NET 9 Web API designed to facilitate the reporting, management, and resolution of public infrastructure issues (like broken roads, pipe leaks, etc.). It serves as the back-end for a cross-platform solution (interfacing with an Angular front-end at `localhost:4200`) where public users can report issues, authorities can assign them, workers can track and complete tasks, and administrators can manage the entire system.

## Architecture
The solution follows **Clean Architecture (Onion Architecture)** principles, ensuring a separation of concerns and a highly maintainable, testable codebase.

The project is divided into the following layers:
- **Domain (`InfraReportingSystem.Domain`)**: Contains enterprise logic, core entities, and AI models.
- **Service Abstractions (`InfraReportingSystem.ServiceAbstractions`)**: Interfaces for services and repositories to ensure loose coupling.
- **Services (`InfraReportingSystem.Services`)**: Contains business logic and external integrations (like AI, Email, and Cloudinary services).
- **Persistence (`InfraReportingSystem.Persistence`)**: Data access layer containing the Entity Framework Core DbContext, migrations, and concrete repository implementations.
- **Presentation (`InfraReportingSystem.Presentation`)**: API Controllers handling HTTP requests and responses.
- **Shared (`InfraReportingSystem.Shared`)**: Common utilities and strongly-typed settings.
- **Host (`InfrastructureReportingSystem`)**: The executable API project containing configuration, dependency injection setup, and the middleware pipeline.
- **Tests (`InfrastructureReportingSystem.Tests.Unit`)**: Unit tests for ensuring reliability.

## Key Features by Role

### 👥 Public User
- **Submit Reports:** Report infrastructure issues with descriptions and images. Uses AI integration to automatically classify images and provide issue descriptions.
- **Map View:** View nearby infrastructure issues on an interactive map using geographic markers.
- **Interaction:** Support for acknowledging existing issues ("Also Suffer" functionality).
- **My Reports:** Track the status and progress of submitted reports.

### 🏛️ Authority
- **Incoming Reports:** Review and manage newly submitted reports.
- **Worker Management:** View available workers and their current workloads.
- **Assign Tasks:** Allocate infrastructure issues to specific workers for resolution.
- **Monitoring:** Monitor issue status through map views and detailed marker popups.

### 👷 Worker
- **Task Dashboard:** View currently assigned tasks and to-dos.
- **Task Execution:** Update the status of tasks currently being worked on.
- **Task History:** Review previously completed tasks.

### 🛡️ Administrator
- **Dashboard:** System-wide overview and analytics.
- **User Management:** Create, manage, and assign roles to users (Authorities, Workers, etc.).
- **Audit Logs:** Track system activities and user actions.

## Technologies Used
- **Framework:** .NET 9 Web API
- **Database:** SQL Server & Entity Framework Core
- **Authentication:** ASP.NET Core Identity & JWT (JSON Web Tokens)
- **Media Storage:** Cloudinary integration for scalable image hosting
- **AI Integrations:** Hugging Face hosted models for automated Image Classification and Description Generation
- **Documentation:** Swagger/OpenAPI with XML comments and JWT authentication support
- **CORS:** Pre-configured for the Angular frontend (`http://localhost:4200`)

## Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server
- Cloudinary Account
- IDE (Visual Studio 2022, JetBrains Rider, or VS Code)

## Getting Started

1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   ```

2. **Configure Application Settings:**
   Update `appsettings.json` or `appsettings.Development.json` in the `InfrastructureReportingSystem` project:
   - Set the `DefaultConnection` string for your SQL Server.
   - Provide your `Cloudinary` configuration details.
   - Configure `JwtSettings` with a secure `Key`, `Issuer`, and `Audience`.

3. **Database Setup:**
   The application is configured to automatically run database migrations and seed default data on startup (via `DataSeeder`, `UserSeeder`, and `TestAuditLogSeeder`). 
   Simply run the application, and the database will be initialized.

4. **Run the API:**
   ```bash
   cd InfrastructureReportingSystem
   dotnet run
   ```

5. **API Documentation:**
   Once running, navigate to the application root in your browser to view the Swagger UI and explore the endpoints:
   `https://localhost:<port>/`

## Contributing
Please follow the existing Clean Architecture structure when adding new features. Ensure that domain logic remains isolated and dependencies always flow inwards towards the Domain layer.
