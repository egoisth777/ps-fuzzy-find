using System.Runtime.InteropServices;
using System.Text;

namespace FuzzyPath.Backends;

internal static class EverythingInterop
{
    private const string DllName = "Everything64.dll";

    // Error codes
    internal const uint EVERYTHING_OK = 0;
    internal const uint EVERYTHING_ERROR_MEMORY = 1;
    internal const uint EVERYTHING_ERROR_IPC = 2;
    internal const uint EVERYTHING_ERROR_REGISTERCLASSEX = 3;
    internal const uint EVERYTHING_ERROR_CREATEWINDOW = 4;
    internal const uint EVERYTHING_ERROR_CREATETHREAD = 5;
    internal const uint EVERYTHING_ERROR_INVALIDINDEX = 6;
    internal const uint EVERYTHING_ERROR_INVALIDCALL = 7;

    // Request flags
    internal const uint EVERYTHING_REQUEST_FILE_NAME = 0x00000001;
    internal const uint EVERYTHING_REQUEST_PATH = 0x00000002;

    [DllImport(DllName, CharSet = CharSet.Unicode)]
    internal static extern uint Everything_SetSearchW(string search);

    [DllImport(DllName)]
    internal static extern void Everything_SetRequestFlags(uint flags);

    [DllImport(DllName)]
    internal static extern void Everything_SetMax(uint max);

    [DllImport(DllName)]
    internal static extern void Everything_SetMatchPath(bool enable);

    [DllImport(DllName)]
    internal static extern bool Everything_QueryW(bool wait);

    [DllImport(DllName)]
    internal static extern uint Everything_GetNumResults();

    [DllImport(DllName, CharSet = CharSet.Unicode)]
    internal static extern void Everything_GetResultFullPathNameW(uint index, StringBuilder buf, uint bufSize);

    [DllImport(DllName)]
    internal static extern bool Everything_IsFileResult(uint index);

    [DllImport(DllName)]
    internal static extern bool Everything_IsFolderResult(uint index);

    [DllImport(DllName)]
    internal static extern uint Everything_GetLastError();

    [DllImport(DllName)]
    internal static extern uint Everything_GetMajorVersion();

    [DllImport(DllName)]
    internal static extern void Everything_CleanUp();
}
