using FuzzyPath.Models;

namespace FuzzyPath.Interfaces;

public interface ISearchBackend
{
    Task<IReadOnlyList<SearchResult>> QueryAsync(string query, SearchOptions options, CancellationToken cancellationToken = default);
}
