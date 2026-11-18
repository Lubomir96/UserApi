
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using UserApi.Interfaces;

namespace UserApi.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(
        string toEmail,
        string subject,
        string body)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");

        var senderEmail = emailSettings["SenderEmail"];
        var senderName = emailSettings["SenderName"];
        var username = emailSettings["Username"];
        var password = emailSettings["Password"];
        var smtpServer = emailSettings["SmtpServer"];
        var port = int.Parse(emailSettings["Port"] ?? "587");

        if (string.IsNullOrWhiteSpace(senderEmail) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(smtpServer))
        {
            throw new InvalidOperationException(
                "Email settings are missing.");
        }

        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(senderName, senderEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        message.Body = new TextPart("plain")
        {
            Text = body
        };

        using var client = new SmtpClient();
        client.CheckCertificateRevocation = false;

        await client.ConnectAsync(
            smtpServer,
            port,
            SecureSocketOptions.StartTls);

        await client.AuthenticateAsync(username, password);

        await client.SendAsync(message);

        await client.DisconnectAsync(true);
    }
}
