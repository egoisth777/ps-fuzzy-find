namespace FuzzyPath.Console;

internal sealed class ConsoleNativeWrapper : IConsoleNative
{
    public IntPtr GetStdHandle(int nStdHandle) => ConsoleInterop.GetStdHandle(nStdHandle);
    public bool GetConsoleMode(IntPtr handle, out uint mode) => ConsoleInterop.GetConsoleMode(handle, out mode);
    public bool SetConsoleMode(IntPtr handle, uint mode) => ConsoleInterop.SetConsoleMode(handle, mode);
    public bool FlushConsoleInputBuffer(IntPtr handle) => ConsoleInterop.FlushConsoleInputBuffer(handle);
    public void Write(string text) => System.Console.Write(text);
}
