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

        MailboxAddress recipient;
        try
        {
            recipient = MailboxAddress.Parse(to);
        }
        catch (ParseException ex)
        {
            throw new InvalidOperationException($"Cannot send email because '{to}' is not a valid email address.", ex);
        }

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(displayName ?? userName, userName));
        email.To.Add(recipient);
        email.Subject = subject;
        email.Body = new TextPart("html") { Text = body };

        using var smtp = new SmtpClient();
        try
        {
            await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(userName, password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
        catch (MailKit.Security.AuthenticationException ex)
        {
            _logger.LogError(ex, "SMTP authentication failed for {UserName}. Check EmailSettings:UserName and the Gmail App Password.", userName);
            throw new InvalidOperationException("Email authentication failed. For Gmail SMTP, EmailSettings:Password must be a 16-character App Password, not the normal Gmail password.", ex);
        }
        catch (SmtpCommandException ex)
        {
            _logger.LogError(ex, "SMTP command failed while sending email to {Email}. StatusCode: {StatusCode}", to, ex.StatusCode);
            throw new InvalidOperationException($"SMTP rejected the email request: {ex.Message}", ex);
        }
        catch (SmtpProtocolException ex)
        {
            _logger.LogError(ex, "SMTP protocol error while sending email to {Email}.", to);
            throw new InvalidOperationException("SMTP connection failed because the server response could not be understood.", ex);
        }

        _logger.LogInformation("Email sent to {Email} with subject {Subject}.", to, subject);
    }
}
