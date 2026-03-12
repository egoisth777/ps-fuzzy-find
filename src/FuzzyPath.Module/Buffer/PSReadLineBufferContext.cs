using FuzzyPath.Buffer;
using FuzzyPath.Interfaces;
using FuzzyPath.Models;
using Microsoft.PowerShell;

namespace FuzzyPath.Module.Buffer;

public sealed class PSReadLineBufferContext : IBufferContext
{
    private readonly TokenResolver _tokenResolver;

    internal PSReadLineBufferContext(TokenResolver? tokenResolver = null)
    {
        _tokenResolver = tokenResolver ?? new TokenResolver();
    }

    public BufferState GetBufferState()
    {
        PSConsoleReadLine.GetBufferState(out string input, out int cursor);
        return new BufferState(input, cursor);
    }

    public void ReplaceToken(int startOffset, int length, string replacement)
    {
        PSConsoleReadLine.Replace(startOffset, length, replacement);
    }

    public (string token, int startOffset, int length)? ResolveTokenAtCursor()
    {
        var state = GetBufferState();
        return _tokenResolver.ResolveFromInput(state.Content, state.CursorPosition);
    }
}
