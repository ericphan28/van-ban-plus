# Convert PNG to ICO using .NET
param(
    [string]$SourcePng = "D:\AIVanBanCaNhan\image\ChatGPT Image Feb 9, 2026, 02_26_27 AM.png",
    [string]$OutputIco = "D:\AIVanBanCaNhan\AIVanBan.Desktop\Assets\app.ico"
)

Add-Type -AssemblyName System.Drawing

# Create output directory
$outDir = Split-Path $OutputIco
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

# Load source image
$srcImage = [System.Drawing.Image]::FromFile($SourcePng)

# ICO sizes needed (standard Windows icon sizes)
$sizes = @(16, 24, 32, 48, 64, 128, 256)

# Create ICO file manually
$ms = New-Object System.IO.MemoryStream

# ICO Header: Reserved(2) + Type(2) + Count(2)
$writer = New-Object System.IO.BinaryWriter($ms)
$writer.Write([UInt16]0)        # Reserved
$writer.Write([UInt16]1)        # Type: 1 = ICO
$writer.Write([UInt16]$sizes.Count)  # Number of images

# Calculate offset (header=6 + entries=16*count)
$dataOffset = 6 + (16 * $sizes.Count)

# Store PNG data for each size
$pngDataList = @()

foreach ($size in $sizes) {
    # Resize image
    $bmp = New-Object System.Drawing.Bitmap($size, $size)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
    $g.DrawImage($srcImage, 0, 0, $size, $size)
    $g.Dispose()

    # Save as PNG to memory
    $pngStream = New-Object System.IO.MemoryStream
    $bmp.Save($pngStream, [System.Drawing.Imaging.ImageFormat]::Png)
    $pngData = $pngStream.ToArray()
    $pngDataList += ,($pngData)
    $pngStream.Dispose()
    $bmp.Dispose()
}

# Write directory entries
foreach ($i in 0..($sizes.Count - 1)) {
    $size = $sizes[$i]
    $pngData = $pngDataList[$i]
    
    $widthByte = if ($size -ge 256) { 0 } else { $size }
    $heightByte = if ($size -ge 256) { 0 } else { $size }
    
    $writer.Write([byte]$widthByte)     # Width
    $writer.Write([byte]$heightByte)    # Height
    $writer.Write([byte]0)              # Color palette
    $writer.Write([byte]0)              # Reserved
    $writer.Write([UInt16]1)            # Color planes
    $writer.Write([UInt16]32)           # Bits per pixel
    $writer.Write([UInt32]$pngData.Length)  # Size of image data
    $writer.Write([UInt32]$dataOffset)     # Offset to image data
    
    $dataOffset += $pngData.Length
}

# Write image data
foreach ($pngData in $pngDataList) {
    $writer.Write($pngData)
}

# Save to file
$fileBytes = $ms.ToArray()
[System.IO.File]::WriteAllBytes($OutputIco, $fileBytes)

$writer.Dispose()
$ms.Dispose()
$srcImage.Dispose()

Write-Host "ICO file created: $OutputIco" -ForegroundColor Green
Write-Host "Sizes: $($sizes -join ', ')px" -ForegroundColor Gray
Write-Host "File size: $([math]::Round((Get-Item $OutputIco).Length/1KB, 1)) KB" -ForegroundColor Gray
