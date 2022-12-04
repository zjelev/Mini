using System.Text.Json;

public class Config
{
    public static string config = File.ReadAllText(Utils.Config.logPath + "config.json");
    public static string veznaHost = JsonSerializer.Deserialize<Config>(config).Vezna.Host;
    internal static string veznaPath = JsonSerializer.Deserialize<Config>(config).Vezna.Path;
    public static string veznaDb = JsonSerializer.Deserialize<Config>(config).Vezna.Db;
    internal static string veznaFilePattern = JsonSerializer.Deserialize<Config>(config).Vezna.File;
    public static string supplier = JsonSerializer.Deserialize<Config>(config).Speditor.Supplier;
    public static string destination = JsonSerializer.Deserialize<Config>(config).Speditor.Destination;
    public static string client = JsonSerializer.Deserialize<Config>(config).Speditor.Client;
    public static string load = JsonSerializer.Deserialize<Config>(config).Speditor.Load;
    public static string speditorFile = JsonSerializer.Deserialize<Config>(config).Speditor.File;
    public static string dateFormat = JsonSerializer.Deserialize<Config>(config).Speditor.DateFormat;

    public static TimeSpan beginShift = new TimeSpan(8, 0, 0);
    public static TimeSpan endShift = new TimeSpan(20, 0, 0);

    public Vezna Vezna { get; set; }
    public Speditor Speditor { get; set; }
}

public class Vezna
{
    public string Host { get; set; }
    public string Path { get; set; }
    public string File { get; set; }
    public string Db { get; set; }
}

public class Speditor
{
    public string File { get; set; }
    public string Email { get; set; }
    public string Supplier { get; set; }
    public string Destination { get; set; }
    public string Client { get; set; }
    public string Load { get; set; }
    public string DateFormat { get; set; }
}