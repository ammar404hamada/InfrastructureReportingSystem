using InfraReportingSystem.ServiceAbstractions.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace InfraReportingSystem.Services.Email;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var host = _configuration["EmailSettings:Host"];
        var port = int.Parse(_configuration["EmailSettings:Port"]!);
        var userName = _configuration["EmailSettings:UserName"];
        var password = _configuration["EmailSettings:Password"];
        var displayName = _configuration["EmailSettings:DisplayName"];

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
    }
}