namespace Wdrop.Connections;

public class NullPointerUpload
{
    public static async Task UploadAsync(UploadFile uploadFile)
    {
        // max size for 0x0.st is 512mb
        if (new FileInfo(uploadFile.FilePath).Length > 512 * 1024 * 1024)
        {
            WConsole.Error("File is too large for 0x0.st. Please use another provider instead.");
            return;
        }

        WConsole.Info("Uploading to 0x0.st...");

        using var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("WdropUploader/1.0");
        using var multipart = new MultipartFormDataContent();
        using var stream = File.OpenRead(uploadFile.FilePath);
        multipart.Add(new StreamContent(stream), "file", uploadFile.FileName);

        var response = await client.PostAsync("https://0x0.st", multipart);
        string result = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            string link = result.Trim();
            WConsole.Success($"File sent successfully!");
            
            // Generate QR code for the link
            QRCodeGenerator.GenerateQRCode(link);
            WConsole.Info($"Link: {link}");
        }
        else
        {
            WConsole.Error("Failed to upload file.");
            WConsole.Error(result);
        }
    }
}