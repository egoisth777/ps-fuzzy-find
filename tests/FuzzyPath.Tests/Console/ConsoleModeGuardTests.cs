using FuzzyPath.Console;
using NSubstitute;
using Xunit;

namespace FuzzyPath.Tests.Console;

public class ConsoleModeGuardTests
{
    private readonly IConsoleNative _native;
    private readonly IntPtr _inputHandle = new IntPtr(1);
    private readonly IntPtr _outputHandle = new IntPtr(2);

    public ConsoleModeGuardTests()
    {
        _native = Substitute.For<IConsoleNative>();
        _native.GetStdHandle(ConsoleInterop.STD_INPUT_HANDLE).Returns(_inputHandle);
        _native.GetStdHandle(ConsoleInterop.STD_OUTPUT_HANDLE).Returns(_outputHandle);

        _native.GetConsoleMode(_inputHandle, out Arg.Any<uint>())
            .Returns(x => { x[1] = 0x1Fu; return true; });
        _native.GetConsoleMode(_outputHandle, out Arg.Any<uint>())
            .Returns(x => { x[1] = 0x3u; return true; });
    }

    [Fact]
    public void Constructor_SavesInputAndOutputModes()
    {
        _ = new ConsoleModeGuard(_native);

        _native.Received(1).GetConsoleMode(_inputHandle, out Arg.Any<uint>());
        _native.Received(1).GetConsoleMode(_outputHandle, out Arg.Any<uint>());
    }

    [Fact]
    public void Constructor_FlushesInputBuffer()
    {
        _ = new ConsoleModeGuard(_native);

        _native.Received(1).FlushConsoleInputBuffer(_inputHandle);
    }

    [Fact]
    public void Dispose_RestoresInputMode()
    {
        var guard = new ConsoleModeGuard(_native);
        guard.Dispose();

        _native.Received(1).SetConsoleMode(_inputHandle, 0x1Fu);
    }

    [Fact]
    public void Dispose_RestoresOutputMode()
    {
        var guard = new ConsoleModeGuard(_native);
        guard.Dispose();

        _native.Received(1).SetConsoleMode(_outputHandle, 0x3u);
    }

    [Fact]
    public void Dispose_EmitsAlternateScreenExit()
    {
        var guard = new ConsoleModeGuard(_native);
        guard.Dispose();

        _native.Received(1).Write("\x1b[?1049l");
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var guard = new ConsoleModeGuard(_native);
        guard.Dispose();

        var exception = Record.Exception(() => guard.Dispose());
        Assert.Null(exception);

        // Verify SetConsoleMode was only called once per handle (not twice)
        _native.Received(1).SetConsoleMode(_inputHandle, 0x1Fu);
        _native.Received(1).SetConsoleMode(_outputHandle, 0x3u);
    }

    [Fact]
    public void Dispose_InputInvalid_SkipsInputRestore_StillRestoresOutput()
    {
        // Arrange: input handle returns false for GetConsoleMode
        _native.GetConsoleMode(_inputHandle, out Arg.Any<uint>())
            .Returns(x => { x[1] = 0u; return false; });

        var guard = new ConsoleModeGuard(_native);
        guard.Dispose();

        // Input should not be restored
        _native.DidNotReceive().SetConsoleMode(_inputHandle, Arg.Any<uint>());
        // Output should still be restored
        _native.Received(1).SetConsoleMode(_outputHandle, 0x3u);
        _native.Received(1).Write("\x1b[?1049l");
    }

    [Fact]
    public void Dispose_OutputInvalid_SkipsOutputRestore_StillRestoresInput()
    {
        // Arrange: output handle returns false for GetConsoleMode
        _native.GetConsoleMode(_outputHandle, out Arg.Any<uint>())
            .Returns(x => { x[1] = 0u; return false; });

        var guard = new ConsoleModeGuard(_native);
        guard.Dispose();

        // Input should still be restored
        _native.Received(1).SetConsoleMode(_inputHandle, 0x1Fu);
        // Output should not be restored, no alt-screen exit
        _native.DidNotReceive().SetConsoleMode(_outputHandle, Arg.Any<uint>());
        _native.DidNotReceive().Write("\x1b[?1049l");
    }

    [Fact]
    public void Constructor_InputInvalid_DoesNotFlush()
    {
        _native.GetConsoleMode(_inputHandle, out Arg.Any<uint>())
            .Returns(x => { x[1] = 0u; return false; });

        _ = new ConsoleModeGuard(_native);

        _native.DidNotReceive().FlushConsoleInputBuffer(_inputHandle);
    }
}
