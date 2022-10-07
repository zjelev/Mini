using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;

namespace Common
{
    public class Email
    {
        public static void Send(string passwd, string recipient, List<string> ccRecipients, string subject, string body, string[] attachments)
        {
            var config = File.ReadAllText("config.json");
            string server = System.Text.Json.JsonSerializer.Deserialize<Config>(config)?.SmtpServer.Host;
            string sender = System.Text.Json.JsonSerializer.Deserialize<Config>(config)?.Developer.User + "@" + server;

            using SmtpClient smtpServer = new SmtpClient(server);
            using MailMessage mail = new MailMessage();

            mail.From = new MailAddress(sender);

            if (recipient == "sender")
            {
                mail.To.Add(sender);
            }
            else
            {
                mail.To.Add(recipient);
            }

            foreach (var ccRecipient in ccRecipients)
            {
                mail.CC.Add(ccRecipient);
            }

            mail.Bcc.Add(sender);

            mail.Subject = subject;
            mail.Body = body + Environment.NewLine + "Contact: " + System.Text.Json.JsonSerializer.Deserialize<Config>(config)?.Developer.Phone;

            foreach (var attach in attachments)
            {
                Attachment attachment = new Attachment(attach);
                mail.Attachments.Add(attachment);
            }

            smtpServer.Port = System.Text.Json.JsonSerializer.Deserialize<Config>(config).SmtpServer.Port;
            smtpServer.Credentials = new System.Net.NetworkCredential(sender, passwd);
            smtpServer.EnableSsl = false;

            smtpServer.Send(mail);
        }
    }
}