namespace Wdrop;

public class Config
{

    public static Dictionary<string, string> LoadConfig()
    {
        string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wdropconfig");
        var config = new Dictionary<string, string>();

        if (!File.Exists(configPath))
        {
            File.WriteAllText(configPath, "# Wdrop configuration file\n" +
                "# Default upload destination\n" +
                "uploadto=local\n");

            config["uploadto"] = "local";
            
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

        if (File.Exists(configPath))
        {
            foreach (var line in File.ReadAllLines(configPath))
            {
                if (line.StartsWith("#") || !line.Contains("="))
                {
                    lines.Add(line);
                    continue;
                }

                var parts = line.Split("=", 2);
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
        }

        if (!found)
        {
            lines.Add($"{key}={value}");
        }

        File.WriteAllLines(configPath, lines);
    }

}