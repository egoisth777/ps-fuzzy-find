namespace FuzzyPath.Quoting;

/// <summary>
/// Static utility class for PowerShell-safe path quoting.
/// </summary>
public static class PathQuoter
{
    private static readonly HashSet<char> SpecialChars = new()
    {
        '$', '[', ']', '#', ' ', '`', '{', '}', '(', ')', ';', '&', '|', '\''
    };

    /// <summary>
    /// Quotes a file-system path so it is safe to use in PowerShell expressions.
    /// </summary>
    public static string Quote(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        if (!NeedsQuoting(path))
            return path;

        bool hasSingleQuote = path.Contains('\'');
        bool hasDollar = path.Contains('$');

        // If no single quote → wrap in single quotes (safest, no escaping needed)
        if (!hasSingleQuote)
            return $"'{path}'";

        // Has single quote → must use double quotes
        // Escape inner double-quotes and backticks with backtick
        // If dollar sign present, escape $ with backtick too
        var escaped = EscapeForDoubleQuotes(path, escapeDollar: hasDollar);
        return $"\"{escaped}\"";
    }

    /// <summary>
    /// Returns true when <paramref name="path"/> contains characters that
    /// require quoting in PowerShell.
    /// </summary>
    public static bool NeedsQuoting(string path)
    {
        foreach (char c in path)
        {
            if (SpecialChars.Contains(c))
                return true;
        }

        return false;
    }

    private static string EscapeForDoubleQuotes(string path, bool escapeDollar)
    {
        // In PowerShell double-quoted strings:
        //   backtick (`) is the escape character
        //   double-quote (") must be escaped as `"
        //   dollar ($) must be escaped as `$ to prevent variable expansion
        //   backtick itself must be escaped as ``
        var sb = new System.Text.StringBuilder(path.Length + 8);

        foreach (char c in path)
        {
            switch (c)
            {
                case '`':
                    sb.Append("``");
                    break;
                case '"':
                    sb.Append("`\"");
                    break;
                case '$' when escapeDollar:
                    sb.Append("`$");
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }

        return sb.ToString();
    }
}
