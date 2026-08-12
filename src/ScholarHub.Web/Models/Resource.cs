using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScholarHub.Web.Models;

// A single uploaded academic resource (past question / notes / slides) and its
// moderation + engagement metadata.
public class Resource
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required, StringLength(100)]
    public string Department { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string CourseCode { get; set; } = string.Empty;

    // Stored as "2023/2024" style academic year.
    [Required, StringLength(9)]
    public string AcademicYear { get; set; } = string.Empty;

    public ResourceType Type { get; set; }

    [Required]
    public string OriginalFileName { get; set; } = string.Empty;

    // Randomized name the file is actually saved under on disk, to avoid
    // collisions and to stop anyone guessing a path to an unapproved file.
    [Required]
    public string StoredFileName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public ResourceStatus Status { get; set; } = ResourceStatus.Pending;

    [StringLength(500)]
    public string? RejectionReason { get; set; }

    public int ViewCount { get; set; }

    public int DownloadCount { get; set; }

    [Required]
    public string UploadedByUserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UploadedByUserId))]
    public ApplicationUser? UploadedBy { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public string? ReviewedByUserId { get; set; }

    public DateTime? ReviewedAt { get; set; }
}
