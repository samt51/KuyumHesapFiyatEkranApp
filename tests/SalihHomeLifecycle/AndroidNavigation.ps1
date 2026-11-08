param(
    [Parameter(Mandatory = $true)][string]$Adb,
    [string]$Serial = 'emulator-5554'
)
$ErrorActionPreference = 'Stop'
$package = 'com.salihcetinkaya.kuyumhesap'

function Invoke-Adb {
    param([string[]]$Arguments)
    $output = & $Adb -s $Serial @Arguments
    if ($LASTEXITCODE -ne 0) { throw "ADB failed: $Arguments" }
    return $output
}

function Read-Screen {
    Invoke-Adb @('shell', 'uiautomator', 'dump', '/sdcard/salih-navigation.xml') | Out-Null
    [xml]$screen = (Invoke-Adb @('shell', 'cat', '/sdcard/salih-navigation.xml')) -join "`n"
    return $screen
}

$titles = @('Hızlı İşlemler', 'Altın Fiyatları', 'Altın Hesaplama', 'İletişim', 'Hakkımızda')
$tabs = @('Ana Sayfa', 'Fiyatlar', 'Hesaplama', 'İletişim', 'Hakkımızda')
$initialProcess = (Invoke-Adb @('shell', 'pidof', $package)) -join ''
$screen = Read-Screen
$transitionCount = 0

# Each ordered pair is exercised. Fail immediately instead of tapping the
# Android launcher after a crash and accidentally counting that as navigation.
foreach ($source in 0..4) {
    foreach ($target in 0..4) {
        if ($source -eq $target) { continue }
        foreach ($index in @($source, $target)) {
            $label = $tabs[$index]
            $node = $screen.SelectNodes("//node[@text='$label']") |
                Sort-Object { [int]([regex]::Match($_.bounds, '^\[\d+,(\d+)\]').Groups[1].Value) } |
                Select-Object -Last 1
            if (-not $node) { throw "Bottom tab missing: $label" }
            $bounds = [regex]::Matches($node.bounds, '\d+') | ForEach-Object { [int]$_.Value }
            $x = [int](($bounds[0] + $bounds[2]) / 2)
            $y = [int](($bounds[1] + $bounds[3]) / 2)
            Invoke-Adb @('shell', 'input', 'tap', "$x", "$y") | Out-Null
            $screen = Read-Screen
            $currentProcess = (Invoke-Adb @('shell', 'pidof', $package)) -join ''
            if ($currentProcess -ne $initialProcess) { throw 'Application process restarted during navigation.' }
            $heading = $titles[$index]
            $headingNode = $screen.SelectNodes("//node[@text='$heading' and @package='$package']") |
                Where-Object { [int]([regex]::Match($_.bounds, '^\[\d+,(\d+)\]').Groups[1].Value) -lt $bounds[1] }
            if (-not $headingNode) { throw "Target page did not appear: $heading" }
        }
        $transitionCount++
        Write-Output "PASS $transitionCount : $($tabs[$source]) -> $($tabs[$target])"
    }
}
Write-Output "PASS: all $transitionCount directed tab pairs, same app process throughout."
