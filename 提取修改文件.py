import os
import subprocess
import shutil

# 获取修改和删除的文件
def get_modified_and_deleted_files():
    result = subprocess.run(['git', 'status', '--porcelain'], capture_output=True, text=True)
    
    # 打印 Git 输出，帮助调试
    print("Git status output:")
    print(result.stdout)

    modified_files = []
    deleted_files = []

    for line in result.stdout.splitlines():
        # 忽略未跟踪的文件（以 ?? 开头）
        if line.startswith('??'):
            continue

        # 打印每一行，检查其内容和路径格式
        print(f"Processing line: '{line}'")

        # 判断 'M' 表示修改的文件，'D' 表示删除的文件
        status = line[0:2].strip()  # 获取前两个字符作为状态标识
        file_path = line[3:].strip()  # 文件路径从第四个字符开始

        # 检查文件状态并分类
        if status == 'M':
            modified_files.append(file_path)
        elif status == 'D':
            deleted_files.append(file_path)

    return modified_files, deleted_files

# 复制修改的文件并保持目录结构
def copy_modified_files(modified_files, target_dir):
    for file in modified_files:
        # 清理文件路径中的特殊字符或多余的引号
        file = file.strip().replace('"', '')  # 去掉路径中的引号

        target_file_path = os.path.join(target_dir, file)
        target_file_dir = os.path.dirname(target_file_path)

        # 打印目标文件路径，调试用
        print(f"Target directory: {target_file_dir}")
        print(f"Target file path: {target_file_path}")

        # 创建目标目录
        if not os.path.exists(target_file_dir):
            try:
                os.makedirs(target_file_dir)
            except OSError as e:
                print(f"Error creating directory {target_file_dir}: {e}")
                continue

        # 打印文件是否存在
        print(f"Checking if {file} exists...")
        if os.path.exists(file):
            try:
                shutil.copy(file, target_file_path)
                print(f"Copied modified file: {file} -> {target_file_path}")
            except Exception as e:
                print(f"Error copying file {file}: {e}")
        else:
            print(f"Warning: {file} doesn't exist (it might have been deleted)")

# 记录删除的文件
def record_deleted_files(deleted_files, target_dir):
    for file in deleted_files:
        # 去掉文件路径中的多余引号或不必要的反斜杠
        file = file.strip().replace('"', '')  # 去掉路径中的引号
        file = file.replace('\\', '/')  # 替换反斜杠为正斜杠

        target_file_path = os.path.join(target_dir, file)
        target_file_dir = os.path.dirname(target_file_path)

        # 打印目标文件路径，调试用
        print(f"Target directory: {target_file_dir}")
        print(f"Target file path: {target_file_path}")

        # 创建目标文件夹
        if not os.path.exists(target_file_dir):
            try:
                os.makedirs(target_file_dir)
            except OSError as e:
                print(f"Error creating directory {target_file_dir}: {e}")
                continue

        # 创建一个空文件来表示该删除文件
        open(target_file_path, 'w').close()
        print(f"Recorded deleted file: {file} -> {target_file_path}")

# 打包文件夹为压缩包
def create_zip_archive(folder_path, output_name):
    shutil.make_archive(output_name, 'zip', folder_path)
    print(f"Created zip archive: {output_name}.zip")

# 删除文件夹及其内容
def delete_folder(folder_path):
    if os.path.exists(folder_path):
        shutil.rmtree(folder_path)
        print(f"Deleted folder: {folder_path}")

if __name__ == "__main__":
    target_folder = './extracted_files'

    # 获取修改和删除的文件
    modified_files, deleted_files = get_modified_and_deleted_files()

    if modified_files or deleted_files:
        print(f"Found {len(modified_files)} modified files and {len(deleted_files)} deleted files, extracting...")

        # 复制修改的文件
        copy_modified_files(modified_files, target_folder)

        # 记录删除的文件
        record_deleted_files(deleted_files, target_folder)

        # 打包文件夹为压缩包
        create_zip_archive(target_folder, 'extracted_files')

        # 删除临时文件夹
        delete_folder(target_folder)

    else:
        print("No modified or deleted files found.")