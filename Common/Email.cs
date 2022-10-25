using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text.Json;

namespace Common
{
    public class Email
    {
        public static void Send(string passwd, string recipient, List<string> ccRecipients, string subject, string body, string[] attachments)
        {
            var config = File.ReadAllText("config.json");
            string mailServer = JsonSerializer.Deserialize<ConfigEmail>(config).SmtpServer.Host;
            string domain = JsonSerializer.Deserialize<ConfigEmail>(config).SmtpServer.Domain;
            string account = JsonSerializer.Deserialize<ConfigEmail>(config)?.User.Account;
            string sender = account + "@" + domain;

            using SmtpClient smtpServer = new SmtpClient(mailServer + "." + domain);
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
            mail.Body = body + Environment.NewLine + "Contact: " + JsonSerializer.Deserialize<ConfigEmail>(config).User.Phone;

            foreach (var attach in attachments)
            {
                Attachment attachment = new Attachment(attach);
                mail.Attachments.Add(attachment);
            }

            smtpServer.Port = JsonSerializer.Deserialize<ConfigEmail>(config).SmtpServer.Port;
            smtpServer.Credentials = new System.Net.NetworkCredential(account, passwd);
            smtpServer.EnableSsl = false;

            smtpServer.Send(mail);
        }

        public static string ReadHidePassword()
        {
            var pass = string.Empty;
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    Console.Write("\b \b");
                    pass = pass[0..^1];
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    pass += keyInfo.KeyChar;
                }
            } while (key != ConsoleKey.Enter);
            return pass;
        }
    }
}