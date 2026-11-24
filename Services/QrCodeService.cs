using QRCoder;

namespace UniversityExamSystem.Services
{
    public interface IQrCodeService
    {
        string GenerateQrCodeBase64(string payload);
    }

    public class QrCodeService : IQrCodeService
    {
        public string GenerateQrCodeBase64(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload)) return string.Empty;

            try
            {
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
                var qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeBytes = qrCode.GetGraphic(6);
                return "data:image/png;base64," + Convert.ToBase64String(qrCodeBytes);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
