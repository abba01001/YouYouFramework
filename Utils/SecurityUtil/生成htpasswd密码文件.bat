@echo off
echo ========================
echo 生成 .htpasswd 文件
echo ========================

set HTPASSWD_PATH=%~dp0htpasswd.exe

if not exist "%HTPASSWD_PATH%" (
    echo 错误: htpasswd.exe 文件未找到，请确保 htpasswd.exe 和该脚本位于同一目录下。
    pause
    exit /b
)

set /p USERNAME=请输入用户名: 
set /p PASSWORD=请输入密码: 

"%HTPASSWD_PATH%" -cb "%~dp0.htpasswd" %USERNAME% %PASSWORD%

if exist "%~dp0.htpasswd" (
    echo .htpasswd 文件已生成，保存到 %~dp0.htpasswd
) else (
    echo ? 文件生成失败，请检查输入或路径。
)

pause
