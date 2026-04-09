@echo off
:: 设置脚本所在目录为当前工作目录
cd /d %~dp0

:: 输出当前路径
echo Current directory: %cd%

chcp 65001
setlocal

REM 设置项目文件路径
set PROJECT_FILE=TCPServer\TCPServer.csproj

REM 设置发布目标
set TARGET_FRAMEWORK=net5.0
set RUNTIME_IDENTIFIER=win-x64

REM 执行发布
@REM dotnet publish %PROJECT_FILE% -c Release -f %TARGET_FRAMEWORK% -r %RUNTIME_IDENTIFIER% -o .\Server
@REM dotnet publish %PROJECT_FILE% -c Release -f net5.0 -r win-x64 -o .\Server /p:PublishSingleFile=true /p:SelfContained=true
dotnet publish %PROJECT_FILE% -c Release -f %TARGET_FRAMEWORK% -r %RUNTIME_IDENTIFIER% -o .\Server /p:PublishSingleFile=true /p:IncludeAllContentForSelfExtract=true /p:PublishTrimmed=true
IF %ERRORLEVEL% NEQ 0 (
    echo 发布失败，请检查项目设置或错误信息。
) ELSE (
    echo 发布成功，输出文件在当前目录的 Server 文件夹中。
@REM    "%cd%\WinSCP\winscp.com" /script="%cd%\upload_script.txt"
)



pause
发布前判断下有没有打包这个文件夹，如果有要先删除