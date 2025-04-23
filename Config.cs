using System.Text;

namespace Wdrop;

public class Config
{
    private static Dictionary<string, string> TryCreateInitialConfig()
    {
        string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wdropconfig");

        Dictionary<string, string> config = [];
        config["uploadto"] = "local";

        if (File.Exists(configPath))
        {
            return config;
        }

        StringBuilder initialConfigContentSb = new();

        initialConfigContentSb.Append("# Wdrop configuration file");
        initialConfigContentSb.AppendLine("# Default upload destination");
        initialConfigContentSb.AppendLine("uploadto=local");

        File.WriteAllText(configPath, initialConfigContentSb.ToString());

        return config;
    }

    public static Dictionary<string, string> LoadConfig()
    {
        string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wdropconfig");
        Dictionary<string, string> config = [];

        if (!File.Exists(configPath))
        {
            config = TryCreateInitialConfig();

            return config;
        }

        foreach (var line in File.ReadAllLines(configPath))
        {
            if (line.StartsWith("#") || !line.Contains("=")) continue;
            var parts = line.Split("=", 2);
            config[parts[0].Trim()] = parts[1].Trim();
        }

        return config;
    }
    public static void SetConfig(string key, string value)
    {
        string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wdropconfig");
        var lines = new List<string>();
        var found = false;

        TryCreateInitialConfig();

        if (!File.Exists(configPath))
        {
            return;
        }
        
        foreach (string line in File.ReadAllLines(configPath))
        {
            string lineTrimmed = line.Trim();

            if (lineTrimmed.StartsWith("#"))
            {
                lines.Add(lineTrimmed);
                continue;
            }

            string[] parts = lineTrimmed.Split("=", 2);

            if (parts[0].Trim() == key)
            {
                lines.Add($"{key}={value}");
                found = true;
            }
            else
            {
                lines.Add(line);
            }
        }

        if (!found)
        {
            lines.Add($"{key}={value}");
        }

        File.WriteAllLines(configPath, lines);
    }

}