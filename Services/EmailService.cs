using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

public class EmailService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SmtpSettings> smtpSettings, ILogger<EmailService> logger)
    {
        _smtpSettings = smtpSettings.Value;
        _logger = logger;
        LogSmtpSettings(); // Log SMTP settings at startup
    }

    private void LogSmtpSettings()
    {
        _logger.LogInformation("SMTP Host: {Host}", _smtpSettings.Host);
        _logger.LogInformation("SMTP Port: {Port}", _smtpSettings.Port);
        _logger.LogInformation("SMTP Username: {Username}", _smtpSettings.Username);
        
    }

    public void SendEmail(string toEmail, string subject, string body)
    {
        try
        {
            var fromAddress = new MailAddress(_smtpSettings.Username, "Ticketflix");
            var toAddress = new MailAddress(toEmail);
            var smtp = new SmtpClient
            {
                Host = _smtpSettings.Host,
                Port = _smtpSettings.Port,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, _smtpSettings.Password)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Email}", toEmail);
        }
    }
}
