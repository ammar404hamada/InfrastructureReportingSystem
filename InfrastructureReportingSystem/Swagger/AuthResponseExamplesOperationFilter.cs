using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace InfrastructureReportingSystem.Swagger;

public class AuthResponseExamplesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.ActionDescriptor.RouteValues["controller"] != "Auth")
            return;

        var actionName = context.ApiDescription.ActionDescriptor.RouteValues["action"];

        foreach (var response in operation.Responses)
        {
            var example = GetExample(actionName, response.Key);
            if (example is null)
                continue;

            foreach (var content in response.Value.Content.Values)
            {
                content.Example = example;
            }
        }
    }

    private static IOpenApiAny? GetExample(string? actionName, string statusCode)
    {
        return actionName switch
        {
            "Register" when statusCode == "200" => RegisterSuccessExample(),
            "Register" when statusCode == "400" => ErrorExample("Email already exists."),

            "Login" when statusCode == "200" => LoginSuccessExample(),
            "Login" when statusCode == "401" => LoginFailedExample(),

            "RefreshToken" when statusCode == "200" => RefreshTokenSuccessExample(),
            "RefreshToken" when statusCode == "401" => ErrorExample("No refresh token found"),

            "ConfirmEmail" when statusCode == "200" => SuccessMessageExample("Email confirmed successfully."),
            "ConfirmEmail" when statusCode == "400" => ErrorExample("Invalid or expired OTP code."),

            "ForgotPassword" when statusCode == "200" =>
                SuccessMessageExample("If this email exists, you will receive a password reset code."),

            "ResetPassword" when statusCode == "200" => SuccessMessageExample("Password reset successfully."),
            "ResetPassword" when statusCode == "400" => ErrorExample("Invalid or expired OTP."),

            "ResendConfirmation" when statusCode == "200" => SuccessMessageExample("Verification code sent successfully."),
            "ResendConfirmation" when statusCode == "400" =>
                ErrorExample("Invalid email or the account is already confirmed."),

            "Logout" when statusCode == "200" => SuccessMessageExample("Logged out successfully."),

            _ => null
        };
    }

    private static OpenApiObject RegisterSuccessExample()
    {
        return new OpenApiObject
        {
            ["success"] = new OpenApiBoolean(true),
            ["message"] = new OpenApiString("Registration successful. Please confirm your email."),
            ["user"] = UserExample()
        };
    }

    private static OpenApiObject LoginSuccessExample()
    {
        return new OpenApiObject
        {
            ["success"] = new OpenApiBoolean(true),
            ["message"] = new OpenApiString("Login successful."),
            ["user"] = UserExample(),
            ["token"] = new OpenApiString("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.example-token"),
            ["refreshToken"] = new OpenApiNull(),
            ["refreshTokenExpiresOn"] = new OpenApiString("2026-05-22T22:00:54.342Z")
        };
    }

    private static OpenApiObject RefreshTokenSuccessExample()
    {
        return new OpenApiObject
        {
            ["success"] = new OpenApiBoolean(true),
            ["message"] = new OpenApiString("New refresh token generated successflly"),
            ["user"] = new OpenApiNull(),
            ["token"] = new OpenApiString("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.new-example-token"),
            ["refreshToken"] = new OpenApiNull(),
            ["refreshTokenExpiresOn"] = new OpenApiString("2026-05-22T22:00:54.342Z")
        };
    }

    private static OpenApiObject LoginFailedExample()
    {
        return new OpenApiObject
        {
            ["success"] = new OpenApiBoolean(false),
            ["message"] = new OpenApiString("Invalid password."),
            ["user"] = new OpenApiNull(),
            ["token"] = new OpenApiNull(),
            ["refreshToken"] = new OpenApiNull(),
            ["refreshTokenExpiresOn"] = new OpenApiNull()
        };
    }

    private static OpenApiObject SuccessMessageExample(string message)
    {
        return new OpenApiObject
        {
            ["success"] = new OpenApiBoolean(true),
            ["message"] = new OpenApiString(message)
        };
    }

    private static OpenApiObject ErrorExample(string message)
    {
        return new OpenApiObject
        {
            ["success"] = new OpenApiBoolean(false),
            ["message"] = new OpenApiString(message)
        };
    }

    private static OpenApiObject UserExample()
    {
        return new OpenApiObject
        {
            ["name"] = new OpenApiString("Mostafa Ahmed"),
            ["email"] = new OpenApiString("mostafa@example.com"),
            ["role"] = new OpenApiString("PublicUser")
        };
    }
}
