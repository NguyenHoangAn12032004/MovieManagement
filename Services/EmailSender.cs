using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MovieManagement.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            var smtpServer = smtpSettings["Server"];
            var smtpPort = int.Parse(smtpSettings["Port"] ?? "587");
            var smtpUsername = smtpSettings["Username"];
            var smtpPassword = smtpSettings["Password"];
            var senderEmail = smtpSettings["SenderEmail"];
            var senderName = smtpSettings["SenderName"];
            var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");
            var useDefaultCredentials = bool.Parse(smtpSettings["UseDefaultCredentials"] ?? "false");

            // Validate configuration
            if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUsername) || 
                string.IsNullOrEmpty(smtpPassword) || string.IsNullOrEmpty(senderEmail))
            {
                _logger.LogError("SMTP configuration is incomplete. Please check SmtpSettings in appsettings.json");
                throw new InvalidOperationException("SMTP configuration is incomplete");
            }

            // Check if password is placeholder
            if (smtpPassword.Contains("PLEASE_REPLACE") || smtpPassword.Length < 10)
            {
                _logger.LogError("Gmail App Password not configured. Please set up Gmail App Password in appsettings.json");
                throw new InvalidOperationException("Gmail App Password not configured. Please follow SMTP_FIX_GUIDE.md to set up Gmail App Password.");
            }

            // Remove spaces from App Password if any
            smtpPassword = smtpPassword.Replace(" ", "");

            _logger.LogInformation($"Attempting to send email to {email} using SMTP server {smtpServer}:{smtpPort}");
            _logger.LogInformation($"Using sender email: {senderEmail}");
            _logger.LogInformation($"SSL enabled: {enableSsl}");
            _logger.LogInformation($"Password configured: {!string.IsNullOrEmpty(smtpPassword)}");

            try
            {
                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.UseDefaultCredentials = useDefaultCredentials;
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    client.EnableSsl = enableSsl;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.Timeout = 30000; // 30 seconds timeout

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail, senderName),
                        Subject = subject,
                        Body = htmlMessage,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(email);

                    _logger.LogInformation("Connecting to SMTP server...");
                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation($"Email sent successfully to {email}");
                }
            }            catch (SmtpException ex)
            {
                _logger.LogError($"SMTP Error sending email to {email}: {ex.Message}");
                _logger.LogError($"SMTP Status Code: {ex.StatusCode}");
                
                // Provide more specific error messages based on status code
                var errorMessage = ex.StatusCode switch
                {
                    SmtpStatusCode.MailboxBusy => "Gmail server is busy. Please try again later.",
                    SmtpStatusCode.MailboxUnavailable => "Recipient mailbox is unavailable.",
                    SmtpStatusCode.TransactionFailed => "Email transaction failed. This usually means your Gmail App Password is incorrect or 2-Step Verification is not enabled.",
                    SmtpStatusCode.ClientNotPermitted => "SMTP client not permitted. Please verify your Gmail App Password and ensure 2-Step Verification is enabled.",
                    SmtpStatusCode.GeneralFailure => "General SMTP failure. Check your Gmail App Password and network connection.",
                    _ => GetDetailedSmtpError(ex.Message)
                };
                
                _logger.LogError($"Suggested fix: {errorMessage}");
                throw new InvalidOperationException($"Failed to send email: {errorMessage}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error sending email to {email}: {ex.Message}");
                _logger.LogError($"Exception type: {ex.GetType().Name}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
            }
        }

        private string GetDetailedSmtpError(string originalMessage)
        {
            if (originalMessage.Contains("5.7.0") || originalMessage.Contains("Authentication Required"))
            {
                return "Gmail authentication failed. You need to use Gmail App Password instead of regular password. " +
                       "Steps: 1) Enable 2-Step Verification in Gmail, 2) Generate App Password, 3) Update appsettings.json with the 16-character App Password.";
            }
            else if (originalMessage.Contains("5.5.1") || originalMessage.Contains("Authentication unsuccessful"))
            {
                return "Gmail login credentials are incorrect. Please check your Gmail App Password in appsettings.json.";
            }
            else if (originalMessage.Contains("timeout") || originalMessage.Contains("network"))
            {
                return "Network connection issue. Check your internet connection and firewall settings.";
            }
            else if (originalMessage.Contains("SSL") || originalMessage.Contains("TLS"))
            {
                return "SSL/TLS connection issue. Gmail requires secure connection on port 587 with SSL enabled.";
            }
            else
            {
                return $"SMTP error: {originalMessage}. Please check your Gmail App Password and ensure 2-Step Verification is enabled. " +
                       "See SMTP_FIX_GUIDE.md for detailed setup instructions.";
            }
        }
    }
}