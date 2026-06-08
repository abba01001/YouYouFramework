@echo off
setlocal

:: 强制使用 UTF-8 编码进行显示
chcp 65001 >nul

:: 定义源和目标
set "SOURCE_DIR=%~dp0jobs"
set "DEST_DIR=C:\Users\Administrator\.jenkins\jobs"

echo [Info] Preparing to sync jobs to: %DEST_DIR%

:: 执行拷贝
if exist "%SOURCE_DIR%" (
    if not exist "%DEST_DIR%" mkdir "%DEST_DIR%"
    xcopy "%SOURCE_DIR%\*" "%DEST_DIR%\" /E /I /Y
    echo [Info] Job sync completed.
) else (
    echo [Warning] jobs folder not found, skipping sync.
)

:: 启动 Jenkins
echo [Info] Starting Jenkins...
java -Dfile.encoding=UTF-8 -Dhudson.security.csrf.GlobalCrumbIssuerConfiguration.DISABLE_CSRF_PROTECTION=true -jar jenkins.war --enable-future-java
pause