namespace ScholarHub.Web.Services;

// Stores uploaded resource files on the local disk, outside wwwroot, so they
// can only ever reach a browser through the authorized Download handler
// (which checks approval status and bumps the download counter) rather than
// being directly link-able as static files.
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadsRoot;

    public IReadOnlySet<string> AllowedExtensions { get; } =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".pdf", ".docx", ".pptx" };

    public long MaxFileSizeBytes { get; } = 25 * 1024 * 1024; // 25 MB

    public LocalFileStorageService(IWebHostEnvironment env, IConfiguration config)
    {
        var configuredPath = config["Storage:UploadsPath"] ?? "App_Data/Uploads";
        _uploadsRoot = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(env.ContentRootPath, configuredPath);

        Directory.CreateDirectory(_uploadsRoot);
    }

    public async Task<string> SaveAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length == 0)
        {
            throw new InvalidOperationException("The selected file is empty.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException($"The file exceeds the {MaxFileSizeBytes / (1024 * 1024)} MB limit.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Only PDF, DOCX, and PPTX files are accepted.");
        }

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(_uploadsRoot, storedFileName);

        await using var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write);
        await file.CopyToAsync(stream, cancellationToken);

        return storedFileName;
    }

    public string GetPhysicalPath(string storedFileName) => Path.Combine(_uploadsRoot, storedFileName);

    public void Delete(string storedFileName)
    {
        var path = GetPhysicalPath(storedFileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
