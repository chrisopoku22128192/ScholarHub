using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ScholarHub.Web.Models;
using ScholarHub.Web.Services;

namespace ScholarHub.Web.Pages.Admin;

// Moderation console: everything in this folder is gated by the "AdminOnly"
// policy (see the AuthorizeFolder("/Admin", "AdminOnly") convention in
// Program.cs), so no per-page [Authorize] attribute is needed here.
public class IndexModel : PageModel
{
    private readonly IResourceService _resourceService;

    public IndexModel(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    public IReadOnlyList<Resource> Pending { get; private set; } = [];

    public IReadOnlyList<Resource> AllResources { get; private set; } = [];

    [BindProperty(SupportsGet = true)]
    public ResourceStatus? StatusFilter { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        Pending = await _resourceService.GetPendingAsync(ct);
        AllResources = await _resourceService.GetAllForAdminAsync(StatusFilter, ct);
    }

    public async Task<IActionResult> OnPostApproveAsync(int id, CancellationToken ct)
    {
        var reviewerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _resourceService.ApproveAsync(id, reviewerId, ct);
        StatusMessage = "Resource approved.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(int id, string reason, CancellationToken ct)
    {
        var reviewerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _resourceService.RejectAsync(id, reviewerId, reason, ct);
        StatusMessage = "Resource rejected.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken ct)
    {
        await _resourceService.DeleteAsync(id, ct);
        StatusMessage = "Resource removed.";
        return RedirectToPage();
    }
}
