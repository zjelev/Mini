namespace Common
{
    public class Config
    {
        public Developer Developer { get; set; }
        public SmtpServer SmtpServer { get; set; }
        public Vezna Vezna { get; set; }
        public Speditor Speditor { get; set; }
    }

    public class Developer
    {
        public string User { get; set; }
        public string Phone { get; set; }
    }

    public class SmtpServer
    {
        public string Host { get; set; }
        public int Port { get; set; }
    }

    public class Vezna
    {
        public string Host { get; set; }
        public string Path { get; set; }
        public string File { get; set; }
    }

    public class Speditor
    {
        public string File { get; set; }
        public string Email { get; set; }
    }
}