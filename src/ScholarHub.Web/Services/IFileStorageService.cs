namespace ScholarHub.Web.Services;

public interface IFileStorageService
{
    /// <summary>Extensions ScholarHub accepts for resource uploads.</summary>
    IReadOnlySet<string> AllowedExtensions { get; }

    long MaxFileSizeBytes { get; }

    /// <summary>
    /// Persists the file to storage and returns the randomized name it was
    /// saved under. Throws <see cref="InvalidOperationException"/> if the
    /// extension or size isn't allowed.
    /// </summary>
    Task<string> SaveAsync(IFormFile file, CancellationToken cancellationToken = default);

    /// <summary>Full physical path for a previously stored file name.</summary>
    string GetPhysicalPath(string storedFileName);

    void Delete(string storedFileName);
}
