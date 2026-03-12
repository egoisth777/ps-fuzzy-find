using System.Management.Automation.Language;

namespace FuzzyPath.Buffer;

internal sealed class TokenResolver
{
    private static readonly TokenKind[] SkippedOperators =
    [
        TokenKind.Pipe,
        TokenKind.Semi,
        TokenKind.NewLine,
        TokenKind.Redirection,
    ];

    /// <summary>
    /// Resolves the token at the given cursor position from pre-parsed tokens.
    /// </summary>
    public (string token, int startOffset, int length)? Resolve(Token[] tokens, int cursorPosition)
    {
        if (tokens is null || tokens.Length == 0)
            return null;

        Token? best = null;

        foreach (var token in tokens)
        {
            // Skip EndOfInput token (always last)
            if (token.Kind == TokenKind.EndOfInput)
                continue;

            // Check if cursor is within this token's extent (inclusive on both ends)
            if (token.Extent.StartOffset <= cursorPosition &&
                token.Extent.EndOffset >= cursorPosition)
            {
                // Prefer the token with the largest StartOffset (most specific/innermost)
                if (best is null || token.Extent.StartOffset > best.Extent.StartOffset)
                {
                    best = token;
                }
            }
        }

        if (best is null)
            return null;

        // Skip comment tokens
        if (best.Kind == TokenKind.Comment)
            return null;

        // Skip certain operator tokens when cursor is ON them
        if (IsSkippedOperator(best))
            return null;

        return (best.Text, best.Extent.StartOffset, best.Extent.EndOffset - best.Extent.StartOffset);
    }

    /// <summary>
    /// Convenience: parses input and resolves token at cursor.
    /// </summary>
    public (string token, int startOffset, int length)? ResolveFromInput(string input, int cursorPosition)
    {
        if (string.IsNullOrEmpty(input))
            return null;
        if (cursorPosition < 0 || cursorPosition > input.Length)
            return null;

        Parser.ParseInput(input, out var tokens, out _);
        return Resolve(tokens, cursorPosition);
    }

    private static bool IsSkippedOperator(Token token)
    {
        // Check for redirection operators
        if (token is RedirectionToken)
            return true;

        foreach (var kind in SkippedOperators)
        {
            if (token.Kind == kind)
                return true;
        }

        return false;
    }
}
