using FuzzyPath.Tui;
using Xunit;

namespace FuzzyPath.Tests.Tui;

public class FzfSelectorTests
{
    [Fact]
    public async Task FzfNotFound_ThrowsInvalidOperationException()
    {
        var selector = new FzfSelector("__nonexistent_fzf_binary_test__");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => selector.SelectAsync(new[] { "a", "b" }, null));

        Assert.Contains("fzf", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildArgumentList_NullQuery_NoQueryFlag()
    {
        var result = FzfSelector.BuildArgumentList(null);

        Assert.Equal(new[] { "--ansi", "--no-sort" }, result);
        Assert.DoesNotContain("--query", result);
    }

    [Fact]
    public void BuildArgumentList_WithQuery_IncludesQueryFlag()
    {
        var result = FzfSelector.BuildArgumentList("test");

        Assert.Contains("--query", result);
        Assert.Contains("test", result);
        Assert.Equal(4, result.Count);
    }

    [Fact]
    public void BuildArgumentList_QueryWithQuotes_PreservesQuotesLiterally()
    {
        var result = FzfSelector.BuildArgumentList("hello \"world\"");

        Assert.Contains("--query", result);
        Assert.Contains("hello \"world\"", result);
    }

    [Trait("Category", "Integration")]
    [Fact]
    public async Task SelectAsync_WithRealFzf_EmptyCandidates_ReturnsNull()
    {
        // Skip if fzf is not installed
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "fzf",
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var p = System.Diagnostics.Process.Start(psi);
            if (p == null)
            {
                return; // Skip: fzf not available
            }
            await p.WaitForExitAsync();
            if (p.ExitCode != 0)
            {
                return; // Skip: fzf not working
            }
        }
        catch
        {
            return; // Skip: fzf not found
        }

        var selector = new FzfSelector();
        var result = await selector.SelectAsync(Enumerable.Empty<string>(), null);

        Assert.Null(result);
    }
}
