using EmailMicroService.Models;
using EmailMicroService.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmailMicroService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly EmailService _emailService;

        public EmailController(EmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] MailInfos mailInfos)
        {
            await _emailService.SendMailAsync(mailInfos);
            return Ok();
        }
    }
}
