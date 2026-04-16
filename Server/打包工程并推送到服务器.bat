@echo off
chcp 65001
setlocal

cd /d %~dp0

echo =========================
echo 选择发布平台
echo =========================
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

REM 发布（示例）
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
set OUT=Publish\%RUNTIME%

if exist "%OUT%" rmdir /s /q "%OUT%"

dotnet publish TCPServer\TCPServer.csproj ^
 -c Release ^
 -r %RUNTIME% ^
 --self-contained true ^
 -o "%OUT%"

echo 发布完成：%RUNTIME%
exit /b