using InfraReportingSystem.ServiceAbstractions.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace InfraReportingSystem.Services.Email;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var enableSending = _configuration.GetValue("EmailSettings:EnableSending", true);

        if (!enableSending)
        {
            _logger.LogInformation("Email sending is disabled. Skipping email to {Email}.", to);
            return;
        }

        var host = _configuration["EmailSettings:Host"];
        var portValue = _configuration["EmailSettings:Port"];
        var userName = _configuration["EmailSettings:UserName"];
        var password = _configuration["EmailSettings:Password"];
        var displayName = _configuration["EmailSettings:DisplayName"];

        if (string.IsNullOrWhiteSpace(host)
            || string.IsNullOrWhiteSpace(userName)
            || string.IsNullOrWhiteSpace(password)
            || !int.TryParse(portValue, out var port))
        {
            throw new InvalidOperationException("Email settings are missing or incomplete.");
        }

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(displayName, userName));
        email.To.Add(new MailboxAddress(to, to));
        email.Subject = subject;
        email.Body = new TextPart("html") { Text = body };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(userName, password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);

        _logger.LogInformation("Email sent to {Email} with subject {Subject}.", to, subject);
    }
}
