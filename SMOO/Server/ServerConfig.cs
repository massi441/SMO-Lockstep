using System.Text.Json;

public class ServerConfig
{
    public int Port { get; set; }
    public List<string> AllowedIPs { get; set; } = [];

    public static ServerConfig Load(string path)
    {
        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ServerConfig>(json)
                ?? throw new Exception("Config file is invalid");
        }
        catch (FileNotFoundException ex)
        {
            throw new Exception($"Config file not found at {path}", ex);
        }
    }
}