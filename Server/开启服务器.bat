
REM 设置可执行文件路径
set EXECUTABLE=C:\Server\TCPServer.exe

REM 检查可执行文件是否存在
IF NOT EXIST "%EXECUTABLE%" (
    echo 错误: 找不到可执行文件 "%EXECUTABLE%"。
    pause
    exit /b 1
)

REM 尝试执行可执行文件并保持窗口打开
echo 正在启动 %EXECUTABLE%...
"%EXECUTABLE%"

REM 检查执行状态
IF ERRORLEVEL 1 (
    echo 错误: 启动失败，退出代码: %ERRORLEVEL%。
) ELSE (
    echo 启动成功。
)

pause  REM 让窗口保持打开状态
