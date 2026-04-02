using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;
using MailKit.Net.Smtp;

using static System.Net.Mime.MediaTypeNames;
using ETQAN_BY_API.Services;
using ETQAN_BY_API.DTO;

namespace ETQAN_BY_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
      private readonly  IEmailServices _emailServices;

        public EmailController(IEmailServices emailServices)
        {
            _emailServices = emailServices;
        }

        [HttpPost]
        public IActionResult SendEmail( EmailDTO request)
        {

            _emailServices.SendEmail(request);

            return Ok("Email Created Successfully");
        }
    }
}
