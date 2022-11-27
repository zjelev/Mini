using System.Text.Json;

public class Config
{
    internal static string logPath = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar;
    internal static string config = File.ReadAllText(logPath + "config.json");
    public static string veznaHost = JsonSerializer.Deserialize<Config>(config).Vezna.Host;
    internal static string veznaPath = JsonSerializer.Deserialize<Config>(config).Vezna.Path;
    public static string veznaParadoxDb = JsonSerializer.Deserialize<Config>(config).Vezna.ParadoxDb;
    internal static string veznaFilePattern = JsonSerializer.Deserialize<Config>(config).Vezna.File;
    internal static string supplier = JsonSerializer.Deserialize<Config>(config).Speditor.Supplier;
    internal static string destination = JsonSerializer.Deserialize<Config>(config).Speditor.Destination;
    internal static string client = JsonSerializer.Deserialize<Config>(config).Speditor.Client;
    internal static string load = JsonSerializer.Deserialize<Config>(config).Speditor.Load;
    internal static string speditorFile = JsonSerializer.Deserialize<Config>(config).Speditor.File;
    internal static string dateFormat = JsonSerializer.Deserialize<Config>(config).Speditor.DateFormat;

    internal static TimeSpan beginShift = new TimeSpan(8, 0, 0);
    internal static TimeSpan endShift = new TimeSpan(20, 0, 0);

    public Vezna Vezna { get; set; }
    public Speditor Speditor { get; set; }
}

public class Vezna
{
    public string Host { get; set; }
    public string Path { get; set; }
    public string File { get; set; }
    public string ParadoxDb { get; set; }
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