using AcadSign.Backend.Application.Common.Interfaces;
using QRCoder;

namespace AcadSign.Backend.Infrastructure.QrCode;

public class QrCodeService : IQrCodeService
{
    public byte[] GenerateQrCode(string data, int pixelSize = 300)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrCodeData);
        
        var qrCodeImage = qrCode.GetGraphic(pixelsPerModule: 20);
        
        return qrCodeImage;
    }
}
