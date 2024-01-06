# 设置错误处理策略为停止脚本执行
$ErrorActionPreference = "Stop"

# 设置FFmpeg可执行文件的路径
$ffmpegFilePath = "$PSScriptRoot/ffmpeg.exe"

# 检查FFmpeg文件是否已存在
if (Test-Path $ffmpegFilePath) {
    Write-Host "跳过了下载FFmpeg，文件已经存在。"
    exit
}

Write-Host "正在下载FFmpeg..."

# 配置安全协议以支持TLS 1.2
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

# 创建WebClient对象用于下载文件
$http = New-Object System.Net.WebClient
try {
    # 下载FFmpeg压缩包
    $http.DownloadFile("https://github.com/Tyrrrz/FFmpegBin/releases/download/6.1/ffmpeg-windows-x64.zip", "$ffmpegFilePath.zip")
} finally {
    # 清理资源，关闭WebClient
    $http.Dispose()
}

# 使用Get-FileHash函数验证下载文件的SHA256哈希值
try {
    Import-Module Microsoft.PowerShell.Utility -Function Get-FileHash
    $hashResult = Get-FileHash "$ffmpegFilePath.zip" -Algorithm SHA256
    
    # 如果计算出的哈希值与预期不符，则抛出异常
    if ($hashResult.Hash -ne "48130a80aebffb61d06913350c3ad3187efd85096f898045fd65001bf89d7d7f") {
        throw "未能通过校验FFmpeg压缩包的哈希值。"
    }

    # 解压FFmpeg可执行文件
    Add-Type -Assembly System.IO.Compression.FileSystem
    $zip = [IO.Compression.ZipFile]::OpenRead("$ffmpegFilePath.zip")
    try {
        [IO.Compression.ZipFileExtensions]::ExtractToFile($zip.GetEntry("ffmpeg.exe"), $ffmpegFilePath)
    } finally {
        # 关闭并清理ZipArchive对象
        $zip.Dispose()
    }
    
    Write-Host "已完成下载并解压FFmpeg。"
} finally {
    # 删除临时的FFmpeg压缩包文件
    Remove-Item "$ffmpegFilePath.zip" -Force
}