using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QRCoder;
using System.Text;
using OtpNet;
using aspnetcoreapp.Models;

namespace aspnetcoreapp.Pages.Account
{
    public class ProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public string? QrCodeImageUrl { get; private set; }
        public string? ManualEntryKey { get; private set; }
        public bool TwoFactorEnabled { get; private set; }

        public void OnGet()
        {
            TwoFactorEnabled = _userManager.GetUserAsync(User).Result?.TwoFactorEnabled ?? false;

            if (!TwoFactorEnabled)
            {
                GenerateQrCodeAndSecretKey();
            }

        }

        public IActionResult OnPostEnable2FA(string verificationCode)
        {
            string secretKey = TempData["SecretKey"] as string ?? throw new InvalidOperationException("Secret key not found.");
            var totp = new Totp(Base32Encoding.ToBytes(secretKey));
            bool isValid = totp.VerifyTotp(verificationCode, out long timeStepMatched);

            if (isValid)
            {
                EnableTwoFactorForUser(User.Identity?.Name, secretKey);
                TempData["SuccessMessage"] = "Two-Factor Authentication has been enabled.";
                return RedirectToPage();
            }
            else
            {
                GenerateQrCodeAndSecretKey();
                TempData["ErrorMessage"] = "Invalid verification code. The secret key has been reset. Please try again.";
                return Page();
            }
        }

        public IActionResult OnPostDisable2FA()
        {
            DisableTwoFactorForUser(User.Identity?.Name);
            TempData["SuccessMessage"] = "Two-Factor Authentication has been disabled.";
            return RedirectToPage();
        }

        private bool CheckIfTwoFactorEnabled(string? username)
        {
            if (username == null) return false;

            var user = _userManager.FindByNameAsync(username).Result;
            return user?.TwoFactorEnabled ?? false; // Ensure this directly reflects the database value
        }

        private void EnableTwoFactorForUser(string? username, string secretKey)
        {
            if (username == null) throw new ArgumentNullException(nameof(username));

            var user = _userManager.FindByNameAsync(username).Result;
            if (user != null)
            {
                user.TwoFactorEnabled = true;
                user.TwoFactorSecretKey = secretKey;

                var result = _userManager.UpdateAsync(user).Result;
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException("Failed to update user for enabling 2FA.");
                }
            }
        }

        private void DisableTwoFactorForUser(string? username)
        {
            if (username == null) throw new ArgumentNullException(nameof(username));

            var user = _userManager.FindByNameAsync(username).Result;
            if (user != null)
            {
                user.TwoFactorEnabled = false;
                user.TwoFactorSecretKey = null;

                var result = _userManager.UpdateAsync(user).Result;
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException("Failed to update user for disabling 2FA.");
                }
            }
        }

        private void GenerateQrCodeAndSecretKey()
        {
            string secretKey = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));
            string issuer = "aspnetcoreapp";
            string userEmail = User.Identity?.Name ?? "user@example.com";

            string qrCodeData = $"otpauth://totp/{issuer}:{userEmail}?secret={secretKey}&issuer={issuer}";

            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeDataObj = qrGenerator.CreateQrCode(qrCodeData, QRCodeGenerator.ECCLevel.Q);
                using (var qrCode = new PngByteQRCode(qrCodeDataObj))
                {
                    var qrCodeBytes = qrCode.GetGraphic(20);
                    QrCodeImageUrl = "data:image/png;base64," + Convert.ToBase64String(qrCodeBytes);
                }
            }

            ManualEntryKey = secretKey;

            TempData["SecretKey"] = secretKey;
        }
    }
}