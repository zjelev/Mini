using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace BBBUsers
{
    class StartUp
    {
        private static string path = ".." + Path.DirectorySeparatorChar;
        private static string inputFile = path + "Input.txt";

        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (File.Exists(inputFile))
            {
                string emailDomain = String.Empty;
                string passwd = "123456";

                if (args.Length > 0)
                {
                    passwd = args[0];
                }

                if (args.Length > 1)
                {
                    emailDomain = args[1];      //dotnet run passwd somedomain.com
                }

                List<User> users = addNewUsers(inputFile);

                string sendEmails = SendEmails(users, emailDomain);

                using (var writer = new StreamWriter(path + "SendEmails.html"))
                {
                    writer.Write(sendEmails);
                    Console.WriteLine($"in {path}SendEmails.html. Click on the link in this page to send emails");
                }

                string createUsers = CreateUsers(users, passwd);

                using (var writer = new StreamWriter(path + "CreateUsers.txt"))
                {
                    writer.Write(createUsers);
                    Console.WriteLine($"in file {path}CreateUsers.txt." + Environment.NewLine +
                    "You can now paste the generated commands on the BigBlueButton console as root.");
                }
            }

            if (File.Exists(path + "CreatedUsers.txt"))
            {
                using (var reader = new StreamReader(path + "CreatedUsers.txt"))
                {
                    string createUsers = reader.ReadToEnd();
                    List<User> users = UsersFromCommands(createUsers);

                    StringBuilder sb = new StringBuilder();
                    foreach (var user in users)
                    {
                        sb.Append($"{user.FirstName} {user.LastName} <{user.Email}>; ");
                    }

                    using (var writer = new StreamWriter(path + "UsersSortedByFirstName.txt"))
                    {
                        writer.Write(sb);
                        Console.WriteLine($" users sorted by first name written in file {path}UsersSortedByFirstName.txt");
                    }
                }
            }
            else
            {
                Console.WriteLine($"CreatedUsers.txt not found. Users are not sorted in file.");
            }

            sw.Stop();
            Console.WriteLine($"It took {sw.ElapsedTicks} timer ticks.");
        }

        private static List<User> addNewUsers(string inputFile)
        {
            List<User> users = new List<User>();
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
                    User user = new User(firstName, lastName, email);
                    users.Add(user);
                }
            }
            return users;
        }

        private static string SendEmails(List<User> users, string emailDomain)
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
            List<List<User>> userChunks = Extensions.SplitList(users) as List<List<User>>;
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

                writer.Length--;   //remove the last ' ' 
                writer.Length--;   //remove the last ',' 
                writer.Append("?subject=Online meetings&body=Hello," + "%0D%0A" + "%0D%0A" +
                                "Please test our server demo.bigbluebutton.com" + "%0D%0A" + "%0D%0A" +
                                "Regards," + "%0D%0A" + "%0D%0A" +
                                "Administrator" + "%0D%0A" +
                                $"\">Email new users {chunks++}<p></a>");
            }
            string footer = "\n</body>\n</html>";
            writer.Append(footer);
            Console.Write($"{usersWithEmails} email addresses generated ");
            return writer.ToString();
        }

        private static string CreateUsers(List<User> users, string passwd)
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

        private static List<User> UsersFromCommands(string createdUsers)
        {
            StringReader reader = new StringReader(createdUsers);
            StringBuilder writer = new StringBuilder();
            List<User> users = new List<User>();

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

                        User user = new User(firstName, lastName, email);
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
}