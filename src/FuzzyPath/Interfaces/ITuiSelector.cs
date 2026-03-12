namespace FuzzyPath.Interfaces;

public interface ITuiSelector
{
    Task<string?> SelectAsync(IEnumerable<string> candidates, string? initialQuery, CancellationToken cancellationToken = default);
}
