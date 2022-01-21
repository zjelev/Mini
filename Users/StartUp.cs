using System;
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

            string emailDomain;
            string passwd = args[0];

            if (args.Length == 1)
            {
                emailDomain = String.Empty;
            }
            else
            {
                emailDomain = args[1];
            }

            //dotnet run passwd somedomain.com
            string namesEmailInLines = SeparateToLines(inputFile);

            string preparedEmails = SendEmails(namesEmailInLines, emailDomain);
            
            using (var writer = new StreamWriter(path + "SendEmails.txt"))
            {
                writer.Write(preparedEmails);
                Console.WriteLine("Email adresses prepared in file ..\\SendEmails.txt");
            }
            CreateUsers(preparedEmails, passwd);

            sw.Stop();
            Console.WriteLine($"Output files containing the commands are created in {sw.ElapsedTicks} timer ticks.{Environment.NewLine}" +
                                "You can now paste the generated commands on the BigBlueButton console as root.");

        }

        private static string SeparateToLines(string inputFile)
        {
            string separatedLines = path + "SeparatedLines.txt";
            string output;
            using (var reader = new StreamReader(inputFile))
            {
                //using (var writer = new StreamWriter(separatedLines))
                //{
                    output = reader.ReadToEnd();
                    output = output.Replace(";", Environment.NewLine);
                    output = output.Replace(">", ">" + Environment.NewLine);
                    
                    output = Regex.Replace(output, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline);
                    if (output[output.Length - 1].ToString().Contains('\n'))
                    {
                        output = output.Remove(output.Length - 1);      
                    }
                    //writer.Write(file);
                //}
            }
            return output;
        }

        private static string SendEmails(string namesEmailInLines, string emailDomain)
        {
            StringReader reader = new StringReader(namesEmailInLines);
            StringBuilder writer = new StringBuilder();
            //using (var reader = new StreamReader(namesEmailInLines))
            //{
            //using (var writer = new StreamWriter(preparedEmails))
            //{
            string line = reader.ReadLine();
                    int usersWithEmails = 0;
                    
                    while (line != null)
                    {
                        string[] separator =  { "\t", " <", ">; ", "\"", "<", ">", ";", "'", " " };
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

                    Console.WriteLine($"{usersWithEmails} email adresses prepared");
                    return writer.ToString();
                //}
            //}
        }

        private static void CreateUsers(string preparedEmails, string passwd)
        {
            //using (var reader = new StreamReader(preparedEmails))
            //{
            StringReader reader = new StringReader(preparedEmails);
            using (var writer = new StreamWriter(outputFile))
                {
                    string line = reader.ReadLine();
                    int counter = 0;

                    while (line != null)
                    {
                        string[] separator =  { " <", ">; ", "\"", "<", ">", ";", "'" }; //no matter if name is in " or ' or space
                        string[] lineArr = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                        string name = String.Empty;
                        string email = String.Empty;
                        
                        for (int i = 0; i < lineArr.Length-1; i=i+2)
                        {
                            name = lineArr[i][0].ToString().ToUpper() + lineArr[i].Substring(1);
                            email = lineArr[i+1];
                            string newLine = string.Concat("docker exec greenlight-v2 bundle exec rake user:create[\"", name, "\",\"", email, "\",\"", passwd, "\",\"user\"]");
                            writer.WriteLine(newLine);
                            counter++;
                        }

                        line = reader.ReadLine();
                    }

                    Console.WriteLine($"{counter} users prepared in file {outputFile}" + Environment.NewLine);
                }
            //}
        }
    }
}
