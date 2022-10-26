namespace Utils
{
    public class ConfigEFUtils
    {
        public string Podelenie { get; set; }
        public string DbFolder { get; set; }
        public string YearMonth { get; set; }
        public string LeaveFromDate { get; set; }
        public string DestinationDB { get; set; }
        public string Server { get; set; }
        public string WorkOrFor { get; set; }
        public string FirstSourceDb { get; set; }
        public string SecondSourceDb { get; set; }
        public string SecondSourceTable { get; set; }
    }

    public class ConfigEmail
    {
        public User User { get; set; }
        public SmtpServer SmtpServer { get; set; }
    }
    public class User
    {
        public string Name { get; set; }
        public string Account { get; set; }
        public string Phone { get; set; }
    }

    public class SmtpServer
    {
        public string Host { get; set; }
        public string Domain { get; set; }
        public int Port { get; set; }
    }
}