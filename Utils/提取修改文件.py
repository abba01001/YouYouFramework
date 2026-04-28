import os
import subprocess
import shutil
from typing import List

# ===================== 配置项（可自行修改） =====================
TEMP_FOLDER = "./extracted_files"       # 临时文件夹
ZIP_NAME = "extracted_files"            # 压缩包名称
MANIFEST_FILE = "manifest.txt"          # 清单文件名
# ==============================================================

def clean_file_path(file_path: str) -> str:
    """统一清理文件路径：去除空格、引号、统一分隔符"""
    return file_path.strip().replace('"', "").replace("\\", "/")

def get_git_changed_files() -> tuple[List[str], List[str]]:
    """
    获取Git中已修改、已删除的文件
    返回：(修改文件列表, 删除文件列表)
    """
    try:
        # 获取Git状态（精简格式）
        result = subprocess.run(
            ["git", "status", "--porcelain"],
            capture_output=True,
            text=True,
            encoding="utf-8"
        )
    except Exception as e:
        print(f"❌ 获取Git状态失败：{str(e)}")
        return [], []

    modified_files = []
    deleted_files = []

    for line in result.stdout.splitlines():
        if not line.strip() or line.startswith("??"):
            continue  # 跳过空行、未跟踪文件

        status = line[:2].strip()
        file_path = clean_file_path(line[3:])

        # 分类文件状态
        if status in ("M", "A", "R"):  # 修改/新增/重命名
            modified_files.append(file_path)
        elif status == "D":  # 删除
            deleted_files.append(file_path)

    return modified_files, deleted_files

def copy_files_with_structure(files: List[str], target_root: str):
    """复制文件并保持目录结构（仅复制存在的文件）"""
    for file in files:
        if not os.path.isfile(file):
            print(f"⚠️  跳过不存在的文件：{file}")
            continue

        target_path = os.path.join(target_root, file)
        target_dir = os.path.dirname(target_path)

        # 创建目录
        os.makedirs(target_dir, exist_ok=True)
        # 复制文件
        shutil.copy2(file, target_path)
        print(f"✅ 复制文件：{file}")

def create_deleted_markers(files: List[str], target_root: str):
    """为删除的文件创建空标记文件（保留目录结构）"""
    for file in files:
        target_path = os.path.join(target_root, file)
        target_dir = os.path.dirname(target_path)

        os.makedirs(target_dir, exist_ok=True)
        # 创建空文件标记
        open(target_path, "w", encoding="utf-8").close()
        print(f"🗑️  标记删除文件：{file}")

def generate_manifest(modified: List[str], deleted: List[str], target_root: str):
    """
    生成文件清单：
    1. 写入 manifest.txt 到打包目录
    2. 控制台打印清单
    """
    manifest_path = os.path.join(target_root, MANIFEST_FILE)

    with open(manifest_path, "w", encoding="utf-8") as f:
        f.write("=" * 50 + "\n")
        f.write("📄 打包文件清单\n")
        f.write("=" * 50 + "\n\n")

        f.write(f"【修改/新增文件】({len(modified)}个)\n")
        for fpath in modified:
            f.write(f"- {fpath}\n")

        f.write(f"\n【已删除文件】({len(deleted)}个)\n")
        for fpath in deleted:
            f.write(f"- {fpath}\n")

    # 控制台打印清单
    print("\n" + "=" * 60)
    print("📋 最终文件清单")
    print("=" * 60)
    print(f"\n修改/新增文件 ({len(modified)}个):")
    for fpath in modified:
        print(f"  - {fpath}")

    print(f"\n已删除文件 ({len(deleted)}个):")
    for fpath in deleted:
        print(f"  - {fpath}")
    print("=" * 60 + "\n")

def main():
    # 1. 获取变更文件
    modified_files, deleted_files = get_git_changed_files()
    total = len(modified_files) + len(deleted_files)

    if total == 0:
        print("✅ 没有检测到任何修改/删除的文件")
        return

    print(f"🔍 检测到：{len(modified_files)}个修改文件，{len(deleted_files)}个删除文件\n")

    # 2. 创建临时目录
    os.makedirs(TEMP_FOLDER, exist_ok=True)

    # 3. 处理文件
    copy_files_with_structure(modified_files, TEMP_FOLDER)
    create_deleted_markers(deleted_files, TEMP_FOLDER)

    # 4. 生成清单文件（核心需求）
    generate_manifest(modified_files, deleted_files, TEMP_FOLDER)

    # 5. 打包压缩
    shutil.make_archive(ZIP_NAME, "zip", TEMP_FOLDER)
    print(f"📦 打包完成：{ZIP_NAME}.zip")

    # 6. 清理临时文件夹
    shutil.rmtree(TEMP_FOLDER)
    print(f"🧹 已清理临时文件夹：{TEMP_FOLDER}")

if __name__ == "__main__":
    main()
