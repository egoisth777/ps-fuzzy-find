using System.Diagnostics;
using FuzzyPath.Interfaces;

namespace FuzzyPath.Tui;

public sealed class FzfSelector : ITuiSelector
{
    private readonly string _fzfPath;

    public FzfSelector(string? fzfPath = null)
    {
        _fzfPath = fzfPath ?? "fzf";
    }

    public async Task<string?> SelectAsync(IEnumerable<string> candidates, string? initialQuery, CancellationToken cancellationToken = default)
    {
        var psi = new ProcessStartInfo
        {
            FileName = _fzfPath,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = false, // fzf needs stderr for TUI rendering
            UseShellExecute = false,
            CreateNoWindow = false // fzf needs a console window
        };
        foreach (var arg in BuildArgumentList(initialQuery))
            psi.ArgumentList.Add(arg);

        Process process;
        try
        {
            process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start fzf process.");
        }
        catch (System.ComponentModel.Win32Exception)
        {
            throw new InvalidOperationException(
                "fzf executable not found. Ensure fzf is installed and on your PATH.");
        }

        try
        {
            // Dedicated writer task for stdin -- avoids backpressure deadlock
            var writerTask = Task.Run(async () =>
            {
                try
                {
                    await using var stdin = process.StandardInput;
                    foreach (var candidate in candidates)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;
                        await stdin.WriteLineAsync(candidate);
                    }
                }
                catch (IOException) { } // Process may have exited, pipe broken
                catch (ObjectDisposedException) { }
            });

            // Read stdout before WaitForExit to avoid deadlock
            var output = await process.StandardOutput.ReadLineAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);
            try { await writerTask; } catch { /* writer may fault on pipe closure */ }

            return process.ExitCode switch
            {
                0 => output?.Trim(),
                1 => null,   // No match
                130 => null, // User cancelled (Esc/Ctrl-C)
                _ => null    // Defensive
            };
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                try { process.Kill(entireProcessTree: true); } catch { }
            }
            return null;
        }
        finally
        {
            process.Dispose();
        }
    }

    internal static IReadOnlyList<string> BuildArgumentList(string? initialQuery)
    {
        var args = new List<string> { "--ansi", "--no-sort" };
        if (!string.IsNullOrEmpty(initialQuery))
        {
            args.Add("--query");
            args.Add(initialQuery);
        }
        return args;
    }
}
