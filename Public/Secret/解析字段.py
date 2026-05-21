import os
import json
import base64
from Crypto.Cipher import AES
from Crypto.Util.Padding import unpad

SECURITY_KEY = '3ZkPqF9hDjW8q2Z7'  # 16 字节密钥

def decrypt(encrypted_data):
    encrypted_data = base64.b64decode(encrypted_data)

    iv = encrypted_data[:AES.block_size]  # 提取 IV
    cipher = AES.new(SECURITY_KEY.encode('utf-8'), AES.MODE_CBC, iv)
    
    decrypted = cipher.decrypt(encrypted_data[AES.block_size:])
    
    try:
        return unpad(decrypted, AES.block_size).decode('utf-8')
    except ValueError as e:
        print(f"解密时发生错误: {e}")
        return None

def load_from_file(file_name="SecretKey.bytes"):
    current_dir_path = os.path.join(os.getcwd(), file_name)
    if not os.path.exists(current_dir_path):
        print(f"文件 {file_name} 在当前目录不存在！")
        return None
    
    with open(current_dir_path, 'rb') as f:
        return f.read()

def main():
    encrypted_data = load_from_file()
    if encrypted_data is None:
        return

    decrypted_data = decrypt(encrypted_data)
    
    if decrypted_data is None:
        print("解密失败，无法继续。")
        return

    credentials = json.loads(decrypted_data)

    print("\n解密后的数据：")
    for key, value in credentials.items():
        print(f"{key}: {value}")

    input("\n按任意键退出...")

if __name__ == "__main__":
    main()
