@echo off
setlocal enabledelayedexpansion
chcp 65001 >nul
cd /d "%~dp0"

:: 增加一个 ADB 检测，防止因为没装 ADB 直接闪退
where adb >nul 2>nul
if %errorlevel% neq 0 (
    echo [致命错误] 你的电脑没有配置 ADB 环境！
    echo 请先安装 Android SDK 并将 platform-tools 文件夹路径加入环境变量 Path。
    pause
    exit
)

:MAIN_MENU
cls
echo ========================================
echo          ADB APK 快速安装工具 (调试版)
echo ========================================

set "count=0"
:: 捕获 adb 报错
for /f "skip=1 tokens=1,2" %%a in ('adb devices 2^>nul') do (
    if "%%b"=="device" (
        set /a count+=1
        set "dev!count!=%%a"
        echo [!count!] %%a
    )
)

if %count% equ 0 (
    echo [提示] 未检测到设备。请确保手机已连接并开启 USB 调试。
    echo 按任意键重新扫描...
    pause >nul
    goto MAIN_MENU
)

set "serial="
if %count% equ 1 (
    set "serial=%dev1%"
    echo 已自动选择设备: !serial!
) else (
    set /p dev_idx=请输入设备编号: 
    if "!dev_idx!"=="" goto MAIN_MENU
    call set "serial=%%dev!dev_idx!%%"
)

if "!serial!"=="" (
    echo [错误] 编号选择无效。
    timeout /t 2 >nul
    goto MAIN_MENU
)

:SELECT_FILE
echo.
echo [提示] 正在调起文件选择窗口...
set "ps_cmd=Add-Type -AssemblyName System.Windows.Forms; $f = New-Object System.Windows.Forms.OpenFileDialog; $f.Filter = 'APK Files (*.apk)|*.apk'; $f.TopMost = $true; if($f.ShowDialog() -eq 'OK'){ $f.FileName }"

set "apkPath="
:: 捕获 PowerShell 可能的报错
for /f "usebackq delims=" %%I in (`powershell -NoProfile -Command "%ps_cmd%" 2^>nul`) do set "apkPath=%%I"

if "%apkPath%"=="" (
    echo [取消] 未选择文件，返回菜单。
    timeout /t 2 >nul
    goto MAIN_MENU
)

echo.
echo 正在安装: "%apkPath%"
adb -s !serial! install -r "%apkPath%"

if %errorlevel% equ 0 (
    echo.
    echo [OK] 安装成功！
) else (
    echo.
    echo [Fail] 安装失败，错误码: %errorlevel%
)

echo.
echo 按任意键返回主菜单...
pause >nul
goto MAIN_MENU