using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ScholarHub.Web.Models;
using ScholarHub.Web.Services;

namespace ScholarHub.Web.Pages;

// Authenticated user's own upload history and status (see the "/MyUploads"
// AuthorizePage convention in Program.cs).
public class MyUploadsModel : PageModel
{
    private readonly IResourceService _resourceService;

    public MyUploadsModel(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    public IReadOnlyList<Resource> Uploads { get; private set; } = [];

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Uploads = await _resourceService.GetMyUploadsAsync(userId, ct);
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Only allow deleting resources the current user actually owns.
        var resource = await _resourceService.GetVisibleAsync(id, userId, isAdmin: false, ct);
        if (resource is null || resource.UploadedByUserId != userId)
        {
            return Forbid();
        }

        await _resourceService.DeleteAsync(id, ct);
        StatusMessage = "Resource deleted.";
        return RedirectToPage();
    }
}
