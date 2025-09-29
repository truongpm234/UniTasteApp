using System.Net;
using System.Net.Mail;

namespace UserService.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtp = _config.GetSection("Smtp");
            var fromAddress = new MailAddress(smtp["User"], "UniTaste");
            var message = new MailMessage();
            message.From = fromAddress;
            message.To.Add(to);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            using (var client = new SmtpClient(smtp["Host"], int.Parse(smtp["Port"])))
            {
                client.Credentials = new NetworkCredential(smtp["User"], smtp["Pass"]);
                client.EnableSsl = true;
                await client.SendMailAsync(message);
            }
        }

    }
}
