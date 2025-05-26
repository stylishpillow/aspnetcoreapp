using aspnetcoreapp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace aspnetcoreapp.Pages.Account
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ConfirmEmailModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public bool IsEmailConfirmed { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return BadRequest("Invalid confirmation link.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            IsEmailConfirmed = result.Succeeded;

            return Page();
            
        }
    }
}