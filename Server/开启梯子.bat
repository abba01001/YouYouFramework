@echo off
chcp 65001

REM 设置 V2Ray 可执行文件路径
set V2RAY_EXECUTABLE=C:\v2ray-windows-64\v2ray.exe

REM 检查可执行文件是否存在
IF NOT EXIST "%V2RAY_EXECUTABLE%" (
    echo 错误: 找不到可执行文件 "%V2RAY_EXECUTABLE%"。
    pause
    exit /b 1
)

REM 启动 V2Ray
echo 正在启动 V2Ray...
start "" "%V2RAY_EXECUTABLE%"

pause  REM 让窗口保持打开状态
