namespace FuzzyPath.Console;

internal interface IConsoleNative
{
    IntPtr GetStdHandle(int nStdHandle);
    bool GetConsoleMode(IntPtr handle, out uint mode);
    bool SetConsoleMode(IntPtr handle, uint mode);
    bool FlushConsoleInputBuffer(IntPtr handle);
    void Write(string text);
}
