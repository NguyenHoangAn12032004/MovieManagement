using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace MovieManagement.Services
{
    public class DeveloperEmailSender : IEmailSender
    {
        private readonly ILogger<DeveloperEmailSender> _logger;

        public DeveloperEmailSender(ILogger<DeveloperEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Trong môi trường development, chỉ log email thay vì gửi thật
            _logger.LogInformation("=== EMAIL WOULD BE SENT ===");
            _logger.LogInformation($"To: {email}");
            _logger.LogInformation($"Subject: {subject}");
            _logger.LogInformation($"Body: {htmlMessage}");
            _logger.LogInformation("=== END EMAIL ===");

            // Simulate success
            return Task.CompletedTask;
        }
    }

    public class SmtpTestService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpTestService> _logger;

        public SmtpTestService(IConfiguration configuration, ILogger<SmtpTestService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> TestSmtpConnectionAsync()
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                var smtpServer = smtpSettings["Server"];
                var smtpPort = int.Parse(smtpSettings["Port"]);
                var smtpUsername = smtpSettings["Username"];
                var smtpPassword = smtpSettings["Password"];

                _logger.LogInformation($"Testing SMTP connection to {smtpServer}:{smtpPort}");
                _logger.LogInformation($"Username: {smtpUsername}");

                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    client.EnableSsl = true;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.Timeout = 10000; // 10 seconds for testing

                    // Test by sending a simple test email to the sender itself
                    var testMessage = new MailMessage
                    {
                        From = new MailAddress(smtpUsername, "Test"),
                        Subject = "SMTP Test Email",
                        Body = "This is a test email to verify SMTP configuration.",
                        IsBodyHtml = false
                    };
                    testMessage.To.Add(smtpUsername);

                    await client.SendMailAsync(testMessage);
                    _logger.LogInformation("SMTP test successful!");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"SMTP test failed: {ex.Message}");
                return false;
            }
        }
    }
}
