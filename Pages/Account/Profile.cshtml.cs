using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OtpNet;
using QRCoder;
using System;
using System.Text;
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

        public string QrCodeImageUrl { get; private set; }
        public string ManualEntryKey { get; private set; }
        public bool TwoFactorEnabled { get; private set; }

        public void OnGet()
        {
            var user = _userManager.GetUserAsync(User).Result;
            if (user == null) throw new InvalidOperationException("User not found.");

            TwoFactorEnabled = user.TwoFactorEnabled;

            if (!TwoFactorEnabled)
            {
                GenerateQrCodeAndSecretKey(user);
            }
        }

        public async Task<IActionResult> OnPostEnable2FAAsync(string verificationCode)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return Page();
            }

            // Debugging: Log the secret key and verification code
            Console.WriteLine($"TwoFactorSecretKey: {user.TwoFactorSecretKey}");
            Console.WriteLine($"VerificationCode: {verificationCode}");

            var totp = new Totp(OtpNet.Base32Encoding.ToBytes(user.TwoFactorSecretKey));
            var isValid = totp.VerifyTotp(verificationCode, out _);

            if (!isValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid verification code.");
                return Page();
            }

            // Save the authenticator key and enable 2FA
            await _userManager.SetAuthenticationTokenAsync(user, "Default", "AuthenticatorKey", user.TwoFactorSecretKey);
            await _userManager.SetTwoFactorEnabledAsync(user, true);

            Console.WriteLine($"2FA Enabled: {await _userManager.GetTwoFactorEnabledAsync(user)}");
            Console.WriteLine($"Authenticator Key: {await _userManager.GetAuthenticatorKeyAsync(user)}");

            TempData["SuccessMessage"] = "Two-Factor Authentication has been enabled.";
            return RedirectToPage();
        }

        public IActionResult OnPostDisable2FA()
        {
            var user = _userManager.GetUserAsync(User).Result;
            if (user == null) throw new InvalidOperationException("User not found.");

            user.TwoFactorEnabled = false;
            user.TwoFactorSecretKey = null;
            _userManager.UpdateAsync(user).Wait();
            TempData["SuccessMessage"] = "Two-Factor Authentication has been disabled.";
            return RedirectToPage();
        }
        private void GenerateQrCodeAndSecretKey(ApplicationUser user)
        {
            // Generate the secret key only if it doesn't already exist
            if (string.IsNullOrEmpty(user.TwoFactorSecretKey))
            {
                user.TwoFactorSecretKey = OtpNet.Base32Encoding.ToString(OtpNet.KeyGeneration.GenerateRandomKey(20));
                _userManager.UpdateAsync(user).Wait(); // Save the key to the database
            }

            // Generate the QR code using the existing secret key
            var qrCodeData = $"otpauth://totp/aspnetcoreapp:{user.Email}?secret={user.TwoFactorSecretKey}&issuer=aspnetcoreapp";

            using (var qrGenerator = new QRCoder.QRCodeGenerator())
            {
                var qrCodeDataObj = qrGenerator.CreateQrCode(qrCodeData, QRCoder.QRCodeGenerator.ECCLevel.Q);
                using (var qrCode = new QRCoder.PngByteQRCode(qrCodeDataObj))
                {
                    var qrCodeBytes = qrCode.GetGraphic(20);
                    QrCodeImageUrl = "data:image/png;base64," + Convert.ToBase64String(qrCodeBytes);
                }
            }
        }
    }
}