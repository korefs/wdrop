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
            
            Console.WriteLine("\nScan this QR code with your mobile device:\n");
            Console.WriteLine(qrCodeAsAsciiArt);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nCould not generate QR code: {ex.Message}");
            Console.WriteLine("Please install the QRCoder package: dotnet add package QRCoder");
        }
    }
}