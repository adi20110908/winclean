$ErrorActionPreference = 'Stop'

$source = Join-Path $PSScriptRoot 'src\Winclean.cs'
$outputDir = Join-Path $PSScriptRoot 'dist'
$output = Join-Path $outputDir 'Winclean.exe'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path $compiler)) {
    $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
}
if (-not (Test-Path $compiler)) {
    throw 'The Windows .NET Framework C# compiler was not found. Install the .NET Framework 4.8 developer tools or build with a .NET SDK.'
}
if (-not (Test-Path $source)) { throw "Source file not found: $source" }
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

& $compiler /nologo /target:winexe /platform:anycpu /optimize+ /out:$output `
    /reference:System.dll /reference:System.Core.dll /reference:System.Drawing.dll `
    /reference:System.Windows.Forms.dll /reference:System.Security.dll /reference:Microsoft.VisualBasic.dll $source
if ($LASTEXITCODE -ne 0) { throw "C# compilation failed with exit code $LASTEXITCODE" }

Write-Host "Built portable executable: $output"
