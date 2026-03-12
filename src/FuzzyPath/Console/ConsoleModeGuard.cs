namespace FuzzyPath.Console;

internal sealed class ConsoleModeGuard : IDisposable
{
    private readonly IConsoleNative _native;
    private readonly IntPtr _inputHandle;
    private readonly IntPtr _outputHandle;
    private readonly uint _savedInputMode;
    private readonly uint _savedOutputMode;
    private readonly bool _validHandles;
    private bool _disposed;

    public ConsoleModeGuard(IConsoleNative native)
    {
        _native = native;
        _inputHandle = _native.GetStdHandle(ConsoleInterop.STD_INPUT_HANDLE);
        _outputHandle = _native.GetStdHandle(ConsoleInterop.STD_OUTPUT_HANDLE);
        var inputOk = _native.GetConsoleMode(_inputHandle, out _savedInputMode);
        var outputOk = _native.GetConsoleMode(_outputHandle, out _savedOutputMode);
        _validHandles = inputOk && outputOk;
        _native.FlushConsoleInputBuffer(_inputHandle);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_validHandles)
        {
            _native.SetConsoleMode(_inputHandle, _savedInputMode);
            _native.SetConsoleMode(_outputHandle, _savedOutputMode);
        }
        _native.Write("\x1b[?1049l");
        _native.FlushConsoleInputBuffer(_inputHandle);
    }
}
