@echo off
chcp 65001 > nul
setlocal enabledelayedexpansion

REM 设置 protoc 路径和相关文件夹路径
set "BIN_DIR=bin"
set "PROTO_ROOT=local"
set "OUTPUT_DIR=publish"
set "CLIENT_DIR=..\..\Client\Assets\Game\Download\Protocol"

REM 计算目标文件夹的绝对路径
for %%i in ("%CLIENT_DIR%") do set "ABS_TARGET_DIR=%%~fi"

REM 检查 local 文件夹是否存在 .proto 文件
set "PROTO_FILES="
set "PROTO_FOUND=false"

for /r "%PROTO_ROOT%" %%f in (*.proto) do (
    set "PROTO_FILES=!PROTO_FILES! %%f"
    set "PROTO_FOUND=true"
)

if "!PROTO_FOUND!"=="false" (
    echo 没有找到任何 .proto 文件，生成失败!
    pause
    exit /b
)

REM 删除 publish 文件夹下的所有文件或创建文件夹
if exist "%OUTPUT_DIR%" (
    echo 正在删除 %OUTPUT_DIR% 文件夹下的所有文件...
    del /q "%OUTPUT_DIR%\*"
) else (
    echo 正在创建 %OUTPUT_DIR% 文件夹...
    mkdir "%OUTPUT_DIR%"
)

REM 生成协议文件并记录生成信息
set "GENERATED_FILES="

echo 正在生成协议文件...
echo 处理协议文件:

for %%f in (!PROTO_FILES!) do (
    echo     处理文件: %%f...
    "%BIN_DIR%\protoc.exe" --proto_path="%CD%\%PROTO_ROOT%" --csharp_out="%OUTPUT_DIR%" "%%~f" || (
        echo 生成失败: %%~nxF
        exit /b
    )
    set "GENERATED_FILES=!GENERATED_FILES! %%~nxf"
)

REM 显示生成结果
echo 输出协议:
for %%f in (!GENERATED_FILES!) do (
    echo     %CD%\%OUTPUT_DIR%\%%~nxf
)

REM 删除并创建目标文件夹
if exist "%ABS_TARGET_DIR%" (
    echo 正在删除 %ABS_TARGET_DIR% 文件夹及其内容...
    rmdir /s /q "%ABS_TARGET_DIR%"
)

echo 正在创建目标文件夹: "%ABS_TARGET_DIR%"
mkdir "%ABS_TARGET_DIR%"

REM 复制文件到目标文件夹
echo 正在复制文件到目标文件夹...
copy "%OUTPUT_DIR%\*" "%ABS_TARGET_DIR%\" > nul
if errorlevel 1 (
    echo 复制文件失败!
) else (
    echo 文件复制成功!
    echo 目标文件夹: "%ABS_TARGET_DIR%"
)

echo 协议文件生成并复制成功
pause
