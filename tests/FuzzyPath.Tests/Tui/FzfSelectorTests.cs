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
    public void BuildArguments_NullQuery_NoQueryFlag()
    {
        var result = FzfSelector.BuildArguments(null);

        Assert.Equal("--ansi --no-sort", result);
        Assert.DoesNotContain("--query", result);
    }

    [Fact]
    public void BuildArguments_WithQuery_IncludesQueryFlag()
    {
        var result = FzfSelector.BuildArguments("test");

        Assert.Contains("--query=\"test\"", result);
    }

    [Fact]
    public void BuildArguments_QueryWithQuotes_EscapesQuotes()
    {
        var result = FzfSelector.BuildArguments("hello \"world\"");

        Assert.Contains("--query=\"hello \\\"world\\\"\"", result);
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
