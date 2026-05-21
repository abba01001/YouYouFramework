@echo off
chcp 65001
setlocal

cd /d %~dp0

:: --- 弹出选择框 ---
echo 正在等待选择 .csproj 文件...
set "ps_cmd=Add-Type -AssemblyName System.Windows.Forms; $f = New-Object System.Windows.Forms.OpenFileDialog; $f.Filter = 'C# Project (*.csproj)|*.csproj'; $f.TopMost = $true; if($f.ShowDialog() -eq 'OK'){ $f.FileName }"

for /f "usebackq delims=" %%I in (`powershell -NoProfile -Command "%ps_cmd%"`) do set "PROJECT_PATH=%%I"

if "%PROJECT_PATH%"=="" (
    echo.
    echo [提示] 你取消了文件选择，脚本即将退出。
    timeout /t 3 >nul
    exit /b
)

echo =========================
echo 已选择项目: %PROJECT_PATH%
echo =========================
echo 选择发布平台
echo 1. Windows
echo 2. Linux
echo 3. 全部
echo.

set /p choice=请选择：
set /p upload=是否上传(y/n):

REM 默认脚本
set SCRIPT=
if "%choice%"=="1" set SCRIPT=%~dp0WinSCP\upload_win.txt
if "%choice%"=="2" set SCRIPT=%~dp0WinSCP\upload_linux.txt
if "%choice%"=="3" set SCRIPT=%~dp0WinSCP\upload_all.txt

echo.
echo 使用脚本：%SCRIPT%

REM 发布
if "%choice%"=="1" call :publish win-x64
if "%choice%"=="2" call :publish linux-x64
if "%choice%"=="3" (
    call :publish win-x64
    call :publish linux-x64
)

REM 上传
if /I "%upload%"=="y" (
    echo 开始上传...
    WinSCP\winscp.com /script="%SCRIPT%"
)

pause
exit /b

:publish
set RUNTIME=%1

:: --- 核心修改：提取项目文件所在的目录 ---
for %%i in ("%PROJECT_PATH%") do set "PROJ_DIR=%%~dpi"
:: 将输出路径设为项目目录下的 Publish 文件夹
set "OUT=%PROJ_DIR%Publish\%RUNTIME%"

if exist "%OUT%" rmdir /s /q "%OUT%"

dotnet publish "%PROJECT_PATH%" ^
 -c Release ^
 -r %RUNTIME% ^
 --self-contained true ^
 -o "%OUT%"

echo 发布完成：!OUT!
exit /b