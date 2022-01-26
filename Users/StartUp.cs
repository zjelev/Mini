using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace BigBlueButtonUsers
{
    class StartUp
    {
        private static string path = ".." + Path.DirectorySeparatorChar;
        private static string inputFile = path + "Input.txt";
        private static string outputFile = path + "CreateUsers.txt";

        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            string createUsers;

            if (File.Exists(inputFile))
            {
                //dotnet run passwd somedomain.com
                string emailDomain = String.Empty;
                string passwd = "123456";

                if (args.Length > 0)
                {
                    passwd = args[0];
                }

                if (args.Length > 1)
                {
                    emailDomain = args[1];
                }

                string namesEmailInLines = SeparateToLines(inputFile);

                string preparedEmails = SendEmails(namesEmailInLines, emailDomain);

                using (var writer = new StreamWriter(path + "SendEmails.txt"))
                {
                    writer.Write(preparedEmails);
                    Console.WriteLine($"in file {path}SendEmails.txt");
                }

                createUsers = CreateUsers(preparedEmails, passwd);

                using (var writer = new StreamWriter(outputFile))
                {
                    writer.Write(createUsers);
                    Console.WriteLine($"in file {outputFile}." + Environment.NewLine +
                    "You can now paste the generated commands on the BigBlueButton console as root.");
                }
            }

            if (File.Exists(path + "CreatedUsers.txt"))
            {
                using (var reader = new StreamReader(path + "CreatedUsers.txt"))
                {
                    createUsers = reader.ReadToEnd();
                    List<User> users = UsersFromCommands(createUsers);

                    StringBuilder sb = new StringBuilder();
                    foreach (var user in users)
                    {
                        //sb.AppendLine(user.ToString());
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
                Console.WriteLine($"Neither {inputFile} nor ..\\CreatedUsers.txt found. Please provide one or both.");
                return;
            }

            sw.Stop();
            Console.WriteLine($"Output took {sw.ElapsedTicks} timer ticks.");
        }

        private static string SeparateToLines(string inputFile)
        {
            string output;
            using (var reader = new StreamReader(inputFile))
            {
                output = reader.ReadToEnd();
                output = output.Replace(";", Environment.NewLine);
                output = output.Replace(">", ">" + Environment.NewLine);

                output = Regex.Replace(output, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline);
                if (output[output.Length - 1].ToString().Contains('\n'))
                {
                    output = output.Remove(output.Length - 1);
                }
            }
            return output;
        }

        private static string SendEmails(string namesEmailInLines, string emailDomain)
        {
            StringReader reader = new StringReader(namesEmailInLines);
            StringBuilder writer = new StringBuilder();

            string line = reader.ReadLine();
            int usersWithEmails = 0;

            while (line != null)
            {
                string[] separator = { "\t", " <", ">; ", "\"", "<", ">", ";", "'", " " };
                string[] lineArr = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                if (lineArr.Length > 0)
                {
                    string firstName = StringExtensions.FirstCharToUpperNextToLower(lineArr[0]);
                    string lastName = StringExtensions.FirstCharToUpperNextToLower(lineArr[lineArr.Length - 2]);
                    string email = lineArr[lineArr.Length - 1];

                    if (line.Contains(emailDomain))
                    {
                        string newLine = string.Concat(firstName, " ", lastName, " <", email, ">; ");
                        writer.Append(newLine);
                        usersWithEmails++;
                    }
                    else
                    {
                        Console.WriteLine($"{firstName} {lastName} has different email domain: {email}. Skipped.");
                    }
                }

                line = reader.ReadLine();
            }

            Console.Write($"{usersWithEmails} email adresses prepared ");
            return writer.ToString();
        }

        private static string CreateUsers(string preparedEmails, string passwd)
        {
            StringReader reader = new StringReader(preparedEmails);
            StringBuilder writer = new StringBuilder();

            string line = reader.ReadLine();
            int counter = 0;

            while (line != null)
            {
                string[] separator = { " <", ">; ", "\"", "<", ">", ";", "'" }; //no matter if name is in " or ' or space
                string[] lineArr = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                string name = String.Empty;
                string email = String.Empty;

                for (int i = 0; i < lineArr.Length - 1; i = i + 2)
                {
                    name = lineArr[i][0].ToString().ToUpper() + lineArr[i].Substring(1);
                    email = lineArr[i + 1];
                    string newLine = string.Concat("docker exec greenlight-v2 bundle exec rake user:create[\"", name, "\",\"", email, "\",\"", passwd, "\",\"user\"]");
                    writer.AppendLine(newLine);
                    counter++;
                }
                line = reader.ReadLine();
            }

            Console.Write($"Commands for creating {counter} users prepared ");
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
