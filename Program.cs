using Sharprompt;
using Wdrop;
using Wdrop.Connections;
using Wdrop.Strategies;

Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine(@"
 _    _     _                 
| |  | |   | |                
| |  | | __| |_ __ ___  _ __  
| |/\| |/ _` | '__/ _ \| '_ \ 
\  /\  / (_| | | | (_) | |_) |
 \/  \/ \__,_|_|  \___/| .__/ 
                       | |    
                       |_|     v0.3
        Terminal File Sharing

");
Console.ResetColor();

string pathToShare = string.Empty;
string tmpZipPath = string.Empty;
bool isDirectory = false;

Context.Config = Config.LoadConfig();

ICommandStrategy? commandStrategy;

if (args.Length != 0)
{
    string input = args[0].Trim();

    commandStrategy = CommandStrategyFactory.GetStrategy(input);

    if (commandStrategy != null)
    {
        await commandStrategy.Run([.. args.Skip(1)]);
        return;
    }
    
    pathToShare = input;
    isDirectory = Directory.Exists(pathToShare);
}

if (args.Length == 0)
{
    WConsole.Info("No file was passed as an argument. Selecting via menu...");

    string[] items = [.. Directory.GetFiles(Directory.GetCurrentDirectory())];

    if (items.Length == 0)
    {
        WConsole.Warn("No files found in current folder.");
        return;
    }

    string[] directories = Directory.GetDirectories(Directory.GetCurrentDirectory());

    items = [.. items, .. directories];

    string selectedFile = Prompt.Select("Choose a file or folder to share", [.. items.Select(Path.GetFileName)]);

    pathToShare = Path.Combine(Directory.GetCurrentDirectory(), selectedFile);
    isDirectory = Directory.Exists(pathToShare);
}


if (isDirectory)
{
    WConsole.Info("Creating .zip from folder...");
    string zipPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(pathToShare) + ".zip");
    if (File.Exists(zipPath)) File.Delete(zipPath);
    System.IO.Compression.ZipFile.CreateFromDirectory(pathToShare, zipPath);
    pathToShare = zipPath;
    tmpZipPath = zipPath;
}

if (!File.Exists(pathToShare))
{
    WConsole.Error($"File not found: {pathToShare}");
    return;
}

string fileName = Path.GetFileName(pathToShare);

if (!Context.uploadOptions.Contains(Context.Config["uploadto"]))
{
    WConsole.Warn($"Invalid upload destination '{Context.Config["uploadto"]}'. Using 'local' instead.");
    Config.SetConfig("uploadto", "local");
}

string defaultUpload = Context.Config["uploadto"] ?? "local";

UploadFile uploadFile = new(fileName, Path.GetExtension(fileName), pathToShare);

IUploadStrategy uploadStrategy = UploadStrategyFactory.GetStrategy(defaultUpload);

await uploadStrategy.UploadAsync(uploadFile);

if (!string.IsNullOrEmpty(tmpZipPath) && File.Exists(tmpZipPath))
{
    File.Delete(pathToShare);
}
