using System.Text;

string inputFile = "Input.txt";

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

    List<Person> users = UsersController.AddNewUsers(inputFile);

    string sendEmails = UsersController.PrepareEmails(users, emailDomain);

    using (var writer = new StreamWriter("SendEmails.html"))
    {
        writer.Write(sendEmails);
        Console.WriteLine($"in SendEmails.html. Click on the link in this page to send emails");
    }

    string createUsers = UsersController.CreateUsers(users, passwd);

    using (var writer = new StreamWriter("CreateUsers.txt"))
    {
        writer.Write(createUsers);
        Console.WriteLine($"in file CreateUsers.txt." + Environment.NewLine +
        "You can now paste the generated commands on the BigBlueButton console as root.");
    }
}

if (File.Exists("CreatedUsers.txt"))
{
    using (var reader = new StreamReader("CreatedUsers.txt"))
    {
        string createUsers = reader.ReadToEnd();
        List<Person> users = UsersController.UsersFromCommands(createUsers);

        StringBuilder sb = new StringBuilder();
        foreach (var user in users)
        {
            sb.Append($"{user.FirstName} {user.LastName} {user.Email}" + Environment.NewLine);
        }

        using (var writer = new StreamWriter("UsersSortedByFirstName.txt"))
        {
            writer.Write(sb);
            Console.WriteLine($" users sorted by first name written in file UsersSortedByFirstName.txt");
        }
    }
}
else
{
    Console.WriteLine($"CreatedUsers.txt not found. Users are not sorted in file.");
}
