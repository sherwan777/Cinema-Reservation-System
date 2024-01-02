/*using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using System.Text;
using MimeKit;

public class EmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public void SendEmail(string toEmail, string subject, string body, byte[] attachment = null, string attachmentName = "")
    {
        try
        {
            var service = GetGmailService();
            var emailMessage = CreateEmailMessage(toEmail, subject, body, attachment, attachmentName);
            var request = service.Users.Messages.Send(emailMessage, "me");
            request.Execute();
            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Email}", toEmail);
        }
    }

    private GmailService GetGmailService()
    {
        string[] Scopes = { GmailService.Scope.GmailSend };
        UserCredential credential;

        using (var stream = new FileStream("client_secret_552409561077-hg81v0l7p98tr3uth3ia1rbi3dl2dugd.apps.googleusercontent.com.json", FileMode.Open, FileAccess.Read))
        {
            string credPath = "token.json";
            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true)).Result;
            _logger.LogInformation("Credential file saved to: " + credPath);
        }

        return new GmailService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "TicketFlix",
        });
    }

    private Message CreateEmailMessage(string toEmail, string subject, string body, byte[] attachment = null, string attachmentName = "")
    {
        var mailMessage = new MimeMessage();
        mailMessage.From.Add(new MailboxAddress("Ticket Flix Team", "abdeali.hazari@gmail.com"));
        mailMessage.To.Add(new MailboxAddress("", toEmail));
        mailMessage.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = body };
        if (attachment != null)
        {
            bodyBuilder.Attachments.Add(attachmentName, attachment);
        }
        mailMessage.Body = bodyBuilder.ToMessageBody();

        return new Message
        {
            Raw = Base64UrlEncode(mailMessage.ToString())
        };
    }


    private static string Base64UrlEncode(string input)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(inputBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
}*/

using System.Net.Mail;
using System.Net;

public class EmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public void SendEmail(string toEmail, string subject, string body, byte[] attachment = null, string attachmentName = "")
    {
        try
        {
            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("0c11c80afdbf7e", "320cb1a4933ed0"),
                EnableSsl = true
            };

            // Create a MailMessage object
            var mailMessage = new MailMessage
            {
                From = new MailAddress("abdeali.hazari@gmail.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(new MailAddress(toEmail));

            // Check if there's an attachment
            if (attachment != null && attachmentName != null)
            {
                // Do not use using here, as we do not want to dispose the stream yet
                var stream = new MemoryStream(attachment);
                mailMessage.Attachments.Add(new Attachment(stream, attachmentName, "image/png")); 
            }

            // Send the email
            client.Send(mailMessage);

            // Dispose of the mailMessage (and its attachments) manually
            mailMessage.Dispose();

            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Email}", toEmail);
        }
    }


}
