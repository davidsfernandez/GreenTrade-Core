using System.Net;
using System.Net.Mail;

namespace GreenTrade.Server.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "localhost";
        var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "1025");
        var senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "no-reply@greentrade.com";
        var senderName = _configuration["EmailSettings:SenderName"] ?? "GreenTrade Support";

        using var client = new SmtpClient(smtpServer, smtpPort);
        // Mailpit doesn't require auth by default
        client.UseDefaultCredentials = true; 
        
        var mailMessage = new MailMessage
        {
            From = new MailAddress(senderEmail, senderName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
        };
        mailMessage.To.Add(to);

        await client.SendMailAsync(mailMessage);
    }
}
