import os
import subprocess
import webbrowser
import time
import sys
import socket

def is_port_in_use(port):
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.settimeout(0.2)
        return s.connect_ex(('localhost', port)) == 0

def kill_node_process():
    """强制并同步杀死旧进程，确保端口释放"""
    try:
        # 移除 Popen，改回 run，确保杀完再走下一步
        subprocess.run("taskkill /f /im node.exe /t >nul 2>&1", shell=True)
        time.sleep(0.5) # 给操作系统半秒钟回收端口
    except:
        pass

def main():
    is_frozen = getattr(sys, 'frozen', False)
    exe_path = sys.executable if is_frozen else __file__
    app_title = os.path.splitext(os.path.basename(exe_path))[0]
    base_path = os.path.dirname(sys.executable) if is_frozen else os.path.dirname(os.path.abspath(__file__))
    
    target_dir = os.path.join(base_path, "WebManager")
    if not os.path.exists(target_dir):
        print(f"致命错误: 找不到目录 {target_dir}")
        input("按任意键退出...")
        return

    os.chdir(target_dir)

    # 1. 检查依赖
    if not os.path.exists("node_modules"):
        print("首次运行或组件缺失，正在安装依赖...")
        subprocess.run("npm install --registry=https://registry.npmmirror.com", shell=True)

    # 2. 清理环境
    print("正在清理残留服务...")
    kill_node_process()

    # 3. 启动服务 (暂时移除 CREATE_NO_WINDOW 以便查错)
    # 如果启动不了，你会直接在黑窗口看到 Node 的报错信息
    print(f"正在启动 {app_title}...")
    env = os.environ.copy()
    env["APP_TITLE"] = app_title
    
    # 尝试启动。如果这里报 'node' 不是内部命令，说明你没装 Node
    node_proc = subprocess.Popen("node server.js", shell=True, env=env)

    # 4. 轮询检测
    success = False
    for i in range(30): # 最多等 6 秒
        if is_port_in_use(3000):
            print("\n[OK] 服务已启动，正在弹出浏览器...")
            webbrowser.open("http://localhost:3000")
            success = True
            break
        
        # 检查子进程是否已经崩溃退出
        if node_proc.poll() is not None:
            print("\n[错误] Node 进程意外退出！请检查 server.js 代码或 node_modules 是否完整。")
            break
            
        print(".", end="", flush=True)
        time.sleep(0.2)

    if not success:
        print("\n[失败] 无法连接到管理后台。")
        print("请尝试：1. 手动运行 'node server.js' 查看报错")
        print("      2. 检查 3000 端口是否被其他软件强制占用")

    try:
        while True: time.sleep(1)
    except KeyboardInterrupt:
        kill_node_process()
        sys.exit(0)

if __name__ == "__main__":
    main()