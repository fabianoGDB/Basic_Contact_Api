using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_configuration["EmailSettings:SenderName"], _configuration["EmailSettings:SenderEmail"]));
        email.To.Add(new MailboxAddress(toEmail));
        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Html) { Text = message };

        using var smtp = new SmtpClient();
        smtp.Connect(_configuration["EmailSettings:SmtpServer"], int.Parse(_configuration["EmailSettings:SmtpPort"]), MailKit.Security.SecureSocketOptions.StartTls);
        smtp.Authenticate(_configuration["EmailSettings:Username"], _configuration["EmailSettings:Password"]);

        await smtp.SendAsync(email);
        smtp.Disconnect(true);
    }
}
