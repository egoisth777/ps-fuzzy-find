@{
    RootModule = 'FuzzyPath.Module.dll'
    ModuleVersion = '0.1.0'
    GUID = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'
    Author = 'FuzzyPath Contributors'
    CompanyName = 'Community'
    Copyright = '(c) 2026 FuzzyPath Contributors. All rights reserved.'
    Description = 'Interactive fuzzy path completion for PowerShell using Everything search and fzf.'
    PowerShellVersion = '7.4'
    RequiredModules = @('PSReadLine')
    CmdletsToExport = @('Invoke-FuzzyPath', 'Enable-FuzzyPath', 'Test-FuzzyPathEnvironment')
    FunctionsToExport = @()
    AliasesToExport = @()
    PrivateData = @{
        PSData = @{
            Tags = @('fuzzy', 'path', 'completion', 'everything', 'fzf', 'PSReadLine')
            ProjectUri = ''
        }
    }
}
