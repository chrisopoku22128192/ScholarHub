using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ScholarHub.Web.Models;
using ScholarHub.Web.Services;

namespace ScholarHub.Web.Pages;

// Authenticated upload form (see the "/Upload" AuthorizePage convention in
// Program.cs). New resources always start Pending and need admin approval
// before they appear in search.
public class UploadModel : PageModel
{
    private readonly IResourceService _resourceService;

    public UploadModel(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string[] Departments { get; } = Models.Departments.All;

    [TempData]
    public string? StatusMessage { get; set; }

    public class InputModel
    {
        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required, StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required, StringLength(20)]
        [Display(Name = "Course code")]
        public string CourseCode { get; set; } = string.Empty;

        [Required, StringLength(9)]
        [Display(Name = "Academic year")]
        [RegularExpression(@"^\d{4}/\d{4}$", ErrorMessage = "Use the format 2023/2024.")]
        public string AcademicYear { get; set; } = string.Empty;

        [Required]
        public ResourceType Type { get; set; }

        [Required(ErrorMessage = "Please choose a file to upload.")]
        [Display(Name = "File (PDF, DOCX or PPTX)")]
        public IFormFile? File { get; set; }
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var resource = new Resource
        {
            Title = Input.Title,
            Description = Input.Description,
            Department = Input.Department,
            CourseCode = Input.CourseCode,
            AcademicYear = Input.AcademicYear,
            Type = Input.Type,
            UploadedByUserId = userId
        };

        try
        {
            await _resourceService.CreateAsync(resource, Input.File!, ct);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(Input.File), ex.Message);
            return Page();
        }

        StatusMessage = "Your resource was submitted and is now pending admin approval.";
        return RedirectToPage("/MyUploads");
    }
}
