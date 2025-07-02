using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using Shared.Models;
using System.Threading.Tasks;

namespace BloodDonationSupportSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailTestController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailTestController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        /// <summary>
        /// Test the email sending functionality
        /// </summary>
        /// <param name="to">Email address to send the test email to</param>
        /// <returns>Result of the email test</returns>
        [HttpGet("test")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> TestEmail([FromQuery] string to)
        {
            if (string.IsNullOrEmpty(to))
            {
                return BadRequest(new ApiResponse(System.Net.HttpStatusCode.BadRequest, "Email address is required"));
            }

            var (success, message) = await _emailService.SendTestEmailAsync(to);
            
            if (success)
            {
                return Ok(new ApiResponse(message));
            }
            else
            {
                return BadRequest(new ApiResponse(System.Net.HttpStatusCode.BadRequest, message));
            }
        }
    }
}