using Xunit;
using FuzzyPath.Quoting;

namespace FuzzyPath.Tests.Quoting;

public class PathQuoterTests
{
    // PQ-01: No special chars → returned unquoted
    [Theory]
    [InlineData(@"C:\Users\dev\file.txt")]
    [InlineData(@"C:\simple\path.log")]
    [InlineData(@"relative/path/file.cs")]
    public void Quote_PathWithNoSpecialChars_ReturnsUnquoted(string path)
    {
        string result = PathQuoter.Quote(path);
        Assert.Equal(path, result);
    }

    // PQ-02: Space → single-quoted
    [Fact]
    public void Quote_PathWithSpace_ReturnsSingleQuoted()
    {
        string result = PathQuoter.Quote(@"C:\Program Files\app.exe");
        Assert.Equal(@"'C:\Program Files\app.exe'", result);
    }

    // PQ-03: Dollar sign → single-quoted
    [Fact]
    public void Quote_PathWithDollar_ReturnsSingleQuoted()
    {
        string result = PathQuoter.Quote(@"C:\Users\$recycle.bin");
        Assert.Equal(@"'C:\Users\$recycle.bin'", result);
    }

    // PQ-04: Square brackets → single-quoted
    [Fact]
    public void Quote_PathWithSquareBrackets_ReturnsSingleQuoted()
    {
        string result = PathQuoter.Quote(@"C:\Data\[backup]\file.log");
        Assert.Equal(@"'C:\Data\[backup]\file.log'", result);
    }

    // PQ-05: Hash → single-quoted
    [Fact]
    public void Quote_PathWithHash_ReturnsSingleQuoted()
    {
        string result = PathQuoter.Quote(@"C:\Projects\#temp\notes.txt");
        Assert.Equal(@"'C:\Projects\#temp\notes.txt'", result);
    }

    // PQ-06: Pipe char → single-quoted
    [Fact]
    public void Quote_PathWithPipe_ReturnsSingleQuoted()
    {
        string result = PathQuoter.Quote(@"C:\Logs\error\|trace.log");
        Assert.Equal(@"'C:\Logs\error\|trace.log'", result);
    }

    // PQ-07: Single quote, no dollar → double-quoted
    [Fact]
    public void Quote_PathWithSingleQuote_ReturnsDoubleQuoted()
    {
        string result = PathQuoter.Quote(@"C:\Users\O'Brien\file.txt");
        Assert.Equal("\"C:\\Users\\O'Brien\\file.txt\"", result);
    }

    // PQ-08: Single quote AND dollar → double-quoted with escaped dollar
    [Fact]
    public void Quote_PathWithSingleQuoteAndDollar_ReturnsDoubleQuotedWithEscapedDollar()
    {
        string result = PathQuoter.Quote("C:\\it's $dir");
        Assert.Equal("\"C:\\it's `$dir\"", result);
    }

    // PQ-09: Empty string → empty string
    [Fact]
    public void Quote_EmptyString_ReturnsEmptyString()
    {
        string result = PathQuoter.Quote(string.Empty);
        Assert.Equal(string.Empty, result);
    }

    // PQ-10: Null → empty string
    [Fact]
    public void Quote_Null_ReturnsEmptyString()
    {
        string result = PathQuoter.Quote(null);
        Assert.Equal(string.Empty, result);
    }

    // PQ-11: Path containing single-quote chars → uses double-quote strategy
    [Fact]
    public void Quote_PathContainingSingleQuoteChars_UsesDoubleQuoteStrategy()
    {
        // The path itself contains literal ' characters
        string result = PathQuoter.Quote("'C:\\Program Files\\app.exe'");
        Assert.StartsWith("\"", result);
        Assert.EndsWith("\"", result);
    }

    // PQ-12: Path with backtick → single-quoted (backtick is a special char)
    [Fact]
    public void Quote_PathWithBacktick_ReturnsSingleQuoted()
    {
        string path = "C:\\Dir`name\\file.txt";
        string result = PathQuoter.Quote(path);
        Assert.Equal("'C:\\Dir`name\\file.txt'", result);
    }

    // PQ-13: UNC path with space → single-quoted
    [Fact]
    public void Quote_UncPathWithSpace_ReturnsSingleQuoted()
    {
        string result = PathQuoter.Quote(@"\\server\my share");
        Assert.Equal(@"'\\server\my share'", result);
    }

    // NeedsQuoting tests
    [Theory]
    [InlineData(@"C:\Users\dev\file.txt", false)]
    [InlineData(@"simple.txt", false)]
    [InlineData(@"C:\Program Files\app.exe", true)]
    [InlineData(@"path with spaces", true)]
    [InlineData(@"path$var", true)]
    [InlineData(@"path[0]", true)]
    public void NeedsQuoting_ReturnsExpected(string path, bool expected)
    {
        bool result = PathQuoter.NeedsQuoting(path);
        Assert.Equal(expected, result);
    }

    // Edge case: double quotes inside a path with single quote → backtick-escaped
    [Fact]
    public void Quote_PathWithSingleQuoteAndDoubleQuote_EscapesBothCorrectly()
    {
        string path = "C:\\it's a \"test\"";
        string result = PathQuoter.Quote(path);
        // Should use double-quote strategy since single quote is present
        // Inner " should be escaped as `"
        Assert.Equal("\"C:\\it's a `\"test`\"\"", result);
    }

    // Edge case: path with backtick and single quote → double-quoted with backtick escaped
    [Fact]
    public void Quote_PathWithBacktickAndSingleQuote_EscapesBacktickInDoubleQuotes()
    {
        string path = "C:\\it's`here";
        string result = PathQuoter.Quote(path);
        Assert.Equal("\"C:\\it's``here\"", result);
    }
}
