@echo off
chcp 65001
pushd "%~dp0"  REM 切换到脚本所在目录，确保执行路径正确

rem 提示用户输入 Python 文件名
echo 请输入 Python 文件名（例如：输出协议.py）：
set /p PYTHON_FILE=

rem 输出用户输入的文件名，检查是否正确
echo 你输入的 Python 文件名是：%PYTHON_FILE%

rem 检查文件是否存在
if not exist "%PYTHON_FILE%" (
    echo 错误：指定的文件 "%PYTHON_FILE%" 不存在。
    pause
    popd
    exit /b
)

rem 获取文件名（去掉扩展名）
set FILE_NAME=%PYTHON_FILE:~0,-3%

rem 打包 Python 脚本
echo 正在打包 %PYTHON_FILE%...
pyinstaller -F "%PYTHON_FILE%"

rem 检查 pyinstaller 是否成功
if %errorlevel% neq 0 (
    echo 错误：打包失败，请检查 PyInstaller 输出信息。
    pause
    popd
    exit /b %errorlevel%
)

rem 检查 move 是否成功
echo 正在移动文件...
move "dist\%FILE_NAME%.exe" . >nul
if %errorlevel% neq 0 (
    echo 错误：Move命令失败，停止执行。
    pause
    popd
    exit /b %errorlevel%
)

rem 如果 move 成功，继续执行清理操作
echo 清理临时文件...
rd /s /Q build
rd /s /Q dist
del /s /Q "%FILE_NAME%.spec"

echo 打包完成。
pause
popd
