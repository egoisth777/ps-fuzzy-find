namespace FuzzyPath.Console;

internal sealed class ConsoleModeGuard : IDisposable
{
    private readonly IConsoleNative _native;
    private readonly IntPtr _inputHandle;
    private readonly IntPtr _outputHandle;
    private readonly uint _savedInputMode;
    private readonly uint _savedOutputMode;
    private readonly bool _inputValid;
    private readonly bool _outputValid;
    private bool _disposed;

    public ConsoleModeGuard(IConsoleNative native)
    {
        _native = native;
        _inputHandle = _native.GetStdHandle(ConsoleInterop.STD_INPUT_HANDLE);
        _outputHandle = _native.GetStdHandle(ConsoleInterop.STD_OUTPUT_HANDLE);
        _inputValid = _native.GetConsoleMode(_inputHandle, out _savedInputMode);
        _outputValid = _native.GetConsoleMode(_outputHandle, out _savedOutputMode);
        if (_inputValid)
            _native.FlushConsoleInputBuffer(_inputHandle);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_inputValid)
        {
            _native.SetConsoleMode(_inputHandle, _savedInputMode);
            _native.FlushConsoleInputBuffer(_inputHandle);
        }
        if (_outputValid)
        {
            _native.SetConsoleMode(_outputHandle, _savedOutputMode);
            _native.Write("\x1b[?1049l");
        }
    }
}
