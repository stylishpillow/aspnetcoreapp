using aspnetcoreapp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using OtpNet;
using aspnetcoreapp.Models;

namespace aspnetcoreapp.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LoginModel(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [BindProperty]
        public required InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public required string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public required string Password { get; set; }

            public bool RememberMe { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToPage("/Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }
        }

        public IActionResult OnPostLogin(string username, string password, string? verificationCode)
        {
            // Validate username and password
            if (IsValidUser(username, password))
            {
                if (IsTwoFactorEnabled(username))
                {
                    if (string.IsNullOrEmpty(verificationCode))
                    {
                        TempData["ErrorMessage"] = "Please enter your 2FA verification code.";
                        return Page();
                    }

                    string secretKey = GetSecretKeyFromDatabase(username);
                    var totp = new Totp(Base32Encoding.ToBytes(secretKey));
                    if (!totp.VerifyTotp(verificationCode, out long timeStepMatched))
                    {
                        TempData["ErrorMessage"] = "Invalid 2FA verification code.";
                        return Page();
                    }
                }

                // Log the user in
                SignInUser(username);
                return RedirectToPage("/Index");
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid username or password.";
                return Page();
            }
        }

        private bool IsValidUser(string username, string password)
        {
            // Implement user validation logic here
            return true;
        }

        private bool IsTwoFactorEnabled(string username)
        {
            // Implement logic to check if 2FA is enabled for the user
            return true;
        }

        private string GetSecretKeyFromDatabase(string username)
        {
            // Implement logic to retrieve the secret key from the database
            return "SECRET_KEY";
        }

        private void SignInUser(string username)
        {
            // Implement logic to sign in the user
        }
    }
}