@echo off
chcp 65001
setlocal

REM 设置所需的 .NET SDK 版本和下载链接
set DOTNET_VERSION=6.0.427
set INSTALLER_URL=https://download.visualstudio.microsoft.com/download/pr/d28a1003-167b-4223-b7fe-5e4c35d462af/a34a42006700d482816b24224ff3425c/dotnet-sdk-6.0.427-win-x64.exe

REM 检查是否安装了所需版本的 .NET SDK
set "INSTALLED_SDK="
for /f "tokens=1,2" %%a in ('dotnet --list-sdks') do (
    if "%%a"=="%DOTNET_VERSION%" (
        set INSTALLED_SDK=1
    )
)

IF NOT DEFINED INSTALLED_SDK (
    echo .NET SDK %DOTNET_VERSION% 未安装，正在下载...
    REM 使用 curl 进行下载，如果没有 curl，可以使用 PowerShell 的 Invoke-WebRequest
    curl -L -o dotnet-installer.exe %INSTALLER_URL%
    
    REM 检查下载是否成功
    IF EXIST dotnet-installer.exe (
        echo 下载完成，正在安装 .NET SDK %DOTNET_VERSION%...
        start /wait dotnet-installer.exe
        echo 安装完成。

        REM 删除安装包
        del dotnet-installer.exe
        echo 已删除安装包。
    ) ELSE (
        echo 下载失败，请检查网络连接或链接是否有效。
    )
) ELSE (
    echo .NET SDK %DOTNET_VERSION% 已安装。
)

pause
