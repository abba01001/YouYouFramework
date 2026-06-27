import os
import shutil
import subprocess
import zipfile
import glob
import datetime
from typing import List

# 配置项
TEMP_FOLDER = "./update_pkg"
TEMP_UNZIP = "temp_patch_unzip"
ZIP_PREFIX = "patch"  # 补丁文件前缀
DELETE_MANIFEST = "delete_list.txt"
TARGET_DIR = "."
FILE_LIST = "file_list.txt"

def get_git_changes() -> tuple[List[str], List[str], List[str]]:
    # 辅助函数：运行 git 命令并获取行列表
    def run_git(cmd):
        result = subprocess.run(cmd, capture_output=True, encoding='utf-8', errors='replace')
        return result.stdout.splitlines()

    # 1. 获取 修改 (Modified)
    modified = run_git(["git", "diff", "--name-only", "--diff-filter=M", "HEAD"])
    
    # 2. 获取 新增 (Added/Untracked)
    added_staged = run_git(["git", "diff", "--name-only", "--diff-filter=A", "HEAD"])
    
    # --- 关键修改：过滤掉补丁文件 ---
    raw_untracked = run_git(["git", "ls-files", "--others", "--exclude-standard"])
    untracked = [f for f in raw_untracked if not (f.startswith(ZIP_PREFIX) and f.endswith(".zip"))]
    # ----------------------------
    
    renamed = run_git(["git", "diff", "--name-only", "--diff-filter=R", "HEAD"])
    added = added_staged + untracked + renamed
    
    # 3. 获取 删除 (Deleted)
    deleted = run_git(["git", "diff", "--name-only", "--diff-filter=D", "HEAD"])
    
    return modified, added, deleted

def pack():
    modified, added, deleted = get_git_changes()
    if not modified and not added and not deleted:
        print("✅ 无变更，无需打包。")
        return

    # 生成带时间戳的文件名
    timestamp = datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
    zip_filename = f"{ZIP_PREFIX}_{timestamp}"
    
    os.makedirs(TEMP_FOLDER, exist_ok=True)
    
    # 复制文件
    for f in (modified + added):
        # --- 新增过滤逻辑 ---
        if f.endswith(".zip") and f.startswith(ZIP_PREFIX):
            continue
        # ------------------
        if os.path.exists(f): # 确保文件存在
            target = os.path.join(TEMP_FOLDER, f)
            os.makedirs(os.path.dirname(target), exist_ok=True)
            if os.path.isfile(f):
                shutil.copy2(f, target)
            elif os.path.isdir(f):
                # 递归拷贝目录
                shutil.copytree(f, target, dirs_exist_ok=True)

    # 写入清单
    with open(os.path.join(TEMP_FOLDER, FILE_LIST), "w", encoding="utf-8") as f:
        f.write("=== 修改文件 ===\n" + "\n".join(modified) + "\n")
        f.write("\n=== 新增文件 ===\n" + "\n".join(added) + "\n")
        f.write("\n=== 删除文件 ===\n" + "\n".join(deleted) + "\n")
    
    shutil.make_archive(zip_filename, "zip", TEMP_FOLDER)
    shutil.rmtree(TEMP_FOLDER)
    print(f"📦 打包完成: {zip_filename}.zip")

def apply():
    # 自动查找最新的补丁文件
    patches = glob.glob(f"{ZIP_PREFIX}_*.zip")
    if not patches:
        print(f"❌ 错误：未找到任何 {ZIP_PREFIX}_*.zip 文件")
        return
    
    latest_patch = max(patches, key=os.path.getmtime)
    print(f"🔍 找到最新补丁: {latest_patch}")
    
    with zipfile.ZipFile(latest_patch, 'r') as zip_ref:
        zip_ref.extractall(TEMP_UNZIP)
    
    # 1. 执行删除操作
    list_path = os.path.join(TEMP_UNZIP, FILE_LIST)
    if os.path.exists(list_path):
        with open(list_path, "r", encoding="utf-8") as f:
            lines = f.readlines()
            if "=== 删除文件 ===\n" in lines:
                idx = lines.index("=== 删除文件 ===\n")
                for line in lines[idx+1:]:
                    path = line.strip()
                    if not path: continue
                    file_to_del = os.path.join(TARGET_DIR, path)
                    if os.path.exists(file_to_del):
                        if os.path.isdir(file_to_del):
                            shutil.rmtree(file_to_del)
                        else:
                            os.remove(file_to_del)
                        print(f"🗑️ 已删除: {path}")
        os.remove(list_path)

    # 2. 覆盖新文件
    for root, _, files in os.walk(TEMP_UNZIP):
        for file in files:
            src_path = os.path.join(root, file)
            rel_path = os.path.relpath(src_path, TEMP_UNZIP)
            dest_path = os.path.join(TARGET_DIR, rel_path)
            os.makedirs(os.path.dirname(dest_path), exist_ok=True)
            shutil.copy2(src_path, dest_path)
            print(f"✅ 更新: {rel_path}")

    shutil.rmtree(TEMP_UNZIP)
    print("🧹 更新完成，清理完毕。")

if __name__ == "__main__":
    subprocess.run(["git", "config", "--global", "core.quotepath", "false"], capture_output=True)
    print("=== Git 补丁更新工具 ===")
    print("1. 打包 (Pack)")
    print("2. 更新 (Apply)")
    choice = input("请选择 (1/2): ").strip()
    
    if choice == "1":
        pack()
    elif choice == "2":
        apply()
    else:
        print("无效选择")
