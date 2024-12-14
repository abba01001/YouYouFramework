import os
import re
import shutil
import subprocess

def find_proto_files(root_dir):
    """遍历目录以查找所有 .proto 文件。"""
    return [os.path.join(dirpath, filename)
            for dirpath, _, filenames in os.walk(root_dir)
            for filename in filenames if filename.endswith('.proto')]

def extract_messages(proto_file):
    """从 .proto 文件中提取消息定义。"""
    messages = {}
    try:
        with open(proto_file, 'r', encoding='utf-8') as file:
            content = file.read()
            message_pattern = r'message\s+(\w+)\s*\{'
            matches = re.findall(message_pattern, content)

            for message in matches:
                if message in messages:
                    messages[message].append(proto_file)
                else:
                    messages[message] = [proto_file]

    except Exception as e:
        print(f"无法读取文件 {proto_file}: {e}")
    return messages

def find_duplicates(messages):
    """查找重复的消息结构。"""
    return {name: files for name, files in messages.items() if len(files) > 1}

def clear_or_create_directory(dir_path):
    """清理或创建目录。"""
    if os.path.exists(dir_path):
        shutil.rmtree(dir_path)
    os.makedirs(dir_path)

def generate_protocol_files(proto_files, bin_dir, output_dir):
    """生成协议文件并返回生成的文件列表。"""
    clear_or_create_directory(output_dir)
    generated_files = []
    for proto_file in proto_files:
        print(f"文件: ...{proto_file} 处理成功")
        result = subprocess.run(
            [os.path.join(bin_dir, 'protoc.exe'), 
             f'--proto_path={os.path.dirname(proto_file)}', 
             f'--csharp_out={output_dir}', 
             proto_file],
            capture_output=True,
            text=True
        )
        
        if result.returncode != 0:
            print(f"生成失败: {os.path.basename(proto_file)}")
            print(result.stderr)
            exit(1)

        generated_files.append(os.path.basename(proto_file).replace('.proto', '.cs'))

    return generated_files

def copy_files_to_target(source_dir, target_dir):
    """复制文件到目标目录。"""
    for filename in os.listdir(source_dir):
        shutil.copy(os.path.join(source_dir, filename), target_dir)

def main():
    # 设置路径
    bin_dir = 'include'
    proto_root = 'local'
    output_dir = 'publish'
    client_dir = os.path.join('..', '..', 'Client', 'Assets', 'Game', 'Download', 'Protocol')
    server_dir = os.path.join('..', '..', 'Server', 'TCPServer', 'Protocol')

    # 查找 .proto 文件
    proto_files = find_proto_files(proto_root)
    
    if not proto_files:
        print("未找到 .proto 文件，生成失败!")
        input("按任意键继续...")
        exit(1)

    # 检查重复消息结构
    all_messages = {}
    for proto_file in proto_files:
        messages = extract_messages(proto_file)
        for name, files in messages.items():
            if name in all_messages:
                all_messages[name].extend(files)
            else:
                all_messages[name] = files

    duplicates = find_duplicates(all_messages)

    # 开始生成协议文件
    print("-----------------开始检测协议-----------------")
    
    if duplicates:
        print("发现重复的协议结构:")
        for name, files in duplicates.items():
            print(f"协议: {name}")
            for file in files:
                print(f"{file}")
        print("---------------协议数据结构不正常---------------")
        print("生成失败，因存在重复消息结构!")
        input("按任意键继续...")
        exit(1)
    else:
        print("---------------协议数据结构合法---------------")

    print("生成协议文件:")
    generated_files = generate_protocol_files(proto_files, bin_dir, output_dir)
    
    # 显示生成结果
    print("\n输出协议文件:")
    for file in generated_files:
        print(f"{os.path.join(os.getcwd(), output_dir, file)}")

    # 清理并创建目标文件夹
    clear_or_create_directory(client_dir)
    clear_or_create_directory(server_dir)  # 清理并创建服务器目录

    # 复制文件到目标文件夹
    copy_files_to_target(output_dir, client_dir)
    copy_files_to_target(output_dir, server_dir)  # 复制到服务器目录

    # 显示输出到客户端和服务器结果
    print("\n输出到客户端:")
    for file in generated_files:
        print(f"{os.path.join(client_dir, file)}")

    print("\n输出到服务端:")
    for file in generated_files:
        print(f"{os.path.join(server_dir, file)}")

    print("\n生成并复制成功!")
    input("按任意键继续...")

if __name__ == "__main__":
    main()
