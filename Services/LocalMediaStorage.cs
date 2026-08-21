namespace AutoHub.Services;

public sealed class LocalMediaStorage : IMediaStorage
{
    private readonly IWebHostEnvironment _env;

    public LocalMediaStorage(IWebHostEnvironment env)
    {
        _env = env;
    }

    public string GetRelativePath(string brand, string model, string variant, string fileName)
    {
        return $"/Images/{brand}/{model}/{fileName}";
    }

    public string GetAbsolutePath(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return string.Empty;
        var trimmed = relativePath.TrimStart('/');
        return Path.Combine(_env.WebRootPath, trimmed.Replace('/', Path.DirectorySeparatorChar));
    }

    public bool FileExists(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return false;
        return File.Exists(GetAbsolutePath(relativePath));
    }
}
