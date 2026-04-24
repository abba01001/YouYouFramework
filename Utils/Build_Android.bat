@echo off
setlocal
chcp 65001

:: 1. 确认 Unity 路径无误
set UNITY_EXE="D:\Program\UnityEditor\6000.3.12f1\6000.3.12f1\Editor\Unity.exe"

:: 2. 【关键修改】指向 Client 文件夹的绝对路径
:: 因为脚本在 Utils，而工程在 Client，它们是平级关系
cd /d "%~dp0..\Client"
set PROJECT_DIR=%cd%

echo ==========================================
echo    Unity 自动化打包 (备用工具)
echo ==========================================
echo 工程路径: %PROJECT_DIR%
echo 脚本位置: %~dp0

:: 检查目录是否存在，防止路径配错
if not exist "%PROJECT_DIR%\Assets" (
    echo [错误] 找不到 Unity 工程目录，请检查脚本中的路径配置！
    echo 当前指向: %PROJECT_DIR%
    pause
    exit /b 1
)

:: 3. 执行打包 (采用你测试成功的“带界面”模式)
echo 正在启动 Unity，请稍后...
start /min /wait "" %UNITY_EXE% -quit -projectPath "%PROJECT_DIR%" -executeMethod AssetBundleEditor.BuildAndroidLaunchScene -logFile "%~dp0unity_build_log.txt"

if %errorlevel% neq 0 (
    echo.
    echo [失败] 构建终止，错误码: %errorlevel%
    pause
    exit /b %errorlevel%
)

echo.
echo [成功] APK 构建完成！
pause