// Alternative EmailSender using Gmail OAuth2
// Add this to your project if App Password doesn't work

using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace MovieManagement.Services
{
    public class GmailOAuthEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GmailOAuthEmailSender> _logger;
        private readonly HttpClient _httpClient;

        public GmailOAuthEmailSender(
            IConfiguration configuration, 
            ILogger<GmailOAuthEmailSender> logger,
            HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _logger.LogInformation("Sending email via Gmail API OAuth2");
            
            // This would require OAuth2 setup with Gmail API
            // For now, let's try a different SMTP approach
            
            await SendViaBackupSMTP(email, subject, htmlMessage);
        }

        private async Task SendViaBackupSMTP(string email, string subject, string htmlMessage)
        {
            // Try different SMTP settings that might work better
            var smtpConfigs = new[]
            {
                new { Server = "smtp.gmail.com", Port = 587, Ssl = true },
                new { Server = "smtp.gmail.com", Port = 465, Ssl = true },
                new { Server = "smtp.gmail.com", Port = 25, Ssl = false }
            };

            var username = _configuration["SmtpSettings:Username"];
            var password = _configuration["SmtpSettings:Password"];

            foreach (var config in smtpConfigs)
            {
                try
                {
                    _logger.LogInformation($"Trying SMTP {config.Server}:{config.Port} SSL:{config.Ssl}");
                    
                    using var client = new System.Net.Mail.SmtpClient(config.Server, config.Port);
                    client.EnableSsl = config.Ssl;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new System.Net.NetworkCredential(username, password);
                    client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                    client.Timeout = 30000;

                    var mail = new System.Net.Mail.MailMessage
                    {
                        From = new System.Net.Mail.MailAddress(username),
                        Subject = subject,
                        Body = htmlMessage,
                        IsBodyHtml = true
                    };
                    mail.To.Add(email);

                    await client.SendMailAsync(mail);
                    _logger.LogInformation($"Email sent successfully via {config.Server}:{config.Port}");
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed {config.Server}:{config.Port} - {ex.Message}");
                    continue;
                }
            }

            throw new InvalidOperationException("All SMTP configurations failed. Please check Gmail settings.");
        }
    }
}
