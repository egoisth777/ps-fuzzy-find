using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using FuzzyPath.Interfaces;
using FuzzyPath.Models;
using FuzzyPath.Pipeline;
using FuzzyPath.Quoting;

namespace FuzzyPath.Tests.Pipeline;

public class FuzzyPathPipelineTests
{
    private readonly ISearchBackend _searchBackend = Substitute.For<ISearchBackend>();
    private readonly ITuiSelector _tuiSelector = Substitute.For<ITuiSelector>();
    private readonly IBufferContext _bufferContext = Substitute.For<IBufferContext>();

    private FuzzyPathPipeline CreatePipeline(Action<Exception>? onError = null)
        => new(_searchBackend, _tuiSelector, _bufferContext, onError: onError);

    [Fact]
    public async Task ExecuteAsync_WithToken_SearchesAndReplacesToken()
    {
        // Arrange
        _bufferContext.ResolveTokenAtCursor()
            .Returns(("foo", 5, 3));

        _searchBackend.QueryAsync("foo", Arg.Any<SearchOptions>(), Arg.Any<CancellationToken>())
            .Returns(new List<SearchResult> { new("C:\\selected\\path.txt", false) });

        _tuiSelector.SelectAsync(Arg.Any<IEnumerable<string>>(), "foo", Arg.Any<CancellationToken>())
            .Returns("C:\\selected\\path.txt");

        var pipeline = CreatePipeline();

        // Act
        await pipeline.ExecuteAsync();

        // Assert — no special chars so path is returned unquoted
        _bufferContext.Received(1).ReplaceToken(5, 3, "C:\\selected\\path.txt");
    }

    [Fact]
    public async Task ExecuteAsync_NoToken_InsertsAtCursor()
    {
        // Arrange
        _bufferContext.ResolveTokenAtCursor()
            .Returns((ValueTuple<string, int, int>?)null);

        _bufferContext.GetBufferState()
            .Returns(new BufferState("ls ", 3));

        _searchBackend.QueryAsync("", Arg.Any<SearchOptions>(), Arg.Any<CancellationToken>())
            .Returns(new List<SearchResult> { new("C:\\selected\\path.txt", false) });

        _tuiSelector.SelectAsync(Arg.Any<IEnumerable<string>>(), null, Arg.Any<CancellationToken>())
            .Returns("C:\\selected\\path.txt");

        var pipeline = CreatePipeline();

        // Act
        await pipeline.ExecuteAsync();

        // Assert — insert at cursor position with length 0
        _bufferContext.Received(1).ReplaceToken(3, 0, "C:\\selected\\path.txt");
    }

    [Fact]
    public async Task ExecuteAsync_UserCancels_NoReplace()
    {
        // Arrange
        _bufferContext.ResolveTokenAtCursor()
            .Returns(("foo", 5, 3));

        _searchBackend.QueryAsync("foo", Arg.Any<SearchOptions>(), Arg.Any<CancellationToken>())
            .Returns(new List<SearchResult> { new("C:\\selected\\path.txt", false) });

        _tuiSelector.SelectAsync(Arg.Any<IEnumerable<string>>(), "foo", Arg.Any<CancellationToken>())
            .Returns((string?)null);

        var pipeline = CreatePipeline();

        // Act
        await pipeline.ExecuteAsync();

        // Assert
        _bufferContext.DidNotReceive().ReplaceToken(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_EmptyResults_StillCallsSelector()
    {
        // Arrange
        _bufferContext.ResolveTokenAtCursor()
            .Returns(("foo", 5, 3));

        _searchBackend.QueryAsync("foo", Arg.Any<SearchOptions>(), Arg.Any<CancellationToken>())
            .Returns(new List<SearchResult>());

        _tuiSelector.SelectAsync(Arg.Any<IEnumerable<string>>(), "foo", Arg.Any<CancellationToken>())
            .Returns((string?)null);

        var pipeline = CreatePipeline();

        // Act
        await pipeline.ExecuteAsync();

        // Assert — selector is still invoked even with no results
        await _tuiSelector.Received(1).SelectAsync(Arg.Any<IEnumerable<string>>(), "foo", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_BackendThrows_CallsOnError()
    {
        // Arrange
        var capturedException = (Exception?)null;
        var expectedException = new InvalidOperationException("Everything not running");

        _bufferContext.ResolveTokenAtCursor()
            .Returns(("foo", 5, 3));

        _searchBackend.QueryAsync("foo", Arg.Any<SearchOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(expectedException);

        var pipeline = CreatePipeline(onError: ex => capturedException = ex);

        // Act
        await pipeline.ExecuteAsync();

        // Assert
        Assert.Same(expectedException, capturedException);
        _bufferContext.DidNotReceive().ReplaceToken(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_CancellationException_DoesNotCallOnError()
    {
        // Arrange
        var errorCalled = false;

        _bufferContext.ResolveTokenAtCursor()
            .Returns(("foo", 5, 3));

        _searchBackend.QueryAsync("foo", Arg.Any<SearchOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());

        var pipeline = CreatePipeline(onError: _ => errorCalled = true);

        // Act
        await pipeline.ExecuteAsync();

        // Assert — cancellation should not invoke onError
        Assert.False(errorCalled);
        _bufferContext.DidNotReceive().ReplaceToken(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_PathWithSpecialChars_QuotesCorrectly()
    {
        // Arrange
        _bufferContext.ResolveTokenAtCursor()
            .Returns(("foo", 5, 3));

        _searchBackend.QueryAsync("foo", Arg.Any<SearchOptions>(), Arg.Any<CancellationToken>())
            .Returns(new List<SearchResult> { new("C:\\my folder\\file [1].txt", false) });

        _tuiSelector.SelectAsync(Arg.Any<IEnumerable<string>>(), "foo", Arg.Any<CancellationToken>())
            .Returns("C:\\my folder\\file [1].txt");

        var pipeline = CreatePipeline();

        // Act
        await pipeline.ExecuteAsync();

        // Assert — path has spaces and brackets, should be single-quoted
        _bufferContext.Received(1).ReplaceToken(5, 3, "'C:\\my folder\\file [1].txt'");
    }
}
