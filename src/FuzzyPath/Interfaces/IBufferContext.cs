using FuzzyPath.Models;

namespace FuzzyPath.Interfaces;

public interface IBufferContext
{
    BufferState GetBufferState();
    void ReplaceToken(int startOffset, int length, string replacement);
    (string token, int startOffset, int length)? ResolveTokenAtCursor();
}
