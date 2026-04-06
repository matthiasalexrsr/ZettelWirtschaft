namespace DocuDesk.Testing;

public sealed class TemporaryDirectoryScope : IDisposable
{
    public TemporaryDirectoryScope()
    {
        Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "docudesk-tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(Path);
    }

    public string Path { get; }

    public string CreateFile(string relativePath, string content)
    {
        var fullPath = EnsureParentDirectory(relativePath);
        File.WriteAllText(fullPath, content);
        return fullPath;
    }

    public string CreateBinaryFile(string relativePath, byte[] content)
    {
        var fullPath = EnsureParentDirectory(relativePath);
        File.WriteAllBytes(fullPath, content);
        return fullPath;
    }

    private string EnsureParentDirectory(string relativePath)
    {
        var fullPath = System.IO.Path.Combine(Path, relativePath);
        var parent = System.IO.Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(parent))
        {
            Directory.CreateDirectory(parent);
        }

        return fullPath;
    }

    public void Dispose()
    {
        if (!Directory.Exists(Path))
        {
            return;
        }

        try
        {
            Directory.Delete(Path, recursive: true);
        }
        catch (IOException)
        {
            // Best-effort cleanup for open handles during failed test runs.
        }
        catch (UnauthorizedAccessException)
        {
            // Best-effort cleanup for open handles during failed test runs.
        }
    }
}
