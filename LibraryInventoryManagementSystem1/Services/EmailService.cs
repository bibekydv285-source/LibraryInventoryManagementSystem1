using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace LibraryInventoryManagementSystem1.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpHost = _config["EmailSettings:SmtpHost"];
            if (string.IsNullOrWhiteSpace(smtpHost))
                throw new InvalidOperationException("EmailSettings:SmtpHost is not configured.");

            if (!int.TryParse(_config["EmailSettings:SmtpPort"], out var smtpPort))
                throw new InvalidOperationException("EmailSettings:SmtpPort is missing or invalid.");

            var senderEmail = _config["EmailSettings:SenderEmail"];
            var senderPassword = _config["EmailSettings:SenderPassword"];
            if (string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(senderPassword))
                throw new InvalidOperationException("EmailSettings:SenderEmail or SenderPassword is not configured.");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Library Inventory Management System", senderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();
            try
            {
                // Explicitly authenticate with provided credentials (do not rely on any default credentials)
                await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(senderEmail, senderPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (MailKit.ServiceNotConnectedException ex)
            {
                throw new InvalidOperationException($"Failed to connect to SMTP host '{smtpHost}': {ex.Message}", ex);
            }
            catch (AuthenticationException ex)
            {
                throw new InvalidOperationException($"SMTP authentication failed for user '{senderEmail}': {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to send email via SMTP host '{smtpHost}': {ex.Message}", ex);
            }
        }
    }
}