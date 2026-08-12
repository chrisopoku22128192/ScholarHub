using Microsoft.AspNetCore.Identity;

namespace ScholarHub.Web.Models;

// Extends the built-in Identity user with the profile fields ScholarHub needs
// (full name for attribution on uploads, department for default filtering).
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public string? Department { get; set; }
}
