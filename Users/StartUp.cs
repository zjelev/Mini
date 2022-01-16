using System;
using System.Diagnostics;
using System.IO;

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

            string emailDomain;
            if (args.Length == 1)
            {
                emailDomain = String.Empty;
            }
            else
            {
                emailDomain = args[1];
            }

            // dotnet run passwd somedomain.com 
            string preparedEmails = SentEmails(inputFile, emailDomain);

            CreateUsers(preparedEmails, args[0]);

            //CreateUsersFromTabbedTxt(inputFile, passwd, emailDomain);

            sw.Stop();
            Console.WriteLine($"Output files containing the commands are created in {sw.ElapsedTicks} timer ticks.{Environment.NewLine}" +
                                "You can now paste the generated commands on the BigBlueButton console as root.");

        }

        private static string SentEmails(string namesEmailTab, string emailDomain)
        {
            string outputFile = path + "SendEmailsTo.txt";
            using (var reader = new StreamReader(namesEmailTab))
            {
                using (var writer = new StreamWriter(outputFile))
                {
                    string line = reader.ReadLine();
                    int usersWithEmails = 0;
                    
                    while (line != null)
                    {
                        string[] lineArr = line.Split("\t", StringSplitOptions.RemoveEmptyEntries);
                        string firstName = lineArr[0];
                        string lastName = lineArr[lineArr.Length - 2];
                        string email = lineArr[lineArr.Length - 1];
                        
                        if (line.Contains(emailDomain))
                        {
                            string newLine = string.Concat(firstName, " ", lastName, " <", email, ">; ");
                            writer.Write(newLine);
                            usersWithEmails++;
                        }
                        else
                        {
                            Console.WriteLine($"{firstName} {lastName} has different email domain: {email}. Skipped.");
                        }
                        
                        line = reader.ReadLine();
                    }

                    Console.WriteLine($"{usersWithEmails} email adresses prepared in file {outputFile}" + Environment.NewLine);
                    return outputFile;

                }
            }
        }

        private static void CreateUsers(string preparedEmails, string passwd)
        {
            using (var reader = new StreamReader(preparedEmails))
            {
                using (var writer = new StreamWriter(outputFile))
                {
                    string line = reader.ReadLine();
                    int counter = 0;

                    while (line != null)
                    {
                        string[] separator =  { " <", ">; ", "\"", "<", ">", ";" }; //no matter if name is in quotes
                        string[] lineArr = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                        string name = String.Empty;
                        string email = String.Empty;
                        
                        for (int i = 0; i < lineArr.Length-1; i=i+2)
                        {
                            name = lineArr[i];
                            email = lineArr[i+1];
                            string newLine = string.Concat("docker exec greenlight-v2 bundle exec rake user:create[\"", name, "\",\"", email, "\",\"", passwd, "\",\"user\"]");
                            writer.WriteLine(newLine);
                            counter++;
                        }

                        line = reader.ReadLine();
                    }

                    Console.WriteLine($"{counter} users prepared in file {outputFile}" + Environment.NewLine);
                }
            }
        }
    
        //Obsolete
        private static void CreateUsersFromTabbedTxt(string namesEmailTab, string passwd, string emailDomain)
        {
            //Input should be lines with names(1 or more) and e-mail, separated with tabs

            using (var reader = new StreamReader(namesEmailTab))
            {
                using (var writer = new StreamWriter(path + "CreateUsersFromTabbed.txt"))
                {
                    string line = reader.ReadLine();
                    int allUsers = 0;
                    int usersWithEmails = 0;
                    while (line != null)
                    {
                        allUsers++;
                        if (line.Contains(emailDomain))
                        {
                            // More complex, that's why abandoned for now
                            // string pattern = @"[\w-]+@([\w-]+\.)+[\w-]+";
                            // Regex regex = new Regex(pattern);
                            // string replacement = @".";
                            // newLine = regex.Replace(pattern, replacement);

                            string[] lineArr = line.Split("\t", StringSplitOptions.RemoveEmptyEntries);
                            lineArr[lineArr.Length - 2] = lineArr[lineArr.Length - 2] + "\",\"" + lineArr[lineArr.Length - 1];
                            lineArr[lineArr.Length - 1] = String.Empty;
                            string newLine = string.Join(" ", lineArr).Trim();
                            newLine = string.Concat("docker exec greenlight-v2 bundle exec rake user:create[\"", newLine, "\",\"", passwd, "\",\"user\"]");
                            writer.WriteLine(newLine);
                            usersWithEmails++;
                        }
                        line = reader.ReadLine();
                    }
                    Console.WriteLine($"From {allUsers} users with e-mails {usersWithEmails} have e-mails ending in {emailDomain}" + Environment.NewLine);
                }
            }
        }
    
    }
}