import os
import re
import struct
import glob
import pandas as pd

# 基础目录定位
BaseDir = os.path.abspath(os.path.dirname(__file__))

def FindFolder(folder_name):
    current_dir = BaseDir
    while current_dir and not os.path.exists(os.path.join(current_dir, folder_name)):
        parent_dir = os.path.dirname(current_dir)
        if parent_dir == current_dir:  # 到达根目录
            current_dir = None
            break
        current_dir = parent_dir
    return os.path.join(current_dir, folder_name) if current_dir else os.path.join(BaseDir, folder_name)

# 路径定义
ClientRoot = FindFolder("Client")
SourceTablePath = FindFolder("Tables")

ClientBytesOutPath = os.path.join(ClientRoot, "Assets/Game/Download/DataTable")
ClientCodeOutPath = os.path.join(ClientRoot, "Assets/Game/Scripts/LogicScripts/Data/DataTable/Create")
ClientExtendOutPath = os.path.join(ClientRoot, "Assets/Game/Scripts/LogicScripts/Data/DataTable/Extend")


class MMO_MemoryStream:
    """完美对应 C# MMO_MemoryStream 的二进制小端序写入机制"""
    def __init__(self):
        self.buffer = bytearray()

    def WriteInt(self, v): self.buffer.extend(struct.pack('<i', v))
    def WriteLong(self, v): self.buffer.extend(struct.pack('<q', v))
    def WriteShort(self, v): self.buffer.extend(struct.pack('<h', v))
    def WriteFloat(self, v): self.buffer.extend(struct.pack('<f', v))
    def WriteDouble(self, v): self.buffer.extend(struct.pack('<d', v))
    def WriteByte(self, v): self.buffer.extend(struct.pack('B', v))
    def WriteBool(self, v): self.buffer.extend(struct.pack('B', 1 if v else 0))
    def WriteUTF8String(self, s):
        data = str(s if s else "").encode('utf-8')
        if len(data) > 65535: raise ValueError("String too long")
        self.buffer.extend(struct.pack('<H', len(data)))  # 使用 ushort 写入长度前缀
        self.buffer.extend(data)

    def to_array(self): return bytes(self.buffer)


def parse_int(val):
    try: return int(float(val))
    except: return 0

def parse_float(val):
    try: return float(val)
    except: return 0.0

def GetValidRowCount(df):
    for i in range(len(df)):
        val = df.iloc[i, 0]
        if pd.isna(val) or str(val).strip() == "": return i
    return len(df)

def GetValidColumnCount(df):
    if len(df) == 0: return 0
    for j in range(df.shape[1]):
        val = df.iloc[0, j]
        if pd.isna(val) or str(val).strip() == "": return j
    return df.shape[1]


def WriteValueToStream(ms, data_type, val):
    t = data_type.lower()
    if t == "int": ms.WriteInt(parse_int(val))
    elif t == "long": ms.WriteLong(parse_int(val))
    elif t == "short": ms.WriteShort(parse_int(val))
    elif t == "float": ms.WriteFloat(parse_float(val))
    elif t == "double": ms.WriteDouble(parse_float(val))
    elif t == "byte": ms.WriteByte(parse_int(val) & 0xFF)
    elif t == "bool": ms.WriteBool(val == "1" or val.lower() == "true")
    else: ms.WriteUTF8String(val)


def GetReadMethod(data_type):
    t = data_type.lower()
    if t == "byte": return "Byte"
    elif t == "int": return "Int"
    elif t == "short": return "Short"
    elif t == "long": return "Long"
    elif t == "float": return "Float"
    elif t == "double": return "Double"
    elif t == "bool": return "Bool"
    return "UTF8String"


def AppendFileHeader(sb, file_name):
    sb.append("// ========================================================")
    sb.append("// 此配置由python工具自动生成，请勿手动修改！")
    sb.append(f"// 如需拓展请在 Extend 目录下{file_name + 'DBModelExt.cs'}对应类进行扩展接口")
    sb.append("// ========================================================\n")


def GenerateDataTableCode(file_name, head_arr):
    sb = []
    AppendFileHeader(sb, file_name)
    sb.append("using System.Collections.Generic;\n")
    sb.append("namespace GameScripts")
    sb.append("{")
    sb.append(f"    // {file_name} Entity")
    sb.append(f"    public partial class {file_name}Entity : ConfigEntityBase")
    sb.append("    {")
    for h in head_arr:
        sb.append(f"        // {h[2]}")
        sb.append(f"        public {h[1]} {h[0]};")
    sb.append("    }\n")
    
    sb.append(f"    public partial class {file_name}DBModel : ConfigBase<{file_name}DBModel, {file_name}Entity>")
    sb.append("    {")
    sb.append(f'        public override string ConfigName => "{file_name}";\n')
    sb.append("        protected override void LoadList(MMO_MemoryStream ms)")
    sb.append("        {")
    sb.append("            int rows = ms.ReadInt();")
    sb.append("            int columns = ms.ReadInt();")
    sb.append("            for (int i = 0; i < rows; i++)")
    sb.append("            {")
    sb.append(f"                var entity = new {file_name}Entity();")
    for h in head_arr:
        cast = "(byte)" if h[1].lower() == "byte" else ""
        sb.append(f"                entity.{h[0]} = {cast}ms.Read{GetReadMethod(h[1])}();")
    sb.append("                m_List.Add(entity);")
    sb.append("                m_Dic[entity.Id] = entity;")
    sb.append("            }")
    sb.append("        }")
    sb.append("    }")
    sb.append("}")
    return "\n".join(sb)


def GenerateExtensionCode(file_name):
    sb = [
        "using System.Collections.Generic;",
        "using Main;\n",
        "namespace GameScripts",
        "{",
        f"    public partial class {file_name}DBModel",
        "    {",
        "        // 在这里添加你的自定义扩展逻辑，例如：",
        f"        // public Dictionary<int, {file_name}Entity> IdByDic;\n",
        "        protected override void OnLoadListComple()",
        "        {",
        "            base.OnLoadListComple();",
        "        }",
        "    }",
        "}"
    ]
    return "\n".join(sb)


def SaveClientFiles(file_name, head_arr, buffer):
    if not os.path.exists(ClientBytesOutPath): os.makedirs(ClientBytesOutPath)
    with open(os.path.join(ClientBytesOutPath, f"{file_name}.bytes"), "wb") as f:
        f.write(buffer)

    if not os.path.exists(ClientCodeOutPath): os.makedirs(ClientCodeOutPath)
    combined_code = GenerateDataTableCode(file_name, head_arr)
    with open(os.path.join(ClientCodeOutPath, f"{file_name}DBModel.cs"), "w", encoding="utf-8") as f:
        f.write(combined_code)

    if not os.path.exists(ClientExtendOutPath): os.makedirs(ClientExtendOutPath)
    ext_file_path = os.path.join(ClientExtendOutPath, f"{file_name}DBModelExt.cs")
    if not os.path.exists(ext_file_path):
        with open(ext_file_path, "w", encoding="utf-8") as f:
            f.write(GenerateExtensionCode(file_name))
        print(f"Auto-generated extension: {file_name}DBModelExt.cs")

    print(f"Success: {file_name}")


def CreateData(file_name, df):
    try:
        valid_rows = GetValidRowCount(df)
        valid_cols = GetValidColumnCount(df)
        if valid_rows < 3: return

        head_arr = []
        for j in range(valid_cols):
            c_name = str(df.iloc[0, j]).strip() if pd.notnull(df.iloc[0, j]) else ""
            c_type = str(df.iloc[1, j]).strip() if pd.notnull(df.iloc[1, j]) else ""
            c_desc = str(df.iloc[2, j]).strip() if pd.notnull(df.iloc[2, j]) else ""
            head_arr.append([c_name, c_type, c_desc])

        ms = MMO_MemoryStream()
        ms.WriteInt(valid_rows - 3)
        ms.WriteInt(valid_cols)

        for i in range(3, valid_rows):
            for j in range(valid_cols):
                data_type = head_arr[j][1]
                val = str(df.iloc[i, j]).strip() if pd.notnull(df.iloc[i, j]) else ""
                WriteValueToStream(ms, data_type, val)

        buffer = ms.to_array()
        SaveClientFiles(file_name, head_arr, buffer)
    except Exception as ex:
        print(f"Process table [{file_name}] error: {ex}")


def ReadData(file_path, file_name):
    try:
        # 使用 pandas 读取整个 excel 内的所有分表
        all_sheets = pd.read_excel(file_path, sheet_name=None, header=None)
        if not all_sheets: return

        for sheet_name, df in all_sheets.items():
            sheet_name = sheet_name.strip()

            # 1. 校验子表名是否为默认名
            if re.match(r"^Sheet\d+$", sheet_name, re.IGNORECASE):
                print(f"[Warning] 文件 '{file_name}' 中的子表 '{sheet_name}' 疑似未设置正确表名，已跳过。")
                continue

            # 2. 检查子表内容是否为空
            rows = GetValidRowCount(df)
            cols = GetValidColumnCount(df)

            if rows < 3 or cols <= 0:
                print(f"[Info] 子表 '{sheet_name}' 内容为空或格式不正确，已跳过。")
                continue

            CreateData(sheet_name, df)
            
    except Exception as ex:
        print(f"Read file [{file_name}] failed: {ex}")


def ProcessExcelFiles():
    source_path = os.path.abspath(SourceTablePath)
    if not os.path.exists(source_path):
        print("Table directory not found: " + source_path)
        return

    # 过滤临时文件和非Excel文件
    all_files = glob.glob(os.path.join(source_path, "*.*"))
    files = [f for f in all_files if (f.endswith(".xls") or f.endswith(".xlsx")) and not os.path.basename(f).startswith("~$")]

    for file_path in files:
        file_name = os.path.splitext(os.path.basename(file_path))[0]
        ReadData(file_path, file_name)


if __name__ == "__main__":
    try:
        ProcessExcelFiles()
        print("All files processed successfully.")
    except Exception as ex:
        print("Error occurred: " + str(ex))
    
    input("Press Enter to exit...")
