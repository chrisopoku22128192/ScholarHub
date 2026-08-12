using System.ComponentModel.DataAnnotations;

namespace ScholarHub.Web.Models.Dto;

// API response shape for a resource. Deliberately flattens UploadedBy down
// to id/name rather than serializing ApplicationUser directly, which would
// otherwise leak PasswordHash/SecurityStamp/etc. through the navigation
// property.
public record ResourceDto(
    int Id,
    string Title,
    string? Description,
    string Department,
    string CourseCode,
    string AcademicYear,
    ResourceType Type,
    string OriginalFileName,
    long FileSizeBytes,
    ResourceStatus Status,
    string? RejectionReason,
    int ViewCount,
    int DownloadCount,
    string? UploadedByUserId,
    string? UploadedByName,
    DateTime UploadedAt)
{
    public static ResourceDto FromEntity(Resource r) => new(
        r.Id, r.Title, r.Description, r.Department, r.CourseCode, r.AcademicYear, r.Type,
        r.OriginalFileName, r.FileSizeBytes, r.Status, r.RejectionReason, r.ViewCount, r.DownloadCount,
        r.UploadedByUserId, r.UploadedBy?.FullName, r.UploadedAt);
}

public record PagedResultDto<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalCount, int TotalPages);

// Multipart upload payload for POST /api/resources.
public class ResourceUploadDto
{
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required, StringLength(100)]
    public string Department { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string CourseCode { get; set; } = string.Empty;

    [Required, StringLength(9)]
    public string AcademicYear { get; set; } = string.Empty;

    [Required]
    public ResourceType Type { get; set; }

    [Required]
    public IFormFile File { get; set; } = null!;
}

public class ResourceRejectDto
{
    [Required, StringLength(500)]
    public string Reason { get; set; } = string.Empty;
}
