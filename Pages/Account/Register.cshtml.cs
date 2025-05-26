using aspnetcoreapp.Data;
using aspnetcoreapp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace aspnetcoreapp.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(UserManager<ApplicationUser> userManager, ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public required InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            public required string Name { get; set; }

            [Required]
            [EmailAddress]
            public required string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public required string Password { get; set; }
        }

       public async Task<IActionResult> OnPostAsync()
{
    if (!ModelState.IsValid)
    {
        return Page();
    }

    var user = new ApplicationUser
    {
        UserName = Input.Email,
        Email = Input.Email,
        LockoutEnabled = false,
    };

    var result = await _userManager.CreateAsync(user, Input.Password);

    if (result.Succeeded)
    {
        _logger.LogInformation("User created a new account with password.");

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        var confirmationLink = Url.Page(
            "/Account/ConfirmEmail",
            pageHandler: null,
            values: new { userId = user.Id, code = token },
            protocol: Request.Scheme);

        _logger.LogInformation("Email confirmation link: {Link}", confirmationLink);

        var emailSender = HttpContext.RequestServices.GetRequiredService<aspnetcoreapp.Services.EmailSender>();
        await emailSender.SendEmailAsync(
            toEmail: Input.Email,
            subject: "Confirm your email",
            body: $"Please confirm your email by clicking <a href='{confirmationLink}'>here</a>."
        );

        return RedirectToPage("/Index");
    }

    foreach (var error in result.Errors)
    {
        ModelState.AddModelError(string.Empty, error.Description);
        _logger.LogError("Error creating user: {Error}", error.Description);
    }

    return Page();
}
    }
}