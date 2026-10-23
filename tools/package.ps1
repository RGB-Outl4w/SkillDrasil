# Builds dist/SkillDrasil-<version>.zip in Thunderstore layout (also used as the GitHub release asset):
#   manifest.json, icon.png (256x256), README.md, CHANGELOG.md, LICENSE, plugins/SkillDrasil.dll
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
Set-Location $root
$version = (Get-Content manifest.json -Raw | ConvertFrom-Json).version_number
if ((Select-String src\Plugin.cs -Pattern "Version = `"$version`"" -Quiet) -ne $true) { throw "Plugin.cs version does not match manifest $version" }

# icon.png from assets/logo.svg via headless Edge (transparent corners)
$edge = "${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe"
$html = Join-Path $env:TEMP 'skilldrasil-logo.html'
Set-Content $html ("<html><body style='margin:0;background:transparent'>" + ((Get-Content assets\logo.svg -Raw) -replace '<svg ', '<svg width="256" height="256" style="display:block" ') + "</body></html>") -Encoding UTF8
$ErrorActionPreference = 'Continue' # Edge reports progress on stderr
& $edge --headless=new --disable-gpu --hide-scrollbars --force-device-scale-factor=1 --default-background-color=00000000 `
    --window-size=256,256 --screenshot="$root\icon.png" "file:///$($html -replace '\\','/')" 2>&1 | Out-Null
$ErrorActionPreference = 'Stop'
Start-Sleep -Seconds 2
if (-not (Test-Path icon.png)) { throw 'icon render failed' }

dotnet build SkillDrasil.csproj -c Release -nologo -v q | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'build failed' }

# Explicit entries: Compress-Archive on PowerShell 5.1 writes backslash paths, which Thunderstore rejects.
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
New-Item -ItemType Directory -Force dist | Out-Null
$zip = Join-Path $root "dist\SkillDrasil-$version.zip"
Remove-Item $zip -ErrorAction SilentlyContinue
$entries = [ordered]@{
    'manifest.json' = 'manifest.json'; 'icon.png' = 'icon.png'; 'README.md' = 'README.md'
    'CHANGELOG.md' = 'CHANGELOG.md'; 'LICENSE' = 'LICENSE'; 'plugins/SkillDrasil.dll' = 'bin\Release\net48\SkillDrasil.dll'
}
$archive = [System.IO.Compression.ZipFile]::Open($zip, 'Create')
try {
    foreach ($e in $entries.GetEnumerator()) {
        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, (Join-Path $root $e.Value), $e.Key) | Out-Null
    }
} finally { $archive.Dispose() }
"packaged $zip"
