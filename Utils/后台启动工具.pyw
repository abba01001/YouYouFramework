import os
import subprocess
import webbrowser
import time
import sys
import socket
import ctypes

# --- Windows 常量与配置 ---
CREATE_NO_WINDOW = 0x08000000
MB_OK = 0x00000000
MB_ICONERROR = 0x00000010
MB_ICONINFORMATION = 0x00000040

def show_message(title, text, style=MB_OK):
    """弹出 Windows 原生对话框"""
    return ctypes.windll.user32.MessageBoxW(0, text, title, style)

def is_port_in_use(port):
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.settimeout(0.2)
        return s.connect_ex(('localhost', port)) == 0

def kill_node_process():
    """静默杀死旧进程"""
    try:
        subprocess.run("taskkill /f /im node.exe /t >nul 2>&1", 
                       shell=True, 
                       creationflags=CREATE_NO_WINDOW)
        time.sleep(0.5)
    except:
        pass

def main():
    is_frozen = getattr(sys, 'frozen', False)
    exe_path = sys.executable if is_frozen else __file__
    app_title = os.path.splitext(os.path.basename(exe_path))[0]
    base_path = os.path.dirname(sys.executable) if is_frozen else os.path.dirname(os.path.abspath(__file__))
    
    target_dir = os.path.join(base_path, "WebManager")
    
    if not os.path.exists(target_dir):
        show_message("致命错误", f"找不到目录:\n{target_dir}", MB_ICONERROR)
        return

    os.chdir(target_dir)

    # 1. 检查依赖 (npm install 时通常比较慢，这里可以选择静默或显示)
    if not os.path.exists("node_modules"):
        # 如果你希望安装依赖时也不弹窗，加上 creationflags
        res = subprocess.run("npm install --registry=https://registry.npmmirror.com", 
                             shell=True, 
                             creationflags=CREATE_NO_WINDOW)
        if res.returncode != 0:
            show_message("错误", "npm 依赖安装失败，请检查网络或 Node 环境。", MB_ICONERROR)
            return

    # 2. 清理环境
    kill_node_process()

    # 3. 启动服务 (核心：完全隐藏 Node 窗口)
    env = os.environ.copy()
    env["APP_TITLE"] = app_title
    
    try:
        node_proc = subprocess.Popen("node server.js", 
                                     shell=True, 
                                     env=env, 
                                     creationflags=CREATE_NO_WINDOW)
    except Exception as e:
        show_message("启动失败", f"无法运行 Node:\n{str(e)}", MB_ICONERROR)
        return

    # 4. 轮询检测
    success = False
    for i in range(40): # 轮询时间稍微拉长，确保 Node 完全启动
        if is_port_in_use(3000):
            webbrowser.open("http://localhost:3000")
            success = True
            break
        
        # 实时检查 Node 是否崩溃
        if node_proc.poll() is not None:
            show_message("进程崩溃", "Node 服务意外退出！\n可能原因：端口冲突或 server.js 代码错误。", MB_ICONERROR)
            return
            
        time.sleep(0.2)

    if not success:
        show_message("启动超时", "服务已启动但无法访问 localhost:3000。\n请检查防火墙或端口占用情况。", MB_ICONERROR)
        return

    # 5. 后台守候 (直到手动通过任务管理器关闭或 KeyboardInterrupt)
    # 因为是 .pyw 运行，这里不会有任何感官影响
    try:
        while True:
            # 检查 Node 进程是否还活着
            if node_proc.poll() is not None:
                show_message("服务中断", "检测到后台 Node 服务已停止运行。", MB_ICONERROR)
                break
            time.sleep(2)
    except:
        kill_node_process()

if __name__ == "__main__":
    main()