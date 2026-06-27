@echo off
chcp 65001 >nul

echo ===========================================================
echo "  正在通过 Jenkins 导出 App."
echo ===========================================================

:: 1. 强制清理环境变量中的双引号，防止路径被双重引用
set "U_PATH=%UnityPath:"=%"
set "P_ROOT=%ProjectRoot:"=%"
set "U_PLATFORM=%Platform%"

:: 2. 检查关键路径是否有效
if not exist "%U_PATH%" (
    echo [Error] UnityPath 不存在: "%U_PATH%"
    exit /b 1
)

:: --- 新增：Unity 实例检测逻辑 ---
set "LOCK_FILE=%P_ROOT%\Library\EditorInstance.json"
if exist "%LOCK_FILE%" (
    echo [Warning] 检测到 Library/EditorInstance.json，可能项目已被打开...
    :: 简单检测：如果文件存在且内容不为空，说明可能有实例
    for %%i in ("%LOCK_FILE%") do if %%~zi GTR 0 (
        echo [Error] 发现正在运行的 Unity 实例，请先关闭本地 Unity 后再打包！
        exit /b 1
    )
)
:: ------------------------------

:: 3. 动态拼接日志路径 (强制使用反斜杠)
set "LOG_DIR=%OutputDir%\Logs"
if not exist "%LOG_DIR%" mkdir "%LOG_DIR%"
set "LOG_FILE=%LOG_DIR%\UnityBuildLog.log"
if exist "%LOG_FILE%" del "%LOG_FILE%"
type nul > "%LOG_FILE%"

:: 4. 启动日志监控
start /b powershell -Command "Get-Content '%LOG_FILE%' -Wait -Encoding UTF8"

:: 5. 执行 Unity 打包 (显式将变量包在引号内)
echo [Info] 正在执行: "%U_PATH%" -quit -batchmode -projectPath "%P_ROOT%" -executeMethod JenkinsBuilder.BuildApp -buildTarget %U_PLATFORM% -logFile "%LOG_FILE%" -refresh
"%U_PATH%" -quit -nographics -batchmode -refresh -projectPath "%P_ROOT%" -executeMethod JenkinsBuilder.BuildApp -buildTarget %U_PLATFORM% -logFile "%LOG_FILE%"

:: 6. 捕获退出码
set "EXIT_CODE=%ERRORLEVEL%"

:: 7. 清理残留
taskkill /f /im powershell.exe /fi "COMMANDLINE like *Get-Content*" 2>nul
taskkill /f /im adb.exe 2>nul
taskkill /f /im "UnityPackageManager.exe" 2>nul

:: 8. 精准定位本次构建的产物
:: 根据你的路径：D:\Study\Builds\Android\Release\1.0.0
set "TARGET_PATH=%OutputDir%\%Platform%\%BuildType%\%Version%"

echo [Info] 正在定位产物目录: "%TARGET_PATH%"

:: 检查目录是否存在
if not exist "%TARGET_PATH%" (
    echo [Error] 无法找到产物目录: "%TARGET_PATH%"
    exit /b 1
)

echo [Info] 开始搬运 APK...

:: 只搬运 APK，如果找不到则报错，方便你定位打包问题
if exist "%TARGET_PATH%\*.apk" (
    copy /y "%TARGET_PATH%\*.apk" "%WORKSPACE%\"
    echo [Info] APK 搬运成功。
) else (
    echo [Error] 未在目录 "%TARGET_PATH%" 下找到 APK 文件！
    exit /b 1
)

echo [Info] 搬运完成。

:: 8. 退出并回传状态给 Jenkins
exit /b %EXIT_CODE%