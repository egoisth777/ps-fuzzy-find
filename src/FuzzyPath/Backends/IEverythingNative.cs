namespace FuzzyPath.Backends;

internal interface IEverythingNative
{
    uint SetSearch(string search);
    void SetRequestFlags(uint flags);
    void SetMax(uint max);
    void SetMatchPath(bool enable);
    bool Query(bool wait);
    uint GetNumResults();
    string GetResultFullPathName(uint index);
    bool IsFileResult(uint index);
    bool IsFolderResult(uint index);
    uint GetLastError();
    uint GetMajorVersion();
    void CleanUp();
}
