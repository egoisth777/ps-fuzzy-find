using FuzzyPath.Backends;
using FuzzyPath.Models;
using Xunit;

namespace FuzzyPath.Tests.Backends;

public class FileSystemSearchBackendTests : IDisposable
{
    private readonly string _tempDir;

    public FileSystemSearchBackendTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "FuzzyPath_Tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private void CreateFile(string relativePath)
    {
        var fullPath = Path.Combine(_tempDir, relativePath);
        var dir = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(dir);
        File.WriteAllText(fullPath, string.Empty);
    }

    private void CreateDir(string relativePath)
    {
        Directory.CreateDirectory(Path.Combine(_tempDir, relativePath));
    }

    [Fact]
    public async Task FS01_ReturnsMatchingFiles()
    {
        CreateFile("report.txt");
        CreateFile("readme.md");
        CreateFile("data.csv");

        var backend = new FileSystemSearchBackend(_tempDir);
        var results = await backend.QueryAsync("read", new SearchOptions());

        Assert.Contains(results, r => r.FullPath.EndsWith("readme.md") && !r.IsDirectory);
    }

    [Fact]
    public async Task FS02_ReturnsMatchingFolders()
    {
        CreateDir("docs");
        CreateDir("images");
        CreateDir("downloads");

        var backend = new FileSystemSearchBackend(_tempDir);
        var results = await backend.QueryAsync("doc", new SearchOptions());

        Assert.Contains(results, r => r.FullPath.EndsWith("docs") && r.IsDirectory);
    }

    [Fact]
    public async Task FS03_NoMatchesReturnsEmpty()
    {
        CreateFile("file1.txt");
        CreateFile("file2.txt");

        var backend = new FileSystemSearchBackend(_tempDir);
        var results = await backend.QueryAsync("zzz_nomatch", new SearchOptions());

        Assert.Empty(results);
    }

    [Fact]
    public async Task FS04_RespectsMaxResults()
    {
        for (var i = 0; i < 20; i++)
            CreateFile($"match_{i:D2}.txt");

        var backend = new FileSystemSearchBackend(_tempDir);
        var options = new SearchOptions(MaxResults: 5);
        var results = await backend.QueryAsync("match", options);

        Assert.Equal(5, results.Count);
    }

    [Fact]
    public async Task FS05_NonExistentDirectoryReturnsEmpty()
    {
        var backend = new FileSystemSearchBackend(Path.Combine(_tempDir, "does_not_exist"));
        var results = await backend.QueryAsync("anything", new SearchOptions());

        Assert.Empty(results);
    }

    [Fact]
    public async Task FS06_FileOnlyFilter()
    {
        CreateFile("file_a.txt");
        CreateDir("folder_a");

        var backend = new FileSystemSearchBackend(_tempDir);
        var options = new SearchOptions(IncludeFolders: false);
        var results = await backend.QueryAsync("", options);

        Assert.All(results, r => Assert.False(r.IsDirectory));
        Assert.Contains(results, r => r.FullPath.EndsWith("file_a.txt"));
    }

    [Fact]
    public async Task FS07_FolderOnlyFilter()
    {
        CreateFile("file_b.txt");
        CreateDir("folder_b");

        var backend = new FileSystemSearchBackend(_tempDir);
        var options = new SearchOptions(IncludeFiles: false);
        var results = await backend.QueryAsync("", options);

        Assert.All(results, r => Assert.True(r.IsDirectory));
        Assert.Contains(results, r => r.FullPath.EndsWith("folder_b"));
    }

    [Fact]
    public async Task FS08_EmptyQueryReturnsAllEntries()
    {
        CreateFile("alpha.txt");
        CreateFile("beta.txt");
        CreateDir("gamma");

        var backend = new FileSystemSearchBackend(_tempDir);
        var results = await backend.QueryAsync("", new SearchOptions());

        Assert.Equal(3, results.Count);
    }
}
