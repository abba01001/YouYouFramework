import os
import json
import base64
import re  # 用于正则表达式过滤特殊字符
from Crypto.Cipher import AES
from Crypto.Util.Padding import pad

SECURITY_KEY = '3ZkPqF9hDjW8q2Z7'  # 保持 16 字节密钥

# 移除特殊字符，确保字段名和值的安全
def sanitize_input(input_string, allow_dot=False):
    # 只允许字母、数字、下划线、连字符、空格，以及可选的点号
    pattern = r'[^\w\s.-]' if allow_dot else r'[^\w\s-]'
    return re.sub(pattern, '', input_string)

def encrypt(plain_text):
    cipher = AES.new(SECURITY_KEY.encode('utf-8'), AES.MODE_CBC)
    iv = cipher.iv
    encrypted = cipher.encrypt(pad(plain_text.encode('utf-8'), AES.block_size))
    return base64.b64encode(iv + encrypted).decode('utf-8')

def save_to_file(encrypted_data, file_name):
    # 当前目录路径
    current_dir_path = os.path.join(os.getcwd(), file_name)
    with open(current_dir_path, 'wb') as f:
        f.write(base64.b64decode(encrypted_data))

    # Client 目录路径
    client_dir_path = os.path.join('..', '..', 'Client', 'Assets', 'PackageTool', file_name)
    with open(client_dir_path, 'wb') as f:
        f.write(base64.b64decode(encrypted_data))

def main():
    credentials = {}

    print("请输入字段和值，输入 'done' 完成并生成文件")
    while True:
        field = input("\n请输入字段名称 (例如：SecretId, SecretKey, Region 等)，或输入 'done' 完成输入: ")

        if field.lower() == 'done':  # 如果用户输入 'done'，则结束输入
            break
        
        sanitized_field = sanitize_input(field)  # 过滤字段名称
        if not sanitized_field:
            print("字段名称无效，请重新输入！")
            continue

        value = input("请输入字段的值: ")
        sanitized_value = sanitize_input(value, allow_dot=True)  # 允许点号
        if not sanitized_value:
            print("字段值无效，请重新输入！")
            continue
        
        # 将过滤后的字段和值添加到字典
        credentials[sanitized_field] = sanitized_value

    # 序列化为 JSON 字符串
    credentials_json = json.dumps(credentials)

    # 加密
    encrypted_credentials = encrypt(credentials_json)

    # 获取用户输入的文件名
    file_name = input("请输入保存的文件名（不需要后缀名，默认为 '存储加密文件'）: ") or "存储加密文件"
    file_name += ".bytes"  # 添加后缀名

    # 保存到文件
    save_to_file(encrypted_credentials, file_name)

    print(f"\n加密数据已保存到当前目录和 Client/Assets/PackageTool 目录下的 '{file_name}' 文件。")
    input("按任意键退出...")

if __name__ == "__main__":
    main()
