using Microsoft.AspNetCore.Identity;

namespace aspnetcoreapp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? TwoFactorSecretKey { get; set; }
        public new bool TwoFactorEnabled { get; internal set; }
    }
}