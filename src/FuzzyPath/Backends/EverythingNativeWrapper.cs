using System.Text;

namespace FuzzyPath.Backends;

internal sealed class EverythingNativeWrapper : IEverythingNative
{
    private const int BufferSize = 512;

    public uint SetSearch(string search) => EverythingInterop.Everything_SetSearchW(search);
    public void SetRequestFlags(uint flags) => EverythingInterop.Everything_SetRequestFlags(flags);
    public void SetMax(uint max) => EverythingInterop.Everything_SetMax(max);
    public void SetMatchPath(bool enable) => EverythingInterop.Everything_SetMatchPath(enable);
    public bool Query(bool wait) => EverythingInterop.Everything_QueryW(wait);
    public uint GetNumResults() => EverythingInterop.Everything_GetNumResults();

    public string GetResultFullPathName(uint index)
    {
        var sb = new StringBuilder(BufferSize);
        EverythingInterop.Everything_GetResultFullPathNameW(index, sb, (uint)sb.Capacity);
        return sb.ToString();
    }

    public bool IsFileResult(uint index) => EverythingInterop.Everything_IsFileResult(index);
    public bool IsFolderResult(uint index) => EverythingInterop.Everything_IsFolderResult(index);
    public uint GetLastError() => EverythingInterop.Everything_GetLastError();
    public uint GetMajorVersion() => EverythingInterop.Everything_GetMajorVersion();
    public void CleanUp() => EverythingInterop.Everything_CleanUp();
}
