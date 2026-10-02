using Microsoft.EntityFrameworkCore;
using ScholarHub.Web.Data;
using ScholarHub.Web.Models;

namespace ScholarHub.Web.Services;

public class ResourceService : IResourceService
{
    private readonly ApplicationDbContext _db;
    private readonly IFileStorageService _storage;

    public ResourceService(ApplicationDbContext db, IFileStorageService storage)
    {
        _db = db;
        _storage = storage;
    }
     /// <summary>Filters approved resources by keyword/department
    /// /course/year/type, paginated.</summary>
    public async Task<PagedResult<Resource>> SearchApprovedAsync(ResourceSearchQuery query, CancellationToken ct = default)
    {
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = query.PageSize is > 0 and <= 50 ? query.PageSize : 10;

        var q = _db.Resources.AsNoTracking().Where(r => r.Status == ResourceStatus.Approved);

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.Trim();
            q = q.Where(r =>
                r.Title.Contains(keyword) ||
                (r.Description != null && r.Description.Contains(keyword)) ||
                r.CourseCode.Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(query.Department))
        {
            q = q.Where(r => r.Department == query.Department);
        }

        if (!string.IsNullOrWhiteSpace(query.CourseCode))
        {
            q = q.Where(r => r.CourseCode.Contains(query.CourseCode));
        }

        if (!string.IsNullOrWhiteSpace(query.AcademicYear))
        {
            q = q.Where(r => r.AcademicYear == query.AcademicYear);
        }

        if (query.Type.HasValue)
        {
            q = q.Where(r => r.Type == query.Type);
        }

        var totalCount = await q.CountAsync(ct);

        var items = await q
            .Include(r => r.UploadedBy)
            .OrderByDescending(r => r.UploadedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Resource>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Resource?> GetVisibleAsync(int id, string? requestingUserId, bool isAdmin, CancellationToken ct = default)
    {
        var resource = await _db.Resources.Include(r => r.UploadedBy).FirstOrDefaultAsync(r => r.Id == id, ct);
        if (resource is null)
        {
            return null;
        }

        var isVisible = resource.Status == ResourceStatus.Approved
            || isAdmin
            || (requestingUserId is not null && resource.UploadedByUserId == requestingUserId);

        return isVisible ? resource : null;
    }

    public async Task<IReadOnlyList<Resource>> GetMyUploadsAsync(string userId, CancellationToken ct = default) =>
        await _db.Resources.AsNoTracking()
            .Where(r => r.UploadedByUserId == userId)
            .OrderByDescending(r => r.UploadedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Resource>> GetPendingAsync(CancellationToken ct = default) =>
        await _db.Resources.AsNoTracking()
            .Include(r => r.UploadedBy)
            .Where(r => r.Status == ResourceStatus.Pending)
            .OrderBy(r => r.UploadedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Resource>> GetAllForAdminAsync(ResourceStatus? statusFilter, CancellationToken ct = default)
    {
        var q = _db.Resources.AsNoTracking().Include(r => r.UploadedBy).AsQueryable();
        if (statusFilter.HasValue)
        {
            q = q.Where(r => r.Status == statusFilter);
        }

        return await q.OrderByDescending(r => r.UploadedAt).ToListAsync(ct);
    }

    public async Task<Resource> CreateAsync(Resource resource, IFormFile file, CancellationToken ct = default)
    {
        var storedFileName = await _storage.SaveAsync(file, ct);

        resource.OriginalFileName = Path.GetFileName(file.FileName);
        resource.StoredFileName = storedFileName;
        resource.ContentType = file.ContentType;
        resource.FileSizeBytes = file.Length;
        resource.Status = ResourceStatus.Pending;
        resource.UploadedAt = DateTime.UtcNow;

        _db.Resources.Add(resource);
        await _db.SaveChangesAsync(ct);

        return resource;
    }

    public async Task<bool> ApproveAsync(int id, string reviewerId, CancellationToken ct = default)
    {
        var resource = await _db.Resources.FindAsync([id], ct);
        if (resource is null)
        {
            return false;
        }

        resource.Status = ResourceStatus.Approved;
        resource.RejectionReason = null;
        resource.ReviewedByUserId = reviewerId;
        resource.ReviewedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> RejectAsync(int id, string reviewerId, string reason, CancellationToken ct = default)
    {
        var resource = await _db.Resources.FindAsync([id], ct);
        if (resource is null)
        {
            return false;
        }

        resource.Status = ResourceStatus.Rejected;
        resource.RejectionReason = reason;
        resource.ReviewedByUserId = reviewerId;
        resource.ReviewedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var resource = await _db.Resources.FindAsync([id], ct);
        if (resource is null)
        {
            return false;
        }

        _storage.Delete(resource.StoredFileName);
        _db.Resources.Remove(resource);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task RegisterViewAsync(int id, CancellationToken ct = default)
    {
        // Avoid loading the whole entity/tracking graph for a simple counter bump.
        await _db.Resources.Where(r => r.Id == id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.ViewCount, r => r.ViewCount + 1), ct);
    }

    public async Task<DownloadResult> PrepareDownloadAsync(int id, string? requestingUserId, bool isAdmin, CancellationToken ct = default)
    {
        var resource = await _db.Resources.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Resource {id} not found.");

        var canDownload = resource.Status == ResourceStatus.Approved
            || isAdmin
            || (requestingUserId is not null && resource.UploadedByUserId == requestingUserId);

        if (!canDownload)
        {
            throw new UnauthorizedAccessException("This resource is not available for download.");
        }

        resource.DownloadCount += 1;
        await _db.SaveChangesAsync(ct);

        return new DownloadResult(_storage.GetPhysicalPath(resource.StoredFileName), resource.OriginalFileName, resource.ContentType);
    }
}
