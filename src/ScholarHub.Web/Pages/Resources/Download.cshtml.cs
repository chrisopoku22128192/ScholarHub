using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ScholarHub.Web.Data;
using ScholarHub.Web.Services;

namespace ScholarHub.Web.Pages.Resources;

// Handler-only page: streams the underlying file for an approved resource
// (or one the requester owns / moderates). Requires authentication - see
// the "/Resources/Download" AuthorizePage convention in Program.cs.
public class DownloadModel : PageModel
{
    private readonly IResourceService _resourceService;

    public DownloadModel(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole(SeedData.AdminRole);

        try
        {
            var download = await _resourceService.PrepareDownloadAsync(id, userId, isAdmin, ct);
            return PhysicalFile(download.PhysicalPath, download.ContentType, download.OriginalFileName);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
