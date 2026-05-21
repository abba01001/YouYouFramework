const express = require('express');
const router = express.Router();
const path = require('path');
const fs = require('fs');

const { exec, spawn } = require('child_process');
const { promisify } = require('util');
const execAsync = promisify(exec);

let currentLogProcess = null;

// 新增接口：获取当前所有在线设备列表
router.get('/list-devices', async (req, res) => {
    try {
        const { stdout } = await execAsync('adb devices');

        // 使用正则匹配：开头是设备名，中间是空白符，结尾是 device
        // 这能兼容所有平台和模拟器的输出差异
        const devices = [];
        const lines = stdout.split(/\r?\n/); // 兼容 Windows(\r\n) 和 Linux(\n)

        lines.forEach(line => {
            // 正则解析逻辑：抓取状态为 "device" 的行
            const match = line.match(/^([^\s]+)\s+device$/);
            if (match) {
                devices.push(match[1]); // match[1] 就是序列号（如 127.0.0.1:16384）
            }
        });

        console.log("--- 最终解析结果 ---");
        console.log(devices);
        console.log("--------------------");

        res.json(devices);
    } catch (err) {
        console.error("ADB 获取失败:", err);
        res.status(500).json([]);
    }
});

// 启动日志：支持通过参数传入指定的 serial
router.get('/start-logcat', async (req, res) => {
    const io = req.app.get('io');
    // 如果前端没传具体设备，我们尝试自动获取第一个
    let serial = req.query.serial;

    try {
        if (!serial) {
            const { stdout } = await execAsync('adb devices');
            const devices = stdout.split('\n')
                .filter(line => line.endsWith('\tdevice'))
                .map(line => line.split('\t')[0]);

            if (devices.length === 0) return res.status(404).send('未发现任何在线设备');
            serial = devices[0];
        }

        console.log(`[*] 正在监听设备: ${serial}`);

        if (currentLogProcess) {
            currentLogProcess.kill();
            currentLogProcess = null;
        }

        const logcat = spawn('adb', [
            '-s', serial,
            'logcat',
            '-v', 'time',
            '-s',
            'Unity:V',
            'AndroidRuntime:E',
            'ActivityManager:I',
            'DEBUG:I',           // 崩溃时的堆栈信息（Native Crash）
            'libc:W',            // C 库的警告/错误
            'CRASH:E',           // 部分系统的崩溃捕获标签
            'native:I',          // 一些原生插件会使用的通用标签
            '*:S'                // 依然保持静默模式，只看上面这些
        ]);

        currentLogProcess = logcat;

        logcat.stdout.on('data', (data) => {
            io.emit('adb-log', data.toString());
        });

        res.send(`已连接设备: ${serial}`);

    } catch (err) {
        res.status(500).send('启动失败: ' + err.message);
    }
});

// --- 接口：Python 脚本打包 ---
router.get('/py-build', (req, res) => {
    // 1. 设置你那个 .bat 文件的绝对路径
    // 这里的路径请根据你实际存放位置调整
    const scriptPath = path.join(__dirname, '..', '..', 'Utils', 'python代码打包exe.bat');
    const scriptDir = path.dirname(scriptPath);

    console.log(`[EXEC] 准备运行脚本: ${scriptPath}`);

    if (!fs.existsSync(scriptPath)) {
        console.error(`[ERROR] 找不到脚本: ${scriptPath}`);
        return res.status(404).send(`找不到脚本文件: ${path.basename(scriptPath)}`);
    }

    // 2. 直接启动！
    // 使用 start /d 切换到脚本目录执行，这样脚本里的 pushd %~dp0 才会生效
    // cmd /c 会在脚本跑完后尝试关闭这个外层中转窗口，但你脚本里如果有 pause 依然会停住
    const cmd = `start "PythonPackager" /d "${scriptDir}" cmd.exe /c "${path.basename(scriptPath)}"`;

    exec(cmd, (error) => {
        if (error) {
            console.error("[ERROR] 脚本启动失败:", error);
            return res.status(500).send('脚本启动失败');
        }
        // 这里直接返回响应，不再等待脚本结束，前端“正在请求”会立即消失
        res.send('打包脚本已在服务器控制台启动，请在黑窗口中操作');
    });
});

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

// --- 接口：ADB 安装 APK ---
router.get('/adb-install', (req, res) => {
    // 路径：WebManager/routes -> .. -> .. -> Utils -> install_apk.bat
    const scriptPath = path.join(__dirname, '..', '..', 'Utils', 'install_apk.bat');
    const scriptDir = path.dirname(scriptPath);

    if (!fs.existsSync(scriptPath)) {
        return res.status(404).send(`找不到 ADB 脚本: ${scriptPath}`);
    }

    // 使用 /d 确保在 Utils 目录下运行，cmd /c 执行完后如果没报错就关闭窗口
    exec(`start /d "${scriptDir}" cmd.exe /c "install_apk.bat"`, (error) => {
        if (error) {
            console.error(`ADB启动失败: ${error}`);
            return res.status(500).send('ADB 安装工具启动失败');
        }
        res.send('ADB 安装工具已启动，请查看服务器桌面弹窗');
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