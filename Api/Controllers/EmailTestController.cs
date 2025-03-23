using Core.Common;
using Core.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailTestController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailTestController> _logger;
        private readonly SmtpSettings _smtpSettings;

        public EmailTestController(
            IEmailService emailService,
            IOptions<SmtpSettings> smtpSettings,
            ILogger<EmailTestController> logger)
        {
            _emailService = emailService;
            _logger = logger;
            _smtpSettings = smtpSettings.Value;
        }

        /// <summary>
        /// Test SMTP server connection
        /// </summary>
        [HttpGet("test-connection")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> TestConnection()
        {
            // Cast to EmailService to access the test method
            if (_emailService is EmailService service)
            {
                var (success, message) = await service.TestConnectionAsync();

                if (success)
                {
                    return Ok(ApiResponse<string>.SuccessResponse(message));
                }
                else
                {
                    return StatusCode(500, ApiResponse<string>.ErrorResponse(message));
                }
            }

            return StatusCode(500, ApiResponse<string>.ErrorResponse(
                "Could not test SMTP connection - service implementation not available"));
        }

        /// <summary>
        /// Send a test email
        /// </summary>
        [HttpPost("send-test")]
        //[Authorize(Roles = "Admin")] // Only allow admins to test
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendTestEmail([FromBody] TestEmailRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(ApiResponse<string>.ErrorResponse("Email address is required"));
            }

            _logger.LogInformation("Sending test email to {Email}", request.Email);

            // Prepare test email
            string subject = "Test Email from Your Application";
            string body = @"
            <html>
            <body>
                <h2>Test Email</h2>
                <p>This is a test email to confirm your SMTP configuration is working correctly.</p>
                <p>If you're seeing this, your email configuration is working!</p>
                <p>SMTP Settings:</p>
                <ul>
                    <li>Host: " + _smtpSettings.Host + @"</li>
                    <li>Port: " + _smtpSettings.Port + @"</li>
                    <li>User: " + _smtpSettings.User + @"</li>
                </ul>
                <p>Thank you,</p>
                <p>Your Application Team</p>
            </body>
            </html>";

            bool success = await _emailService.SendEmailAsync(request.Email, subject, body, true);

            if (success)
            {
                return Ok(ApiResponse<string>.SuccessResponse("Test email sent successfully"));
            }
            else
            {
                return StatusCode(500, ApiResponse<string>.ErrorResponse("Failed to send test email. Check server logs for details."));
            }
        }
    }

    public class TestEmailRequest
    {
        public string Email { get; set; } = string.Empty;
    }
}