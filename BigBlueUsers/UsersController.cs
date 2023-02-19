using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Utils;

public class UsersController
{
    internal static List<Person> AddNewUsers(string inputFile)
    {
        List<Person> users = new List<Person>();
        string lines;
        string[] namesEmail;
        using (var reader = new StreamReader(inputFile))
        {
            lines = reader.ReadToEnd();
            lines = lines.Replace(";", Environment.NewLine);
            lines = lines.Replace(">", ">" + Environment.NewLine);
            lines = Regex.Replace(lines, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline);
        }
        namesEmail = lines.Split(Environment.NewLine);

        foreach (var line in namesEmail)
        {
            string[] separator = { "\t", " <", ">; ", "\"", "<", ">", ";", "'", " " };
            string[] lineArr = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (lineArr.Length > 0)
            {
                string firstName = Extensions.FirstCharToUpperNextToLower(lineArr[0]);
                string lastName = Extensions.FirstCharToUpperNextToLower(lineArr[lineArr.Length - 2]);
                string email = lineArr[lineArr.Length - 1];
                Person user = new Person(firstName, lastName, email);
                users.Add(user);
            }
        }
        return users;
    }

    internal static string PrepareEmails(List<Person> users, string emailDomain)
    {
        StringBuilder writer = new StringBuilder();
        string header =
@"<!DOCTYPE html>
    <head>
    <meta charset=" + "\"UTF-8\">" +
@"<title>Send Emails</title>
    </head>
    <body>";
        writer.Append(header);

        int usersWithEmails = 0;
        int chunks = 0;
        List<List<Person>> userChunks = Extensions.SplitList(users) as List<List<Person>>;
        foreach (var chunk in userChunks)
        {
            writer.AppendLine("<a href=\"mailto:");
            foreach (var user in chunk)
            {
                if (user.Email.Contains(emailDomain))
                {
                    string email = string.Concat(user.Email, "; ");
                    writer.Append(email);
                    usersWithEmails++;
                }
                else
                {
                    Console.WriteLine($"{user.FirstName} {user.LastName} has different email domain: {user.Email}. Skipped.");
                }
            }

            string config = File.ReadAllText("..\\WeightNotes\\bin\\config.json");
            string server = JsonSerializer.Deserialize<ConfigEmail>(config).SmtpServer.Domain;
            string sender = JsonSerializer.Deserialize<ConfigEmail>(config).User.Name;
            string password = JsonSerializer.Deserialize<ConfigEmail>(config).User.Password;
            string phone = JsonSerializer.Deserialize<ConfigEmail>(config).User.Phone;
            string position = JsonSerializer.Deserialize<ConfigEmail>(config).User.Position;

            writer.Length--;   //remove the last ' ' 
            writer.Length--;   //remove the last ',' 
            writer.Append("?subject=Онлайн оперативки&body=Здравейте," + "%0D%0A" + "%0D%0A" +
                            "Моля тествайте нашия нов сървър за онлайн оперативки https://bbb." + server + "%0D%0A" + "%0D%0A" +
                            "Потребителското име е Вашия е-мейл, а първоначалната парола е " + password + "%0D%0A" + "%0D%0A" +
                            "Поздрави," + "%0D%0A" + "%0D%0A" + sender + " %0D%0A" + position + "%0D%0A" + phone +
                            $"\">Email new users {chunks++}<p></a>");
        }
        string footer = "\n</body>\n</html>";
        writer.Append(footer);
        Console.Write($"{usersWithEmails} email addresses generated ");
        return writer.ToString();
    }

    internal static string CreateUsers(List<Person> users, string passwd)
    {
        StringBuilder writer = new StringBuilder();

        foreach (var user in users)
        {
            string newLine = string.Concat("docker exec greenlight-v2 bundle exec rake user:create[\"", user.FirstName, " ", user.LastName, "\",\"", user.Email, "\",\"", passwd, "\",\"user\"]");
            writer.AppendLine(newLine);
        }

        Console.Write($"Commands for creating {users.Count} users prepared ");
        return writer.ToString();
    }

    internal static List<Person> UsersFromCommands(string createdUsers)
    {
        StringReader reader = new StringReader(createdUsers);
        StringBuilder writer = new StringBuilder();
        List<Person> users = new List<Person>();

        string line = reader.ReadLine();
        int counter = 0;
        while (line != null)
        {
            if (line.Length > 1 && line != Environment.NewLine)
            {
                int delimiter = line.IndexOf('[') + 1;
                string newLine = line.Remove(0, delimiter);
                string[] separator = { "\t", " <", ">; ", "\"", "<", ">", ";", "'", " " };
                string[] lineArr = newLine.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                if (lineArr.Length > 2)
                {
                    string firstName = lineArr[0];
                    string lastName = lineArr[1];
                    string email = lineArr[3];

                    Person user = new Person(firstName, lastName, email);
                    users.Add(user);
                    counter++;
                }
            }
            line = reader.ReadLine();
        }

        // // With Linq:
        // List<User> sortedUsers = users.OrderBy(x => x.FirstName)
        //                         .ThenBy(x => x.LastName)
        //                         .ToList();

        users.Sort((x, y) =>
        {
            int ret = String.Compare(x.FirstName, y.FirstName);
            return ret != 0 ? ret : x.LastName.CompareTo(y.LastName);
        });

        Console.Write(counter);
        return users;
    }
}