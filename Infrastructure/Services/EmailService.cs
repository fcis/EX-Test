using Core.Common;
using Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly SmtpSettings _smtpSettings;

        public EmailService(
            IOptions<SmtpSettings> smtpSettings,
            ILogger<EmailService> logger)
        {
            _logger = logger;
            _smtpSettings = smtpSettings.Value;

            // Log SMTP settings (exclude password for security)
            _logger.LogInformation("Email service initialized with: Host={Host}, Port={Port}, User={User}",
                _smtpSettings.Host, _smtpSettings.Port, _smtpSettings.User);
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            try
            {
                _logger.LogInformation("Attempting to send email to {To} with subject: {Subject}", to, subject);

                // Parse port as int (in case it's stored as string in config)
                if (!int.TryParse(_smtpSettings.Port.ToString(), out int port))
                {
                    port = 587; // Default fallback
                    _logger.LogWarning("Failed to parse SMTP port '{Port}', using default port 587", _smtpSettings.Port);
                }

                // Configure the SMTP client with more detailed options
                using var client = new SmtpClient()
                {
                    Host = _smtpSettings.Host,
                    Port = port,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_smtpSettings.User, _smtpSettings.Password),
                    Timeout = 30000 // 30 seconds timeout
                };

                // Override SSL certificate validation if necessary (optional, uncomment if needed)
                // ServicePointManager.ServerCertificateValidationCallback = 
                //    delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

                var message = new MailMessage
                {
                    From = new MailAddress(_smtpSettings.User),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                message.To.Add(to);

                // Set priority
                message.Priority = MailPriority.High;

                _logger.LogInformation("Sending email via SMTP: {Host}:{Port}", _smtpSettings.Host, port);
                await client.SendMailAsync(message);

                _logger.LogInformation("Email sent successfully to {To}", to);
                return true;
            }
            catch (SmtpException smtpEx)
            {
                // Handle specific SMTP exceptions with detailed logging
                string errorDetails = GetDetailedSmtpError(smtpEx);
                _logger.LogError(smtpEx, "SMTP error sending email to {To}: {ErrorDetails}", to, errorDetails);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending email to {To} with subject {Subject}", to, subject);
                return false;
            }
        }

        private string GetDetailedSmtpError(SmtpException ex)
        {
            string details = $"Status code: {ex.StatusCode}, ";

            switch (ex.StatusCode)
            {
                case SmtpStatusCode.ServiceNotAvailable:
                    details += "SMTP service is not available. Check server address and port.";
                    break;
                case SmtpStatusCode.MailboxBusy:
                    details += "Recipient mailbox is busy.";
                    break;
                case SmtpStatusCode.MailboxUnavailable:
                    details += "Recipient mailbox is unavailable.";
                    break;
                case SmtpStatusCode.ClientNotPermitted:
                    details += "Client not permitted. Authentication may be required.";
                    break;

                default:
                    details += $"Error: {ex.Message}";
                    break;
            }

            if (ex.InnerException != null)
            {
                details += $" Inner error: {ex.InnerException.Message}";
            }

            return details;
        }

        // Add a test method to verify SMTP settings
        public async Task<(bool Success, string Message)> TestConnectionAsync()
        {
            try
            {
                // Parse port as int
                if (!int.TryParse(_smtpSettings.Port.ToString(), out int port))
                {
                    port = 587;
                }

                // Just test the connection, don't send an actual email
                using var client = new SmtpClient(_smtpSettings.Host, port)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_smtpSettings.User, _smtpSettings.Password),
                    Timeout = 10000 // Short timeout for testing
                };

                // Connect to the server
                client.ServicePoint.MaxIdleTime = 1;
                var connectionSuccess = client.ServicePoint.ConnectionLimit > 0;

                return (true, $"Successfully connected to SMTP server {_smtpSettings.Host}:{port}");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to connect to SMTP server: {ex.Message}");
            }
        }
    }
}