using System.Text;
using Sharprompt;
using Wdrop;
using Wdrop.Connections;

Console.WriteLine(@"
 _    _     _                 
| |  | |   | |                
| |  | | __| |_ __ ___  _ __  
| |/\| |/ _` | '__/ _ \| '_ \ 
\  /\  / (_| | | | (_) | |_) |
 \/  \/ \__,_|_|  \___/| .__/ 
                       | |    
                       |_|     v0.2
         ?? Wdrop - LAN File Sharing
");

string? pathToShare;
bool isDirectory = false;

var config = Config.LoadConfig();

string[] uploadOptions = ["local", "0x0.st", "p2p"];

if (args.Length != 0 && args[0] == "--defaultupload")
{
    if (args.Length < 2)
    {
        Console.WriteLine($"Current Default upload destination is '{config["uploadto"]}'");
        return;
    }

    string pickSelectedUpload = args[1];

    if (!uploadOptions.Contains(pickSelectedUpload))
    {
        Console.WriteLine($"Unknown destination provided: {pickSelectedUpload}");
        Console.WriteLine("Available destination providers: ");
        StringBuilder sb = new();
        foreach (string s in uploadOptions)
        {
            sb.Append($"{s};");
        }
        System.Console.WriteLine(sb.ToString());
        return;
    }

    Config.SetConfig("uploadto", args[1]);
    Console.WriteLine($"? Default upload destination set to '{args[1]}'");
    return;
}

if (args.Length == 0)
{
    Console.WriteLine("?? No file was passed as an argument. Selecting via menu...");

    string[] items = [.. Directory.GetFiles(Directory.GetCurrentDirectory())];

    if (items.Length == 0)
    {
        Console.WriteLine("? No files found in current folder.");
        return;
    }

    string[] directories = Directory.GetDirectories(Directory.GetCurrentDirectory());

    items = [.. items, .. directories];

    string selectedFile = Prompt.Select("Choose a file or folder to share", [.. items.Select(Path.GetFileName)]);

    pathToShare = Path.Combine(Directory.GetCurrentDirectory(), selectedFile);
    isDirectory = Directory.Exists(pathToShare);
}
else
{
    pathToShare = args[0];
    isDirectory = Directory.Exists(pathToShare);
}


if (isDirectory)
{
    Console.WriteLine("Creating .zip from folder...");
    string zipPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(pathToShare) + ".zip");
    if (File.Exists(zipPath)) File.Delete(zipPath);
    System.IO.Compression.ZipFile.CreateFromDirectory(pathToShare, zipPath);
    pathToShare = zipPath;
}

if (!File.Exists(pathToShare))
{
    Console.WriteLine($"? File not found. \n{pathToShare}");
    return;
}

string fileName = Path.GetFileName(pathToShare);


if (!uploadOptions.Contains(config["uploadto"]))
{
    Console.WriteLine($"? Invalid upload destination '{config["uploadto"]}'. Using 'local' instead.");
    Config.SetConfig("uploadto", "local");
}

string defaultUpload = config["uploadto"] ?? "local";

UploadFile uploadFile = new(fileName, Path.GetExtension(fileName), pathToShare);

switch (defaultUpload)
{
    case "0x0.st":
        await NullPointerUpload.UploadAsync(uploadFile);
        break;
    case "p2p":
        await P2P.StartP2PServer(uploadFile);
        break;
    case "local":
        await Local.UploadLocal(uploadFile);
        break;
    default:
        await Local.UploadLocal(uploadFile);
        break;
}