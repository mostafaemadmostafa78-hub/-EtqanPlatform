using ETQAN_BY_API.DTO;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
//using System.Net.Mail;
using Twilio.TwiML.Messaging;
using MimeKit;

using MailKit.Net.Smtp;


namespace ETQAN_BY_API.Services
{
    public class EmailServices : IEmailServices
    {
        private readonly IConfiguration _configuration;

        public EmailServices(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendEmail(EmailDTO request)
        {

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(request.To));
            email.To.Add(MailboxAddress.Parse(request.To));
            email.Subject = request.Subject;
            email.Body = new TextPart(TextFormat.Html) { Text =request.Body  };
            using var smtp = new SmtpClient();
            smtp.Connect(_configuration.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(_configuration.GetSection("EmailUserName").Value, _configuration.GetSection("EmailPassword").Value);
            smtp.Send(email);
            smtp.Disconnect(true);
             
        }
    }
}
