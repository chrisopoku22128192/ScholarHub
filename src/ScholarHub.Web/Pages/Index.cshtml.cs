using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ScholarHub.Web.Models;
using ScholarHub.Web.Services;

namespace ScholarHub.Web.Pages;

// Public resource library: keyword/department/course/year/type search over
// approved resources only. No sign-in required to browse or search.
public class IndexModel : PageModel
{
    private readonly IResourceService _resourceService;

    public IndexModel(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    [BindProperty(SupportsGet = true)]
    public string? Keyword { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Department { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? CourseCode { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? AcademicYear { get; set; }

    [BindProperty(SupportsGet = true)]
    public ResourceType? Type { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public PagedResult<Resource> Results { get; private set; } = new();

    public string[] Departments { get; } = Models.Departments.All;

    public async Task OnGetAsync(CancellationToken ct)
    {
        Results = await _resourceService.SearchApprovedAsync(new ResourceSearchQuery
        {
            Keyword = Keyword,
            Department = Department,
            CourseCode = CourseCode,
            AcademicYear = AcademicYear,
            Type = Type,
            PageNumber = PageNumber
        }, ct);
    }
}
