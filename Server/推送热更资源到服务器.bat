@echo off
:: 设置脚本所在目录为当前工作目录
cd /d %~dp0

:: 输出当前路径
echo Current directory: %cd%

chcp 65001
setlocal

echo 发布成功，输出文件在当前目录的 Server 文件夹中。
   "%cd%\WinSCP\winscp.com" /script="%cd%\upload_assetbundle_script.txt"


pause
