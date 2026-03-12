using Xunit;
using NSubstitute;
using FuzzyPath.Backends;
using FuzzyPath.Models;

namespace FuzzyPath.Tests.Backends;

public class EverythingSearchBackendTests
{
    private readonly IEverythingNative _native;
    private readonly EverythingSearchBackend _sut;

    public EverythingSearchBackendTests()
    {
        _native = Substitute.For<IEverythingNative>();
        _sut = new EverythingSearchBackend(_native);
    }

    private void SetupSuccessfulQuery(uint numResults)
    {
        _native.GetLastError().Returns(EverythingInterop.EVERYTHING_OK);
        _native.GetNumResults().Returns(numResults);
    }

    [Fact]
    public async Task ES01_Query_WithOkErrorCode_ReturnsCorrectResults()
    {
        // Arrange
        SetupSuccessfulQuery(3);
        _native.GetResultFullPathName(0u).Returns(@"C:\file1.txt");
        _native.GetResultFullPathName(1u).Returns(@"C:\file2.txt");
        _native.GetResultFullPathName(2u).Returns(@"C:\folder1");
        _native.IsFolderResult(0u).Returns(false);
        _native.IsFolderResult(1u).Returns(false);
        _native.IsFolderResult(2u).Returns(true);

        var options = new SearchOptions();

        // Act
        var results = await _sut.QueryAsync("test", options);

        // Assert
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public async Task ES02_Query_WithIpcError_ThrowsInvalidOperationException()
    {
        // Arrange
        _native.GetLastError().Returns(EverythingInterop.EVERYTHING_ERROR_IPC);
        var options = new SearchOptions();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.QueryAsync("test", options));
        Assert.Contains("Everything service is not running", ex.Message);
    }

    [Fact]
    public async Task ES03_Query_WithMemoryError_ThrowsInvalidOperationException()
    {
        // Arrange
        _native.GetLastError().Returns(EverythingInterop.EVERYTHING_ERROR_MEMORY);
        var options = new SearchOptions();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.QueryAsync("test", options));
        Assert.Contains("memory allocation error", ex.Message);
    }

    [Fact]
    public async Task ES04_Query_MapsFullPathAndIsDirectoryCorrectly()
    {
        // Arrange
        SetupSuccessfulQuery(2);
        _native.GetResultFullPathName(0u).Returns(@"C:\Documents\report.pdf");
        _native.GetResultFullPathName(1u).Returns(@"C:\Projects");
        _native.IsFolderResult(0u).Returns(false);
        _native.IsFolderResult(1u).Returns(true);

        var options = new SearchOptions();

        // Act
        var results = await _sut.QueryAsync("test", options);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal(@"C:\Documents\report.pdf", results[0].FullPath);
        Assert.False(results[0].IsDirectory);
        Assert.Equal(@"C:\Projects", results[1].FullPath);
        Assert.True(results[1].IsDirectory);
    }

    [Fact]
    public async Task ES05_Query_ForwardsMaxResultsToSetMax()
    {
        // Arrange
        SetupSuccessfulQuery(0);
        var options = new SearchOptions(MaxResults: 100);

        // Act
        await _sut.QueryAsync("test", options);

        // Assert
        _native.Received(1).SetMax(100u);
    }

    [Fact]
    public async Task ES06_Query_WithIncludeFoldersFalse_FiltersOutFolders()
    {
        // Arrange
        SetupSuccessfulQuery(2);
        _native.GetResultFullPathName(0u).Returns(@"C:\file.txt");
        _native.GetResultFullPathName(1u).Returns(@"C:\folder");
        _native.IsFolderResult(0u).Returns(false);
        _native.IsFolderResult(1u).Returns(true);

        var options = new SearchOptions(IncludeFolders: false);

        // Act
        var results = await _sut.QueryAsync("test", options);

        // Assert
        Assert.Single(results);
        Assert.Equal(@"C:\file.txt", results[0].FullPath);
        Assert.False(results[0].IsDirectory);
    }

    [Fact]
    public async Task ES07_Query_WithEmptyQuery_ReturnsEmptyWithoutCallingQuery()
    {
        // Arrange
        var options = new SearchOptions();

        // Act
        var results = await _sut.QueryAsync("", options);

        // Assert
        Assert.Empty(results);
        _native.DidNotReceive().Query(Arg.Any<bool>());
    }

    [Fact]
    public async Task ES08_Query_WithPreCancelledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var options = new SearchOptions();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => _sut.QueryAsync("test", options, cts.Token));
    }
}
