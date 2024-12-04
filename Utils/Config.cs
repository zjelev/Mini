using System.Text.Json;

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
        public string[] Tables { get; set; }
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
        public string Password { get; set; }
        public string Position { get; set; }
        public string PersId { get; set; }
    }

    public class SmtpServer
    {
        public string Host { get; set; }
        public string Domain { get; set; }
        public int Port { get; set; }
    }

    public class Config
    {
        public static string logPath = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar;
        public Dictionary<string, string[]> NoDodCodes { get; set; }
		    public static string config = File.ReadAllText("\\\\appl-srv\\d$\\DeliveryApp\\appsettings.json");
		// db props moved to Models\AvtoVezni\DbServer.cs
		public static string supplier = JsonSerializer.Deserialize<Config>(config).Speditor.Supplier;
		public static string speditorFile = JsonSerializer.Deserialize<Config>(config).Speditor.PlanFile;
		public static string dateFormat = JsonSerializer.Deserialize<Config>(config).Speditor.DateFormat;
		public static int port = JsonSerializer.Deserialize<Config>(config).WebServer.Port;
		public static string filesPath = JsonSerializer.Deserialize<Config>(config).WebServer.FilesPath;
		public static string opisPath = JsonSerializer.Deserialize<Config>(config).WebServer.OpisPath;
		public static string wwwRootPath = JsonSerializer.Deserialize<Config>(config).WebServer.WwwRootPath;
		public static string product = JsonSerializer.Deserialize<Config>(config).Speditor.Product;

		public static TimeSpan beginShift = new TimeSpan(8, 0, 0);
		public static TimeSpan endShift = new TimeSpan(20, 0, 0);

		public WebServer WebServer { get; set; }
		public Speditor Speditor { get; set; }
    }
	
	public class WebServer
    {
        public int Port { get; set; }
        public string FilesPath { get; set; }
        public string OpisPath { get; set; }
        public string WwwRootPath { get; set; }
    }

    public class Speditor
    {
        public string PlanFile { get; set; }
        public string Email { get; set; }
        public string Supplier { get; set; }
        public string DateFormat { get; set; }
        public string Product { get; set; }
    }
}