@echo off
chcp 65001

echo ===========================================================
echo "  全量构建安装包 (Clear Build)"
echo "     - 包含：重新生成底层依赖 + 编译热更资源 + 导出完整包"
echo "     - 适用：新加了插件代码、更新了热更程序集结构时使用"
echo ===========================================================
echo.

:: 1. 配置路径
SET UNITY_PATH=D:\Program\UnityEditor\6000.3.12f1\6000.3.12f1\Editor\Unity.exe
SET PROJ_PATH=%~dp0..\..\Client
SET LOG_DIR=%~dp0..\..\Builds
if not exist "%LOG_DIR%" mkdir "%LOG_DIR%"
SET LOG_PATH=%LOG_DIR%\build_live.txt

:: 2. 清理旧日志文件
if exist "%LOG_PATH%" del "%LOG_PATH%"

:: 3. 在后台启动日志监控 (直接桥接到 Jenkins 标准输出流)
:: 使用 powershell 直接读取文件并写入 stdout，无需额外窗口
start /b powershell -Command "Get-Content '%LOG_PATH%' -Wait -Encoding UTF8"

:: 4. 执行 Unity 打包 (必须保留 logFile 以便监控)
"%UNITY_PATH%" -batchmode -quit ^
    -projectPath "%PROJ_PATH%" ^
    -executeMethod JenkinsBuildScript.PerformBuild ^
    -logFile "%LOG_PATH%" ^
    -buildTarget Android ^
    -hybridCLR true ^
    -buildType Release ^
    -fullBuild true

:: 5. 构建完成后，清理监控任务
taskkill /f /im powershell.exe /fi "WINDOWTITLE eq Administrator: *" 2>nul