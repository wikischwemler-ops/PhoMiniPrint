# Erzeugt src/PhoMiniPrint.App/PhoMiniPrint.ico aus dem Programm-Icon
# (src/PhoMiniPrint.App/Ressourcen/ProgrammIcon.xaml, einzige Quelle).
# Größen 16–256 Pixel, PNG-komprimiert. Aufruf: powershell -File tools\icon_erzeugen.ps1

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName PresentationFramework, PresentationCore, WindowsBase

$wurzel = Split-Path -Parent $PSScriptRoot
$quelle = Join-Path $wurzel 'src\PhoMiniPrint.App\Ressourcen\ProgrammIcon.xaml'
$ziel = Join-Path $wurzel 'src\PhoMiniPrint.App\PhoMiniPrint.ico'

$woerterbuch = [Windows.Markup.XamlReader]::Parse([IO.File]::ReadAllText($quelle))
$bild = $woerterbuch['ProgrammIcon']

$groessen = 16, 20, 24, 32, 40, 48, 64, 128, 256
$pngs = foreach ($g in $groessen) {
    $visual = New-Object Windows.Media.DrawingVisual
    $dc = $visual.RenderOpen()
    $dc.DrawImage($bild, (New-Object Windows.Rect 0, 0, $g, $g))
    $dc.Close()
    $bitmap = New-Object Windows.Media.Imaging.RenderTargetBitmap $g, $g, 96, 96, ([Windows.Media.PixelFormats]::Pbgra32)
    $bitmap.Render($visual)
    $encoder = New-Object Windows.Media.Imaging.PngBitmapEncoder
    $encoder.Frames.Add([Windows.Media.Imaging.BitmapFrame]::Create($bitmap))
    $speicher = New-Object IO.MemoryStream
    $encoder.Save($speicher)
    , $speicher.ToArray()
}

# ICO-Datei: Kopf, Verzeichnis, Bilddaten.
$aus = New-Object IO.MemoryStream
$w = New-Object IO.BinaryWriter $aus
$w.Write([UInt16]0); $w.Write([UInt16]1); $w.Write([UInt16]$groessen.Count)
$versatz = 6 + 16 * $groessen.Count
for ($i = 0; $i -lt $groessen.Count; $i++) {
    $g = $groessen[$i]
    $kante = if ($g -ge 256) { 0 } else { $g }
    $w.Write([byte]$kante); $w.Write([byte]$kante); $w.Write([byte]0); $w.Write([byte]0)
    $w.Write([UInt16]1); $w.Write([UInt16]32)
    $w.Write([UInt32]$pngs[$i].Length); $w.Write([UInt32]$versatz)
    $versatz += $pngs[$i].Length
}
foreach ($png in $pngs) { $w.Write($png) }
$w.Flush()
[IO.File]::WriteAllBytes($ziel, $aus.ToArray())
Write-Host "Icon erzeugt: $ziel ($($aus.Length) Byte, Größen $($groessen -join ', '))"
