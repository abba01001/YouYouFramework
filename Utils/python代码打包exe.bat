:      <-- 在最前面加一行这个，后面随便空几个格，用来“吸收”乱码
@echo off
setlocal enabledelayedexpansion
:: 强制设置代码页为 UTF-8 (65001)
chcp 65001 >nul
pushd "%~dp0"

echo ========================================
echo       PyInstaller 快速打包工具
echo ========================================

:: 1. 调用 PowerShell 弹出文件选择框
echo [系统] 正在打开文件选择窗口...
set "ps_cmd=Add-Type -AssemblyName System.Windows.Forms; $f = New-Object System.Windows.Forms.OpenFileDialog; $f.Filter = 'Python 脚本 (*.py)|*.py'; $f.TopMost = $true; if($f.ShowDialog() -eq 'OK'){ $f.FileName }"

set "PYTHON_FILE="
for /f "usebackq delims=" %%I in (`powershell -NoProfile -Command "%ps_cmd%"`) do set "PYTHON_FILE=%%I"

:: 2. 判断用户是否选择了文件
if "%PYTHON_FILE%"=="" (
    echo [取消] 未选择任何文件，脚本即将退出。
    timeout /t 3 >nul
    popd
    exit /b
)

:: 3. 解析文件名信息 (支持带空格的路径)
for %%F in ("%PYTHON_FILE%") do (
    set "RAW_NAME=%%~nxF"
    set "FILE_NAME=%%~nF"
)

echo [选择] 已锁定目标: !RAW_NAME!
echo [进程] 正在启动 PyInstaller 打包流程...
echo ----------------------------------------

:: 4. 执行打包 ( -F 参数生成单个独立 EXE )
pyinstaller -F "%PYTHON_FILE%"

:: 5. 检查打包结果
if %errorlevel% neq 0 (
    echo.
    echo [错误] 打包过程出错，请查看上方输出信息。
    pause
    popd
    exit /b %errorlevel%
)

:: 6. 后续整理与临时文件清理
echo.
echo [整理] 正在提取 EXE 文件并清理缓存...

if exist "dist\!FILE_NAME!.exe" (
    :: 强制移动 EXE 到当前目录下
    move /y "dist\!FILE_NAME!.exe" . >nul
    echo [完成] 成功生成: !FILE_NAME!.exe
)

:: 删除 PyInstaller 产生的中间文件夹和配置
rd /s /Q build 2>nul
rd /s /Q dist 2>nul
del /q "!FILE_NAME!.spec" 2>nul

echo ----------------------------------------
echo 打包任务圆满完成！
echo ----------------------------------------
pause
popd