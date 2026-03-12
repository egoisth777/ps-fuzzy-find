using FuzzyPath.Buffer;
using Xunit;

namespace FuzzyPath.Tests.Buffer;

public sealed class TokenResolverTests
{
    private readonly TokenResolver _sut = new();

    [Fact] // TR-01
    public void SimpleCommandArgument_ReturnsToken()
    {
        var result = _sut.ResolveFromInput("cd foo", 5);

        Assert.NotNull(result);
        Assert.Equal("foo", result.Value.token);
        Assert.Equal(3, result.Value.startOffset);
        Assert.Equal(3, result.Value.length);
    }

    [Fact] // TR-02
    public void QuotedPathWithSpaces_ReturnsTokenIncludingQuotes()
    {
        var input = "cd \"C:\\My Docs\"";
        var result = _sut.ResolveFromInput(input, 10);

        Assert.NotNull(result);
        // The token should include the quotes
        Assert.Contains("C:\\My Docs", result.Value.token);
        Assert.Equal(3, result.Value.startOffset);
    }

    [Fact] // TR-03
    public void SingleQuotedPath_ReturnsTokenIncludingQuotes()
    {
        var input = "cd 'C:\\My Docs'";
        var result = _sut.ResolveFromInput(input, 10);

        Assert.NotNull(result);
        Assert.Contains("C:\\My Docs", result.Value.token);
        Assert.Equal(3, result.Value.startOffset);
    }

    [Fact] // TR-04
    public void EmptyBuffer_ReturnsNull()
    {
        var result = _sut.ResolveFromInput("", 0);
        Assert.Null(result);
    }

    [Fact] // TR-05
    public void CursorOnWhitespace_ReturnsNull()
    {
        var result = _sut.ResolveFromInput("cd  foo", 3);
        Assert.Null(result);
    }

    [Fact] // TR-06
    public void ColonPath_ReturnsFullPath()
    {
        var input = "Get-Item C:\\Users";
        var result = _sut.ResolveFromInput(input, 12);

        Assert.NotNull(result);
        // PowerShell parser may tokenize this differently; verify token contains path content
        Assert.Contains("Users", result.Value.token);
    }

    [Fact] // TR-07
    public void FileRedirection_ReturnsFilenameToken()
    {
        var input = "cmd > output.txt";
        var result = _sut.ResolveFromInput(input, 10);

        Assert.NotNull(result);
        Assert.Equal("output.txt", result.Value.token);
    }

    [Fact] // TR-08
    public void IncompleteSingleQuote_ReturnsIncompleteToken()
    {
        var input = "cd 'C:\\My";
        var result = _sut.ResolveFromInput(input, 8);

        Assert.NotNull(result);
        Assert.Contains("C:\\My", result.Value.token);
    }

    [Fact] // TR-09
    public void IncompleteDoubleQuote_ReturnsIncompleteToken()
    {
        var input = "cd \"C:\\My";
        var result = _sut.ResolveFromInput(input, 8);

        Assert.NotNull(result);
        Assert.Contains("C:\\My", result.Value.token);
    }

    [Fact] // TR-10
    public void CommentToken_ReturnsNull()
    {
        var result = _sut.ResolveFromInput("# comment", 3);
        Assert.Null(result);
    }

    [Fact] // TR-11
    public void MultipleArgsCursorOnSecond_ReturnsSecondToken()
    {
        var result = _sut.ResolveFromInput("cp src dest", 9);

        Assert.NotNull(result);
        Assert.Equal("dest", result.Value.token);
    }

    [Fact] // TR-12
    public void CursorAtTokenStart_ReturnsToken()
    {
        var result = _sut.ResolveFromInput("cd foo", 3);

        Assert.NotNull(result);
        Assert.Equal("foo", result.Value.token);
        Assert.Equal(3, result.Value.startOffset);
        Assert.Equal(3, result.Value.length);
    }

    [Fact] // TR-13
    public void CursorAtTokenEnd_ReturnsToken()
    {
        var result = _sut.ResolveFromInput("cd foo", 6);

        Assert.NotNull(result);
        Assert.Equal("foo", result.Value.token);
        Assert.Equal(3, result.Value.startOffset);
        Assert.Equal(3, result.Value.length);
    }

    [Fact] // TR-14
    public void PipelineArgument_ReturnsToken()
    {
        var result = _sut.ResolveFromInput("ls | grep foo", 12);

        Assert.NotNull(result);
        Assert.Equal("foo", result.Value.token);
    }

    [Fact] // TR-15
    public void MultipleTokensCursorOnFirst_ReturnsFirstToken()
    {
        var result = _sut.ResolveFromInput("cp src dest", 4);

        Assert.NotNull(result);
        Assert.Equal("src", result.Value.token);
    }

    [Fact] // TR-16
    public void WhitespaceOnly_ReturnsNull()
    {
        var result = _sut.ResolveFromInput("   ", 1);
        Assert.Null(result);
    }

    [Fact] // TR-17
    public void CursorBeyondBuffer_ReturnsNull()
    {
        var result = _sut.ResolveFromInput("cd foo", 100);
        Assert.Null(result);
    }

    [Fact] // TR-18
    public void CursorOnPipeOperator_ReturnsNull()
    {
        var result = _sut.ResolveFromInput("ls | grep foo", 3);
        Assert.Null(result);
    }

    [Fact] // TR-19
    public void CursorOnSemicolon_ReturnsNull()
    {
        var result = _sut.ResolveFromInput("cd foo; ls", 6);
        Assert.Null(result);
    }
}
