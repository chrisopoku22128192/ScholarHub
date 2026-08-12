using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScholarHub.Web.Data;
using ScholarHub.Web.Models;
using ScholarHub.Web.Models.Dto;
using ScholarHub.Web.Services;

namespace ScholarHub.Web.Controllers;

// RESTful API surface over the same IResourceService the Razor Pages UI
// uses, so both surfaces enforce identical rules. Auth is the same Identity
// cookie the UI uses (no separate token scheme) - fine for a single-app
// student project; a public client would need a bearer-token scheme instead.
[ApiController]
[Route("api/resources")]
public class ResourcesController : ControllerBase
{
    private readonly IResourceService _resourceService;

    public ResourcesController(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private bool IsAdmin => User.IsInRole(SeedData.AdminRole);

    /// <summary>Search approved resources by keyword/department/course/year/type.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResultDto<ResourceDto>>> Search([FromQuery] ResourceSearchQuery query, CancellationToken ct)
    {
        var result = await _resourceService.SearchApprovedAsync(query, ct);
        return Ok(new PagedResultDto<ResourceDto>(
            result.Items.Select(ResourceDto.FromEntity).ToList(),
            result.PageNumber, result.PageSize, result.TotalCount, result.TotalPages));
    }

    /// <summary>Get a resource if visible to the caller (approved for anyone; pending/rejected for its owner or an admin).</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ResourceDto>> GetById(int id, CancellationToken ct)
    {
        var resource = await _resourceService.GetVisibleAsync(id, CurrentUserId, IsAdmin, ct);
        return resource is null ? NotFound() : Ok(ResourceDto.FromEntity(resource));
    }

    /// <summary>The current user's own uploads, regardless of status.</summary>
    [HttpGet("mine")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<ResourceDto>>> Mine(CancellationToken ct)
    {
        var uploads = await _resourceService.GetMyUploadsAsync(CurrentUserId!, ct);
        return Ok(uploads.Select(ResourceDto.FromEntity).ToList());
    }

    /// <summary>Resources awaiting moderation.</summary>
    [HttpGet("pending")]
    [Authorize(Roles = SeedData.AdminRole)]
    public async Task<ActionResult<IReadOnlyList<ResourceDto>>> Pending(CancellationToken ct)
    {
        var pending = await _resourceService.GetPendingAsync(ct);
        return Ok(pending.Select(ResourceDto.FromEntity).ToList());
    }

    /// <summary>Upload a new resource. Always starts Pending until an admin approves it.</summary>
    [HttpPost]
    [Authorize]
    [RequestSizeLimit(30_000_000)]
    public async Task<ActionResult<ResourceDto>> Create([FromForm] ResourceUploadDto input, CancellationToken ct)
    {
        var resource = new Resource
        {
            Title = input.Title,
            Description = input.Description,
            Department = input.Department,
            CourseCode = input.CourseCode,
            AcademicYear = input.AcademicYear,
            Type = input.Type,
            UploadedByUserId = CurrentUserId!
        };

        try
        {
            var created = await _resourceService.CreateAsync(resource, input.File, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ResourceDto.FromEntity(created));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Approve a pending resource.</summary>
    [HttpPost("{id:int}/approve")]
    [Authorize(Roles = SeedData.AdminRole)]
    public async Task<IActionResult> Approve(int id, CancellationToken ct) =>
        await _resourceService.ApproveAsync(id, CurrentUserId!, ct) ? NoContent() : NotFound();

    /// <summary>Reject a pending resource with a reason.</summary>
    [HttpPost("{id:int}/reject")]
    [Authorize(Roles = SeedData.AdminRole)]
    public async Task<IActionResult> Reject(int id, [FromBody] ResourceRejectDto input, CancellationToken ct) =>
        await _resourceService.RejectAsync(id, CurrentUserId!, input.Reason, ct) ? NoContent() : NotFound();

    /// <summary>Download the underlying file, bumping the download counter.</summary>
    [HttpGet("{id:int}/download")]
    [Authorize]
    public async Task<IActionResult> Download(int id, CancellationToken ct)
    {
        try
        {
            var download = await _resourceService.PrepareDownloadAsync(id, CurrentUserId, IsAdmin, ct);
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

    /// <summary>Delete a resource. Allowed for its owner or an admin.</summary>
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var resource = await _resourceService.GetVisibleAsync(id, CurrentUserId, IsAdmin, ct);
        if (resource is null)
        {
            return NotFound();
        }

        if (!IsAdmin && resource.UploadedByUserId != CurrentUserId)
        {
            return Forbid();
        }

        await _resourceService.DeleteAsync(id, ct);
        return NoContent();
    }
}
