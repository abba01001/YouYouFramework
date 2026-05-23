@echo off
chcp 65001

echo ===========================================================
echo " 快速构建安装包 (Incremental Build)"
echo "     - 包含：跳过底层生成 + 仅编译最新热更资源 + 导出完整包"
echo "     - 适用：日常美术/程序修改纯热更业务逻辑时使用(速度快)"
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
    -fullBuild false

:: 5. 构建完成后，清理监控任务
taskkill /f /im powershell.exe /fi "WINDOWTITLE eq Administrator: *" 2>nul