@echo off
chcp 65001 > nul

:: 这里保留你原有的 Unity 路径（你指定为绝对路径）
SET UNITY_PATH="D:\Program\UnityEditor\6000.3.12f1\6000.3.12f1\Editor\Unity.exe"

:: 因为脚本移动到了 Utils 目录，所以 Client 路径需要返回上一级目录查找
SET PROJ_PATH=%~dp0..\Client
SET LOG_PATH=%~dp0..\unity_local_build_log.txt

echo ===========================================================
echo "                [本地打包脚本] 请选择构建模式"
echo ===========================================================
echo " [1] 全量构建安装包 (Clear Build)"
echo "     - 包含：重新生成底层依赖 + 编译热更资源 + 导出完整包"
echo "     - 适用：新加了插件代码、更新了热更程序集结构时使用"
echo.
echo " [2] 快速构建安装包 (Incremental Build)"
echo "     - 包含：跳过底层生成 + 仅编译最新热更资源 + 导出完整包"
echo "     - 适用：日常美术/程序修改纯热更业务逻辑时使用(速度快)"
echo ===========================================================
echo.

:CHOOSE_MODE
set /p BUILD_CHOICE="请输入选项序号 [1 或 2]: "
if "%BUILD_CHOICE%"=="1" ( set FULL_BUILD_ARG=true & goto START_BUILD )
if "%BUILD_CHOICE%"=="2" ( set FULL_BUILD_ARG=false & goto START_BUILD )
echo ❌ 输入错误！
goto CHOOSE_MODE

:START_BUILD
echo.
echo 🚀 正在启动 Unity 编译管线...
echo 锁定项目路径: "%PROJ_PATH%"

:: 1. 清理旧日志
if exist "%LOG_PATH%" del /f /q "%LOG_PATH%"

echo -----------------------------------------------------------
echo 📦 打包中，请稍候 (日志已同步保存)...
echo -----------------------------------------------------------

:: 记录开始时间
set t1=%time%

:: 2. 使用相对路径执行 Unity
"%UNITY_PATH%" -batchmode -quit ^
    -projectPath "%PROJ_PATH%" ^
    -executeMethod JenkinsBuildScript.PerformBuild ^
    -logFile "%LOG_PATH%" ^
    -buildTarget Android ^
    -hybridCLR true ^
    -buildType Release ^
    -fullBuild %FULL_BUILD_ARG%

:: 记录结束时间并计算
set t2=%time%
for /f "tokens=1-4 delims=:.," %%a in ("%t1%") do set /a "start=(((%%a*60)+%%b)*60+%%c)*100+%%d"
for /f "tokens=1-4 delims=:.," %%a in ("%t2%") do set /a "end=(((%%a*60)+%%b)*60+%%c)*100+%%d"
set /a "diff=(end-start)/100, min=diff/60, sec=diff%%60"

echo.

echo -----------------------------------------------------------
echo [打包结束] 打包用时: %min%分%sec%秒，请检查日志: %LOG_PATH%
if %ERRORLEVEL% NEQ 0 echo ❌ 打包进程返回了错误码: %ERRORLEVEL%
pause