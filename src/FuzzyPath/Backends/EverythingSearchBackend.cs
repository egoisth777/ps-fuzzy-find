using FuzzyPath.Interfaces;
using FuzzyPath.Models;

namespace FuzzyPath.Backends;

public sealed class EverythingSearchBackend : ISearchBackend
{
    private readonly IEverythingNative _native;

    /// <summary>
    /// Production constructor using the real Everything64.dll P/Invoke wrapper.
    /// </summary>
    public EverythingSearchBackend() : this(new EverythingNativeWrapper()) { }

    /// <summary>
    /// Test constructor allowing injection of a mock IEverythingNative.
    /// </summary>
    internal EverythingSearchBackend(IEverythingNative native)
    {
        _native = native;
    }

    public Task<IReadOnlyList<SearchResult>> QueryAsync(string query, SearchOptions options, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled<IReadOnlyList<SearchResult>>(cancellationToken);

        if (string.IsNullOrEmpty(query))
            return Task.FromResult<IReadOnlyList<SearchResult>>(Array.Empty<SearchResult>());

        var searchQuery = BuildSearchQuery(query, options);

        // All Everything SDK calls must happen on the same thread (thread-local storage)
        _native.SetSearch(searchQuery);
        _native.SetRequestFlags(EverythingInterop.EVERYTHING_REQUEST_FILE_NAME | EverythingInterop.EVERYTHING_REQUEST_PATH);
        _native.SetMax((uint)Math.Max(0, options.MaxResults));
        _native.SetMatchPath(options.MatchPath);

        _native.Query(true);

        var errorCode = _native.GetLastError();
        if (errorCode == EverythingInterop.EVERYTHING_ERROR_IPC)
            throw new InvalidOperationException("Everything service is not running. Please start the Everything service and try again.");
        if (errorCode == EverythingInterop.EVERYTHING_ERROR_MEMORY)
            throw new InvalidOperationException("Everything service encountered a memory allocation error.");
        if (errorCode != EverythingInterop.EVERYTHING_OK)
            throw new InvalidOperationException($"Everything query failed with error code {errorCode}.");

        var numResults = _native.GetNumResults();
        var results = new List<SearchResult>((int)Math.Min(numResults, (uint)Math.Max(0, options.MaxResults)));

        for (uint i = 0; i < numResults; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fullPath = _native.GetResultFullPathName(i);
            var isFolder = _native.IsFolderResult(i);

            if (!options.IncludeFiles && !isFolder) continue;
            if (!options.IncludeFolders && isFolder) continue;

            results.Add(new SearchResult(fullPath, isFolder));
        }

        return Task.FromResult<IReadOnlyList<SearchResult>>(results);
    }

    private static string BuildSearchQuery(string query, SearchOptions options)
    {
        if (options.IncludeFiles && !options.IncludeFolders)
            return $"file:{query}";
        if (options.IncludeFolders && !options.IncludeFiles)
            return $"folder:{query}";
        return query;
    }
}
