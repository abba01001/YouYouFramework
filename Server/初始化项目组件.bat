@echo off
chcp 65001
setlocal enabledelayedexpansion

:: 确保脚本在它所在的文件夹（Utils）执行，避免下载的文件跑偏
cd /d "%~dp0"

echo ==========================================
echo       开发环境自动化检测工具 (汇总版)
echo ==========================================
echo.

:: --- 步骤 1：执行静默检测 ---
echo 正在扫描系统环境，请稍候...

:: 初始化状态变量 (0=未安装, 1=已安装)
set HAS_DOTNET=0
set HAS_REDIS=0
set HAS_MYSQL=0
set "STR_DOTNET=未安装"
set "STR_REDIS=未安装"
set "STR_MYSQL=未安装"

:: 检测 .NET SDK 6.0
set "DOTNET_TARGET=6.0"
where dotnet >nul 2>nul
if !ERRORLEVEL! EQU 0 (
    for /f "tokens=1" %%a in ('dotnet --list-sdks 2^>nul') do (
        echo %%a | findstr /b "%DOTNET_TARGET%." >nul
        if !ERRORLEVEL! EQU 0 (
            set HAS_DOTNET=1
            set "STR_DOTNET=已安装 (%%a)"
        )
    )
)

:: --- 检测 Redis ---
set "PATH_REDIS="
where redis-server >nul 2>nul
if !ERRORLEVEL! EQU 0 (
    set HAS_REDIS=1
    for /f "delims=" %%i in ('where redis-server') do (
        if not defined PATH_REDIS set "PATH_REDIS=%%i"
    )
    set "STR_REDIS=已安装 (!PATH_REDIS!)"
) else (
    sc query redis >nul 2>nul
    if !ERRORLEVEL! EQU 0 (
        set HAS_REDIS=1
        set "STR_REDIS=已安装 (仅服务，未配环境变量)"
    ) else (
        set HAS_REDIS=0
        set "STR_REDIS=未安装"
    )
)

:: --- 检测 MySQL ---
set "PATH_MYSQL="
where mysql >nul 2>nul
if !ERRORLEVEL! EQU 0 (
    set HAS_MYSQL=1
    for /f "delims=" %%i in ('where mysql') do (
        if not defined PATH_MYSQL set "PATH_MYSQL=%%i"
    )
    set "STR_MYSQL=已安装 (!PATH_MYSQL!)"
) else (
    sc query mysql >nul 2>nul
    if !ERRORLEVEL! EQU 0 (
        set HAS_MYSQL=1
        set "STR_MYSQL=已安装 (仅服务，未配环境变量)"
    ) else (
        :: 这里的逻辑修复：原代码中检查 Redis 目录来判断 MySQL 状态可能是不小心写错了
        set HAS_MYSQL=0
        set "STR_MYSQL=未安装"
    )
)

:: --- 步骤 2：打印检测报告 ---
echo.
echo ==========================================
echo             当前环境检测报告
echo ==========================================
echo  1. .NET SDK 6.0 :  %STR_DOTNET%
echo  2. Redis        :  %STR_REDIS%
echo  3. MySQL        :  %STR_MYSQL%
echo ==========================================
echo.

:: --- 步骤 3：根据报告询问是否下载 ---

:: 处理 .NET
if %HAS_DOTNET%==0 (
    set /p choice="[?] 检测到 .NET SDK 缺失，是否下载安装? (Y/N): "
    if /i "!choice!"=="Y" (
        echo 正在下载 .NET SDK...
        set "DN_URL=https://download.visualstudio.microsoft.com/download/pr/d28a1003-167b-4223-b7fe-5e4c35d462af/a34a42006700d482816b24224ff3425c/dotnet-sdk-6.0.427-win-x64.exe"
        curl -L -o "dotnet-install.exe" "!DN_URL!"
        if exist "dotnet-install.exe" (
            start /wait "" "dotnet-install.exe"
            del "dotnet-install.exe"
            echo [OK] .NET 安装任务执行完毕。
        )
    )
)

:: 处理 Redis
if %HAS_REDIS%==0 (
    set /p choice="[?] 检测到 Redis 缺失，是否下载安装? (Y/N): "
    if /i "!choice!"=="Y" (
        echo 正在下载 Redis...
        set "RD_URL=https://github.com/tporadowski/redis/releases/download/v5.0.14.1/Redis-x64-5.0.14.1.msi"
        curl -L -o "redis-install.msi" "!RD_URL!"
        if exist "redis-install.msi" (
            msiexec /i "redis-install.msi" /passive
            echo [OK] Redis 安装程序已启动。
            del "redis-install.msi"
        )
    )
)

:: 处理 MySQL
if %HAS_MYSQL%==0 (
    set /p choice="[?] 检测到 MySQL 缺失，是否下载安装包? (Y/N): "
    if /i "!choice!"=="Y" (
        echo 正在下载 MySQL Installer...
        set "MS_URL=https://dev.mysql.com/get/Downloads/MySQLInstaller/mysql-installer-community-8.0.35.0.msi"
        curl -L -o "mysql-install.msi" "!MS_URL!"
        if exist "mysql-install.msi" (
            echo [OK] 下载完成，请手动运行 mysql-install.msi 进行配置。
            start "" "mysql-install.msi"
        )
    )
)

echo.
echo 脚本执行完毕。
pause
exit /b