namespace Wdrop.Connections;

public class UploadFile
{
    public string FileName { get; set; }
    public string MimeType { get; set; }
    public string FilePath { get; set; }

    public UploadFile(string fileName, string mimeType, string filePath)
    {
        FileName = fileName;
        MimeType = mimeType;
        FilePath = filePath;
    }
}

public interface IUpload
{
    Task UploadAsync(UploadFile uploadFile);
}