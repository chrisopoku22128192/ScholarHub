using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ScholarHub.Web.Models;

namespace ScholarHub.Web.Data;

public static class SeedData
{
    public const string AdminRole = "Admin";
    public const string StudentRole = "Student";

    // Runs once at startup: applies pending migrations, then ensures the
    // Admin/Student roles and a default admin account exist. There is no
    // self-serve admin signup — matching a real moderation model where admin
    // access is provisioned out of band, not requested at registration.
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var db = provider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { AdminRole, StudentRole })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
// Overridable via Seed:AdminEmail / Seed:AdminPassword so the
        // default credentials never have to be the ones used in a
        // real deployment.
        var config = provider.GetRequiredService<IConfiguration>();
        var adminEmail = config["Seed:AdminEmail"] ?? "admin@scholarhub.local";
        var adminPassword = config["Seed:AdminPassword"] ?? "Admin#12345";

        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "ScholarHub Admin",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to seed default admin account: {string.Join("; ", result.Errors.Select(e => e.Description))}");
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, AdminRole))
        {
            await userManager.AddToRoleAsync(adminUser, AdminRole);
        }
    }
}
