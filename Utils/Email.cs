using System.Net.Mail;
using System.Text.Json;

namespace Utils
{
    public class Email
    {
        public static void Send(string config, string passwd, string recipient, List<string> ccRecipients, string subject, string body, params string[] attachments)
        {
            string mailServer = JsonSerializer.Deserialize<ConfigEmail>(config).SmtpServer.Host;
            string domain = JsonSerializer.Deserialize<ConfigEmail>(config).SmtpServer.Domain;
            string account = JsonSerializer.Deserialize<ConfigEmail>(config)?.User.Account;
            string sender = account + "@" + domain;

            using SmtpClient smtpServer = new SmtpClient(mailServer + "." + domain);
            using MailMessage mail = new MailMessage();

            mail.From = new MailAddress(sender);
            mail.To.Add(recipient);

            foreach (var ccRecipient in ccRecipients)
            {
                mail.CC.Add(ccRecipient);
            }

            var accountFirstName = account.Substring(0, account.IndexOf('.'));
            if (!recipient.StartsWith(accountFirstName))
            {
                mail.Bcc.Add(sender);
            }

            mail.Subject = subject;
            mail.Body = body;// + Environment.NewLine + "Contact: " + JsonSerializer.Deserialize<ConfigEmail>(config).User.Phone;

            foreach (var attach in attachments)
            {
                if (attach != null)
                {
                    Attachment attachment = new Attachment(attach);
                    mail.Attachments.Add(attachment);
                }
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