using ScholarHub.Web.Models;

namespace ScholarHub.Web.Services;

// Filter/paging parameters shared by the Razor Pages library view and the
// REST API's GET /api/resources endpoint.
public class ResourceSearchQuery
{
    public string? Keyword { get; set; }
    public string? Department { get; set; }
    public string? CourseCode { get; set; }
    public string? AcademicYear { get; set; }
    public ResourceType? Type { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
