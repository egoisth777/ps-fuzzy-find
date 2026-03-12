using FuzzyPath.Interfaces;
using FuzzyPath.Models;
using FuzzyPath.Quoting;

namespace FuzzyPath.Pipeline;

public sealed class FuzzyPathPipeline
{
    private readonly ISearchBackend _searchBackend;
    private readonly ITuiSelector _tuiSelector;
    private readonly IBufferContext _bufferContext;
    private readonly SearchOptions _options;
    private readonly Action<Exception>? _onError;

    public FuzzyPathPipeline(
        ISearchBackend searchBackend,
        ITuiSelector tuiSelector,
        IBufferContext bufferContext,
        SearchOptions? options = null,
        Action<Exception>? onError = null)
    {
        _searchBackend = searchBackend;
        _tuiSelector = tuiSelector;
        _bufferContext = bufferContext;
        _options = options ?? new SearchOptions();
        _onError = onError;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Resolve token at cursor
            var tokenInfo = _bufferContext.ResolveTokenAtCursor();
            string query = tokenInfo?.token ?? "";

            // 2. Search
            var results = await _searchBackend.QueryAsync(query, _options, cancellationToken);

            // 3. Select via TUI
            var candidates = results.Select(r => r.FullPath);
            var selection = await _tuiSelector.SelectAsync(candidates, tokenInfo?.token, cancellationToken);

            // 4. If user cancelled or no selection, do nothing
            if (string.IsNullOrEmpty(selection))
                return;

            // 5. Quote the selected path
            string quoted = PathQuoter.Quote(selection);

            // 6. Replace token or insert at cursor
            if (tokenInfo.HasValue)
            {
                _bufferContext.ReplaceToken(tokenInfo.Value.startOffset, tokenInfo.Value.length, quoted);
            }
            else
            {
                var state = _bufferContext.GetBufferState();
                _bufferContext.ReplaceToken(state.CursorPosition, 0, quoted);
            }
        }
        catch (OperationCanceledException)
        {
            // Cancellation is not an error — silently return
        }
        catch (Exception ex)
        {
            _onError?.Invoke(ex);
        }
    }
}
