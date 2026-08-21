namespace AutoHub.Services;

public interface IMediaStorage
{
    string GetRelativePath(string brand, string model, string variant, string fileName);
    string GetAbsolutePath(string relativePath);
    bool FileExists(string relativePath);
}
