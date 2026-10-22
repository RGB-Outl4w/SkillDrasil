# Renders assets/icons/*.svg into one atlas (assets/atlas.png) plus its index (assets/atlas.txt),
# using headless Edge. Cells are 128 px, 8 per row, in file-name order.
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$icons = Get-ChildItem (Join-Path $root 'assets\icons') -Filter *.svg | Sort-Object Name
$cell = 128; $cols = 8
$rows = [Math]::Ceiling($icons.Count / $cols)

$cells = foreach ($i in $icons) {
    $svg = (Get-Content $i.FullName -Raw) -replace '<svg ', "<svg width=`"$cell`" height=`"$cell`" "
    "<div>$svg</div>"
}
$html = @"
<!doctype html><html><head><style>
html,body{margin:0;padding:0;background:transparent}
body{display:grid;grid-template-columns:repeat($cols,${cell}px);grid-auto-rows:${cell}px}
div{width:${cell}px;height:${cell}px}svg{display:block}
</style></head><body>$($cells -join "`n")</body></html>
"@
$tmp = Join-Path $env:TEMP 'skilldrasil-atlas.html'
Set-Content $tmp $html -Encoding UTF8

$edge = "${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe"
$png = Join-Path $root 'assets\atlas.png'
& $edge --headless=new --disable-gpu --hide-scrollbars --force-device-scale-factor=1 `
    --default-background-color=00000000 --window-size="$($cols * $cell),$($rows * $cell)" `
    --screenshot="$png" "file:///$($tmp -replace '\\','/')" | Out-Null
Start-Sleep -Seconds 2
if (-not (Test-Path $png)) { throw 'Edge did not write the atlas' }

Set-Content (Join-Path $root 'assets\atlas.txt') ("$cell $cols`n" + (($icons | ForEach-Object { $_.BaseName }) -join "`n")) -Encoding ASCII -NoNewline
"atlas: $($icons.Count) icons, $($cols * $cell)x$($rows * $cell)"
