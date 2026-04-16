const express = require('express');
const router = express.Router();
const { exec } = require('child_process');
const path = require('path');
const fs = require('fs');

// --- 接口 1：初始化组件 (现在在 Server 目录) ---
router.get('/check-env', (req, res) => {
    // 路径：WebManager/routes -> .. -> .. -> Server
    const scriptPath = path.join(__dirname, '..', '..', 'Server', '初始化项目组件.bat');
    const scriptDir = path.dirname(scriptPath);

    if (!fs.existsSync(scriptPath)) {
        return res.status(404).send(`找不到环境脚本: ${scriptPath}`);
    }

    // 使用 /d 确保在 Server 目录下运行，安装包会下载到 Server 文件夹
    exec(`start /d "${scriptDir}" cmd.exe /k "初始化项目组件.bat"`, (error) => {
        if (error) return res.status(500).send('环境脚本启动失败');
        res.send('环境检测已在 Server 窗口中启动');
    });
});

// --- 接口 2：执行协议输出工具 (在 Public/Protobuf 目录) ---
router.get('/gen-proto', (req, res) => {
    // 路径：WebManager/routes -> .. -> .. -> Public -> Protobuf
    const exePath = path.join(__dirname, '..', '..', 'Public', 'Protobuf', '输出协议.exe');
    const exeDir = path.dirname(exePath);

    if (!fs.existsSync(exePath)) {
        return res.status(404).send(`找不到协议工具: ${exePath}`);
    }

    // 使用 /d 确保 exe 在自己的目录下运行，能找到周边的配置
    exec(`start /d "${exeDir}" 输出协议.exe`, (error) => {
        if (error) {
            console.error(`执行错误: ${error}`);
            return res.status(500).send('协议工具启动失败');
        }
        res.send('协议转换工具已启动');
    });
});

// --- 接口：游戏配置表更新 ---
router.get('/update-tables', (req, res) => {
    // 路径：WebManager/routes -> .. -> .. -> Public -> Tables -> ExcelTool -> bin -> ExcelTool.exe
    const exePath = path.join(__dirname, '..', '..', 'Public', 'Tables', 'ExcelTool', 'bin', 'ExcelTool.exe');
    const exeDir = path.dirname(exePath);

    if (!fs.existsSync(exePath)) {
        return res.status(404).send(`找不到导表工具: ${exePath}`);
    }

    // 使用 start /d 确保工具在自己的 bin 目录下运行，以便它能正确加载周边的 DLL 或配置文件
    exec(`start /d "${exeDir}" ExcelTool.exe`, (error) => {
        if (error) {
            console.error(`导表工具执行错误: ${error}`);
            return res.status(500).send('导表工具启动失败');
        }
        res.send('配置表更新工具已启动');
    });
});

// --- 接口：打包工程并推送到服务器 ---
router.get('/deploy-server', (req, res) => {
    // 路径：WebManager/routes -> .. -> .. -> Server -> 打包工程并推送到服务器.bat
    const scriptPath = path.join(__dirname, '..', '..', 'Server', '打包工程并推送到服务器.bat');
    const scriptDir = path.dirname(scriptPath);

    if (!fs.existsSync(scriptPath)) {
        return res.status(404).send(`找不到部署脚本: ${scriptPath}`);
    }

    // 打包推送通常需要较长时间，建议保留窗口 (/k) 以便观察进度或报错
    exec(`start /d "${scriptDir}" cmd.exe /k "打包工程并推送到服务器.bat"`, (error) => {
        if (error) {
            console.error(`部署脚本启动失败: ${error}`);
            return res.status(500).send('部署脚本启动失败');
        }
        res.send('自动化部署流程已启动，请在弹出的 CMD 窗口中确认进度。');
    });
});

// --- 接口：推送热更资源到服务器 ---
router.get('/push-hotfix', (req, res) => {
    // 路径：WebManager/routes -> .. -> .. -> Server -> 推送热更资源到服务器.bat
    const scriptPath = path.join(__dirname, '..', '..', 'Server', '推送热更资源到服务器.bat');
    const scriptDir = path.dirname(scriptPath);

    if (!fs.existsSync(scriptPath)) {
        return res.status(404).send(`找不到热更脚本: ${scriptPath}`);
    }

    // 执行脚本
    exec(`start /d "${scriptDir}" cmd.exe /k "推送热更资源到服务器.bat"`, (error) => {
        if (error) {
            console.error(`热更脚本启动失败: ${error}`);
            return res.status(500).send('热更脚本启动失败');
        }
        res.send('热更资源推送流程已启动');
    });
});

// --- 接口：运行游戏服务器 ---
router.get('/run-server', (req, res) => {
    // 路径：WebManager/routes -> .. -> .. -> Server -> Publish -> win-x64 -> TCPServer.exe
    const exePath = path.join(__dirname, '..', '..', 'Server', 'Publish', 'win-x64', 'TCPServer.exe');
    const exeDir = path.dirname(exePath);

    if (!fs.existsSync(exePath)) {
        return res.status(404).send(`找不到服务器程序: ${exePath}`);
    }

    // 启动服务器，不带 /b 以便弹出独立的黑窗口观察日志
    // 建议使用 start "GameServer" 给窗口起个名字，方便在任务栏识别
    exec(`start "GameServer" /d "${exeDir}" TCPServer.exe`, (error) => {
        if (error) {
            console.error(`服务器启动错误: ${error}`);
            return res.status(500).send('服务器启动失败');
        }
        res.send('游戏服务器正在启动...');
    });
});

// --- 接口：构建 Android APK (备用自动化) ---
router.get('/build-android-apk', (req, res) => {
    // 路径：WebManager/routes -> .. -> .. -> Utils -> Build_Android.bat
    const scriptPath = path.join(__dirname, '..', '..', 'Utils', 'Build_Android.bat');
    const scriptDir = path.dirname(scriptPath);

    if (!fs.existsSync(scriptPath)) {
        return res.status(404).send(`找不到脚本: ${scriptPath}`);
    }

    // 依然保留这个接口，作为你“全栈控制面板”的一个选装组件
    exec(`start /d "${scriptDir}" cmd.exe /k "Build_Android.bat"`, (error) => {
        if (error) return res.status(500).send('脚本启动失败');
        res.send('Unity 自动化构建任务已启动 (处于 Utils 备用目录)');
    });
});

// --- 预留接口 3：清理缓存 ---
router.get('/clear-cache', (req, res) => {
    res.send('【待开发】清理逻辑预留中...');
});

module.exports = router;