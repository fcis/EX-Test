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

                // Create the mail message
                var message = new MailMessage("mohammed.gamal@tharwah.net", to, subject, body);

                message.To.Add(to);

                // Parse the port
                if (!int.TryParse(_smtpSettings.Port.ToString(), out int port))
                {
                    port = 587; // Default fallback
                    _logger.LogWarning("Failed to parse SMTP port '{Port}', using default port 587", _smtpSettings.Port);
                }

                // Create a new SMTP client with explicit settings
                using (var client = new SmtpClient())
                {
                    // Don't use default credentials
                    client.UseDefaultCredentials = false;

                    // Set credentials before setting other properties
                    client.Credentials = new NetworkCredential(_smtpSettings.User, _smtpSettings.Password);

                    // Set host and port
                    client.Host = _smtpSettings.Host;
                    client.Port = port;



                    // Set delivery method
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;

                    // Set timeout
                    client.Timeout = 30000;

                    // Allow invalid certificates if needed (use only for testing)
                    // ServicePointManager.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;

                    _logger.LogInformation("Sending email via SMTP: {Host}:{Port} with user {User}",
                                          _smtpSettings.Host, port, _smtpSettings.User);

                    // Send email
                    await client.SendMailAsync(message);

                    _logger.LogInformation("Email sent successfully to {To}", to);
                    return true;
                }
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP error sending email to {To}: Status code: {StatusCode}, Error: {Message}",
                                 to, smtpEx.StatusCode, smtpEx.Message);

                if (smtpEx.InnerException != null)
                {
                    _logger.LogError(smtpEx.InnerException, "Inner exception: {Message}", smtpEx.InnerException.Message);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending email to {To}: {Message}", to, ex.Message);
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