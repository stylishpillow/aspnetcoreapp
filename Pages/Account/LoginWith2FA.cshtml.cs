using aspnetcoreapp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OtpNet;
using System.ComponentModel.DataAnnotations;

namespace aspnetcoreapp.Pages.Account
{
    public class LoginWith2FAModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LoginWith2FAModel(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            public string VerificationCode { get; set; }

            public bool RememberMe { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Unable to load two-factor authentication user.");
                return Page();
            }

            // Attempt to sign in using TwoFactorSignInAsync
            var result = await _signInManager.TwoFactorSignInAsync("Authenticator", Input.VerificationCode, Input.RememberMe, false);

            if (result.Succeeded)
            {
                return RedirectToPage("/Index");
            }
            else if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Your account is locked. Please try again later.");
                return Page();
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid 2FA verification code.");
                return Page();
            }
        }
    }
}