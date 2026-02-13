using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EduShelf.Api.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        var host = emailSettings["SmtpHost"];
        var user = emailSettings["SmtpUser"];
        var pass = emailSettings["SmtpPass"];
        var from = emailSettings["FromEmail"];

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            // Log warning or throw depending on strictness. For now, we return if not configured.
            _logger.LogWarning("SMTP not configured. Skipping email.");
            return;
        }

        if (!int.TryParse(emailSettings["SmtpPort"], out var port))
        {
            port = 587;
        }

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(user, pass),
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(!string.IsNullOrEmpty(from) && from.Contains("@") ? from : "noreply@edushelf.com"),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        mailMessage.To.Add(to);

        await client.SendMailAsync(mailMessage);
    }
}
