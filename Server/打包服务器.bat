@echo off
chcp 65001
setlocal

REM 设置项目文件路径
set PROJECT_FILE=TCPServer\TCPServer.csproj

REM 设置发布目标
set TARGET_FRAMEWORK=netcoreapp5.0
set RUNTIME_IDENTIFIER=win-x64

REM 执行发布
dotnet publish %PROJECT_FILE% -c Release -f %TARGET_FRAMEWORK% -r %RUNTIME_IDENTIFIER% -o .\Server

IF %ERRORLEVEL% NEQ 0 (
    echo 发布失败，请检查项目设置或错误信息。
) ELSE (
    echo 发布成功，输出文件在当前目录的 Server 文件夹中。
)

pause
发布前判断下有没有打包这个文件夹，如果有要先删除