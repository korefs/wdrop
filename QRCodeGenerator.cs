using QRCoder;

namespace Wdrop;

public static class QRCodeGenerator
{
    public static void GenerateQRCode(string text)
    {
        try
        {
            QRCoder.QRCodeGenerator qrGenerator = new QRCoder.QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCoder.QRCodeGenerator.ECCLevel.Q);
            AsciiQRCode qrCode = new AsciiQRCode(qrCodeData);
            string qrCodeAsAsciiArt = qrCode.GetGraphicSmall();
            
            WConsole.Info("\nScan this QR code with your mobile device:\n");
            WConsole.Info(qrCodeAsAsciiArt);
        }
        catch (Exception ex)
        {
            WConsole.Error($"\nCould not generate QR code: {ex.Message}");
            WConsole.Error("Please install the QRCoder package: dotnet add package QRCoder");
        }
    }
}