using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QRCoder;
using System.Text;
using OtpNet;

namespace aspnetcoreapp.Pages.Account
{
    public class ProfileModel : PageModel
    {
        public string? QrCodeImageUrl { get; private set; }
        public string? ManualEntryKey { get; private set; }

        public void OnGet()
        {
            // Generate a random secret key
            string secretKey = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));
            string issuer = "aspnetcoreapp"; // Your app name
            string userEmail = User.Identity?.Name ?? "user@example.com";

            // Generate the QR code URL for the authenticator app
            string qrCodeData = $"otpauth://totp/{issuer}:{userEmail}?secret={secretKey}&issuer={issuer}";

            // Generate the QR code image
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeDataObj = qrGenerator.CreateQrCode(qrCodeData, QRCodeGenerator.ECCLevel.Q);
                using (var qrCode = new PngByteQRCode(qrCodeDataObj))
                {
                    var qrCodeBytes = qrCode.GetGraphic(20);
                    QrCodeImageUrl = "data:image/png;base64," + Convert.ToBase64String(qrCodeBytes);
                }
            }

            // Provide the manual entry key for users who cannot scan the QR code
            ManualEntryKey = secretKey;

            TempData["SecretKey"] = secretKey;
        }

        public IActionResult OnPostEnable2FA(string verificationCode)
        {
            string secretKey = TempData["SecretKey"] as string ?? throw new InvalidOperationException("Secret key not found.");
            var totp = new Totp(Base32Encoding.ToBytes(secretKey));
            bool isValid = totp.VerifyTotp(verificationCode, out long timeStepMatched);

            if (isValid)
            {
                // Mark 2FA as enabled for the user (e.g., update the database)
                TempData["SuccessMessage"] = "Two-Factor Authentication has been enabled.";
                return RedirectToPage();
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid verification code. Please try again.";
                return Page();
            }
        }
    }
}