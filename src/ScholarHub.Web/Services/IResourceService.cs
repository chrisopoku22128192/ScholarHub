using ScholarHub.Web.Models;

namespace ScholarHub.Web.Services;

public record DownloadResult(string PhysicalPath, string OriginalFileName, string ContentType);

// Central business logic for resources, shared by the Razor Pages UI and the
// REST API controller so both surfaces enforce the same rules exactly once.
public interface IResourceService
{
    Task<PagedResult<Resource>> SearchApprovedAsync(ResourceSearchQuery query, CancellationToken ct = default);

    /// <summary>
    /// Returns the resource if it is visible to the requester: approved for
    /// anyone, or pending/rejected for its uploader or an admin. Null otherwise.
    /// </summary>
    Task<Resource?> GetVisibleAsync(int id, string? requestingUserId, bool isAdmin, CancellationToken ct = default);

    Task<IReadOnlyList<Resource>> GetMyUploadsAsync(string userId, CancellationToken ct = default);

    Task<IReadOnlyList<Resource>> GetPendingAsync(CancellationToken ct = default);

    Task<IReadOnlyList<Resource>> GetAllForAdminAsync(ResourceStatus? statusFilter, CancellationToken ct = default);

    Task<Resource> CreateAsync(Resource resource, IFormFile file, CancellationToken ct = default);

    Task<bool> ApproveAsync(int id, string reviewerId, CancellationToken ct = default);

    Task<bool> RejectAsync(int id, string reviewerId, string reason, CancellationToken ct = default);

    /// <summary>Removes the resource row and its underlying file (moderation removal or owner cleanup).</summary>
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);

    Task RegisterViewAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Validates the requester may download the resource and increments its
    /// download counter. Throws <see cref="KeyNotFoundException"/> if missing
    /// or <see cref="UnauthorizedAccessException"/> if not visible to them.
    /// </summary>
    Task<DownloadResult> PrepareDownloadAsync(int id, string? requestingUserId, bool isAdmin, CancellationToken ct = default);
}
