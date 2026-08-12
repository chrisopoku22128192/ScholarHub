using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ScholarHub.Web.Data;
using ScholarHub.Web.Models;
using ScholarHub.Web.Services;

namespace ScholarHub.Web.Pages.Resources;

// Detail view for a single resource. Approved resources are visible to
// anyone; pending/rejected ones are only visible to their uploader or an
// admin (enforced by IResourceService.GetVisibleAsync).
public class DetailsModel : PageModel
{
    private readonly IResourceService _resourceService;

    public DetailsModel(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    public Resource Resource { get; private set; } = null!;

    public bool CanDownload { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole(SeedData.AdminRole);

        var resource = await _resourceService.GetVisibleAsync(id, userId, isAdmin, ct);
        if (resource is null)
        {
            return NotFound();
        }

        Resource = resource;
        CanDownload = User.Identity?.IsAuthenticated == true;

        await _resourceService.RegisterViewAsync(id, ct);

        return Page();
    }
}
