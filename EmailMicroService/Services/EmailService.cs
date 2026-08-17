using EmailMicroService.Models;
using EmailMicroService.Utilities;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace EmailMicroService.Services
{
    public class EmailService
    {
        public async Task SendMailAsync(MailInfos mailInfos)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(mailInfos.From));
            message.To.Add(MailboxAddress.Parse(mailInfos.To));
            message.Subject = mailInfos.Subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = mailInfos.Body };

            if (mailInfos.Attachments is not null)
            {
                foreach (var attachment in mailInfos.Attachments)
                {
                    bodyBuilder.Attachments.Add(attachment.FileName, attachment.Content);
                }
            }

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(EnvironmentVaraibles.SmtpHost, EnvironmentVaraibles.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(EnvironmentVaraibles.SmtpLogin, EnvironmentVaraibles.SmtpKey);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
