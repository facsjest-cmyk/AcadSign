namespace AcadSign.Backend.Application.Common.Interfaces;

public interface IQrCodeService
{
    byte[] GenerateQrCode(string data, int pixelSize = 300);
}
