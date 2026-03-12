using FuzzyPath.Interfaces;
using FuzzyPath.Models;

namespace FuzzyPath.Backends;

public sealed class FileSystemSearchBackend : ISearchBackend
{
    private readonly string _baseDirectory;

    public FileSystemSearchBackend(string baseDirectory)
    {
        _baseDirectory = baseDirectory;
    }

    public Task<IReadOnlyList<SearchResult>> QueryAsync(
        string query, SearchOptions options, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => EnumerateEntries(query, options, cancellationToken), cancellationToken);
    }

    private IReadOnlyList<SearchResult> EnumerateEntries(
        string query, SearchOptions options, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_baseDirectory))
            return Array.Empty<SearchResult>();

        var enumerationOptions = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true
        };

        var results = new List<SearchResult>();
        var hasQuery = !string.IsNullOrEmpty(query);

        try
        {
            foreach (var entry in Directory.EnumerateFileSystemEntries(
                         _baseDirectory, "*", enumerationOptions))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var isDirectory = Directory.Exists(entry);

                if (isDirectory && !options.IncludeFolders)
                    continue;
                if (!isDirectory && !options.IncludeFiles)
                    continue;

                if (hasQuery)
                {
                    var name = Path.GetFileName(entry);
                    if (name.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                }

                results.Add(new SearchResult(entry, isDirectory));

                if (results.Count >= options.MaxResults)
                    break;
            }
        }
        catch (DirectoryNotFoundException)
        {
            // Base directory removed during enumeration — return what we have
        }
        catch (UnauthorizedAccessException)
        {
            // Permissions changed during enumeration — return what we have
        }

        return results;
    }
}
