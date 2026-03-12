namespace FuzzyPath.Models;

public sealed record SearchOptions(
    int MaxResults = 50_000,
    bool MatchPath = true,
    bool IncludeFiles = true,
    bool IncludeFolders = true);
