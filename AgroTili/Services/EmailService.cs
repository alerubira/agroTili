using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using AgroTili.Models;

namespace AgroTili.Services
{
    public class EmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task EnviarCorreoAsync(string destinatario, string asunto, string cuerpo)
        {
            using var smtpClient = new SmtpClient(_settings.SmtpServer)
            {
                Port = _settings.Port,
                Credentials = new NetworkCredential(_settings.FromAddress, _settings.Password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.FromAddress, _settings.DisplayName),
                Subject = asunto,
                Body = cuerpo,
                IsBodyHtml = true
            };

            mailMessage.To.Add(destinatario);
            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
