using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using ExcelDataReader; // 必须引用

namespace ExcelTool
{
    class Program
    {
        private static readonly string BaseDir = AppDomain.CurrentDomain.BaseDirectory;

        private static string _clientRoot;
        public static string ClientRoot => _clientRoot ?? (_clientRoot = FindFolder("Client"));
        public static string SourceTablePath => FindFolder("Tables");

        private static string ClientBytesOutPath => Path.Combine(ClientRoot, "Assets/Game/Download/DataTable");
        private static string ClientCodeOutPath => Path.Combine(ClientRoot, "Assets/Game/Scripts/LogicScripts/Data/DataTable/Create");

        static void Main(string[] args)
        {
            // 解决 .NET Core/Modern .NET 环境下的编码问题，确保能读取 Excel 的特殊字符
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            try
            {
                ProcessExcelFiles();
                Console.WriteLine("All files processed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
            }
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static string FindFolder(string folderName)
        {
            DirectoryInfo dir = new DirectoryInfo(BaseDir);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, folderName)))
            {
                dir = dir.Parent;
            }
            return dir != null ? Path.Combine(dir.FullName, folderName) : BaseDir;
        }

        public static void ProcessExcelFiles()
        {
            string sourcePath = Path.GetFullPath(SourceTablePath);
            if (!Directory.Exists(sourcePath))
            {
                Console.WriteLine("Table directory not found: " + sourcePath);
                return;
            }

            // 过滤临时文件和非Excel文件
            var files = Directory.GetFiles(sourcePath, "*.*")
                .Where(s => (s.EndsWith(".xls") || s.EndsWith(".xlsx")) && !Path.GetFileName(s).StartsWith("~$"));

            foreach (string filePath in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                ReadData(filePath, fileName);
            }
        }

        private static void ReadData(string filePath, string fileName)
        {
            try
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var result = reader.AsDataSet();
                        if (result.Tables.Count == 0) return;

                        foreach (DataTable dt in result.Tables)
                        {
                            string sheetName = dt.TableName.Trim();

                            // 1. 校验子表名是否为默认的 Sheet1, Sheet2 等
                            if (System.Text.RegularExpressions.Regex.IsMatch(sheetName, @"^Sheet\d+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"[Warning] 文件 '{fileName}' 中的子表 '{sheetName}' 疑似未设置正确表名，已跳过。");
                                Console.ResetColor();
                                continue;
                            }

                            // 2. 检查子表内容是否为空
                            // GetValidRowCount 是你代码中已有的方法，判断第一列是否有数据
                            int rows = GetValidRowCount(dt);
                            int cols = GetValidColumnCount(dt);

                            if (rows < 3 || cols <= 0) 
                            {
                                // 如果行数不足3行（连表头都不够），判定为空表或无效表
                                Console.WriteLine($"[Info] 子表 '{sheetName}' 内容为空或格式不正确，已跳过。");
                                continue;
                            }

                            // 3. 执行提取逻辑
                            if (sheetName.Equals("Sys_Localization", StringComparison.OrdinalIgnoreCase))
                            {
                                CreateLocalization(sheetName, dt);
                            }
                            else
                            {
                                CreateData(sheetName, dt);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Read file [{fileName}] failed: " + ex.Message);
            }
        }

        private static void CreateData(string fileName, DataTable dt)
        {
            try
            {
                int validRows = GetValidRowCount(dt);
                int validCols = GetValidColumnCount(dt);
                if (validRows < 3) return;

                // 缓存表头：0-变量名, 1-类型, 2-描述
                string[,] headArr = new string[validCols, 3];
                for (int j = 0; j < validCols; j++)
                {
                    headArr[j, 0] = dt.Rows[0][j]?.ToString().Trim() ?? "";
                    headArr[j, 1] = dt.Rows[1][j]?.ToString().Trim() ?? "";
                    headArr[j, 2] = dt.Rows[2][j]?.ToString().Trim() ?? "";
                }

                byte[] buffer;
                using (var ms = new MMO_MemoryStream())
                {
                    ms.WriteInt(validRows - 3); // 实际数据行数
                    ms.WriteInt(validCols);

                    for (int i = 3; i < validRows; i++)
                    {
                        for (int j = 0; j < validCols; j++)
                        {
                            string type = headArr[j, 1].ToLower();
                            string val = dt.Rows[i][j]?.ToString()?.Trim() ?? "";
                            WriteValueToStream(ms, type, val);
                        }
                    }
                    buffer = ms.ToArray();
                }

                SaveClientFiles(fileName, headArr, buffer);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Process table [" + fileName + "] error: " + ex.Message);
            }
        }

        private static void WriteValueToStream(MMO_MemoryStream ms, string type, string val)
        {
            switch (type)
            {
                case "int": ms.WriteInt(int.TryParse(val, out int i) ? i : 0); break;
                case "long": ms.WriteLong(long.TryParse(val, out long l) ? l : 0); break;
                case "short": ms.WriteShort(short.TryParse(val, out short s) ? s : (short)0); break;
                case "float": ms.WriteFloat(float.TryParse(val, out float f) ? f : 0f); break;
                case "double": ms.WriteDouble(double.TryParse(val, out double d) ? d : 0d); break;
                case "byte": ms.WriteByte(byte.TryParse(val, out byte b) ? b : (byte)0); break;
                case "bool": ms.WriteBool(val == "1" || val.ToLower() == "true"); break;
                default: ms.WriteUTF8String(val); break;
            }
        }

        private static void SaveClientFiles(string fileName, string[,] headArr, byte[] buffer)
        {
            if (!Directory.Exists(ClientBytesOutPath)) Directory.CreateDirectory(ClientBytesOutPath);
            File.WriteAllBytes(Path.Combine(ClientBytesOutPath, fileName + ".bytes"), buffer);

            if (!Directory.Exists(ClientCodeOutPath)) Directory.CreateDirectory(ClientCodeOutPath);
            File.WriteAllText(Path.Combine(ClientCodeOutPath, fileName + "Entity.cs"), GenerateEntityCode(fileName, headArr));
            File.WriteAllText(Path.Combine(ClientCodeOutPath, fileName + "DBModel.cs"), GenerateDBModelCode(fileName, headArr));

            Console.WriteLine("Success: " + fileName);
        }

        private static string GenerateEntityCode(string fileName, string[,] headArr)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using GameScripts;");
            sb.AppendLine();
            sb.AppendLine("// " + fileName + " Entity");
            sb.AppendLine("public partial class " + fileName + "Entity : DataTableEntityBase");
            sb.AppendLine("{");
            for (int i = 0; i < headArr.GetLength(0); i++)
            {
                sb.AppendLine("    // " + headArr[i, 2]);
                sb.AppendLine("    public " + headArr[i, 1] + " " + headArr[i, 0] + ";");
            }
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string GenerateDBModelCode(string fileName, string[,] headArr)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using GameScripts;");
            sb.AppendLine();
            sb.AppendLine("public partial class " + fileName + "DBModel : DataTableDBModelBase<" + fileName + "DBModel, " + fileName + "Entity>");
            sb.AppendLine("{");
            sb.AppendLine("    public override string DataTableName => \"" + fileName + "\";");
            sb.AppendLine();
            sb.AppendLine("    protected override void LoadList(MMO_MemoryStream ms)");
            sb.AppendLine("    {");
            sb.AppendLine("        int rows = ms.ReadInt();");
            sb.AppendLine("        int columns = ms.ReadInt();");
            sb.AppendLine("        for (int i = 0; i < rows; i++)");
            sb.AppendLine("        {");
            sb.AppendLine("            var entity = new " + fileName + "Entity();");
            for (int i = 0; i < headArr.GetLength(0); i++)
            {
                string type = headArr[i, 1];
                string readMethod = GetReadMethod(type);
                string cast = type == "byte" ? "(byte)" : "";
                sb.AppendLine("            entity." + headArr[i, 0] + " = " + cast + "ms.Read" + readMethod + "();");
            }
            sb.AppendLine("            m_List.Add(entity);");
            sb.AppendLine("            m_Dic[entity.Id] = entity;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string GetReadMethod(string type)
        {
            switch (type.ToLower())
            {
                case "byte": return "Byte";
                case "int": return "Int";
                case "short": return "Short";
                case "long": return "Long";
                case "float": return "Float";
                case "double": return "Double";
                case "bool": return "Bool";
                default: return "UTF8String";
            }
        }

        private static int GetValidRowCount(DataTable dt)
        {
            for (int i = 0; i < dt.Rows.Count; i++)
                if (string.IsNullOrWhiteSpace(dt.Rows[i][0]?.ToString())) return i;
            return dt.Rows.Count;
        }

        private static int GetValidColumnCount(DataTable dt)
        {
            if (dt.Rows.Count == 0) return 0;
            for (int i = 0; i < dt.Columns.Count; i++)
                if (string.IsNullOrWhiteSpace(dt.Rows[0][i]?.ToString())) return i;
            return dt.Columns.Count;
        }

        private static void CreateLocalization(string fileName, DataTable dt)
        {
            string outPath = Path.Combine(ClientBytesOutPath, "Localization");
            if (!Directory.Exists(outPath)) Directory.CreateDirectory(outPath);

            int rows = GetValidRowCount(dt);
            int cols = GetValidColumnCount(dt);

            for (int j = 3; j < cols; j++)
            {
                string langName = dt.Rows[0][j].ToString().Trim();
                using (var ms = new MMO_MemoryStream())
                {
                    ms.WriteInt(rows - 3);
                    ms.WriteInt(2);
                    for (int i = 3; i < rows; i++)
                    {
                        ms.WriteUTF8String(dt.Rows[i][2]?.ToString().Trim() ?? "");
                        ms.WriteUTF8String(dt.Rows[i][j]?.ToString().Trim() ?? "");
                    }
                    File.WriteAllBytes(Path.Combine(outPath, langName + ".bytes"), ms.ToArray());
                }
            }
            Console.WriteLine("Localization bytes success.");
        }
    }
}