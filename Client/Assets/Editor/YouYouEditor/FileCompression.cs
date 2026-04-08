using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class FileCompression : MonoBehaviour
{
    private static string selectedFilePath;

    [MenuItem("Assets/工具/压缩文件")]
    private static void CompressSelectedFile()
    {
        string[] selectedAssets = Selection.assetGUIDs;
        if (selectedAssets.Length == 1)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(selectedAssets[0]);
            selectedFilePath = Path.Combine(assetPath);
            CompressFile(selectedFilePath);
        }
        else
        {
            Debug.LogError("Please select a single file for compression.");
        }
    }

    [MenuItem("Assets/工具/解压文件")]
    private static void DecompressSelectedFile()
    {
        string[] selectedAssets = Selection.assetGUIDs;
        if (selectedAssets.Length == 1)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(selectedAssets[0]);
            selectedFilePath = Path.Combine(assetPath);
            DecompressFile();
        }
        else
        {
            Debug.LogError("Please select a single file for decompression.");
        }
    }

    private static void CompressFile(string sourceFilePath)
    {
        try
        {
            if (string.IsNullOrEmpty(sourceFilePath))
            {
                Debug.LogError("Please select a file before compressing.");
                return;
            }

            string sourceDirectoryName = Path.GetDirectoryName(sourceFilePath);
            string saveDirectoryPath = Path.Combine(sourceDirectoryName, "Compressed");
            Directory.CreateDirectory(saveDirectoryPath);

            // 使用Path.GetFileName获取选中文件的文件名，并添加.zip后缀
            string saveFileName = Path.GetFileName(sourceFilePath) + ".zip";
            string savePath = Path.Combine(saveDirectoryPath, saveFileName);

            using (FileStream fsOut = File.Create(savePath))
            {
                using (ZipOutputStream zipStream = new ZipOutputStream(fsOut))
                {
                    byte[] buffer = new byte[4096];

                    // 使用Path.GetFileName获取选中文件的文件名
                    ZipEntry entry = new ZipEntry(Path.GetFileName(sourceFilePath));
                    zipStream.PutNextEntry(entry);

                    using (FileStream fsIn = File.OpenRead(sourceFilePath))
                    {
                        int sourceBytes;
                        do
                        {
                            sourceBytes = fsIn.Read(buffer, 0, buffer.Length);
                            zipStream.Write(buffer, 0, sourceBytes);
                        } while (sourceBytes > 0);
                    }
                    zipStream.Finish();
                }
                AssetDatabase.Refresh();
            }

            Debug.Log($"File compressed successfully and saved to: {savePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error compressing file: {ex.Message}");
        }
    }

    private static void DecompressFile()
    {
        try
        {
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                Debug.LogError("Please select a file before decompressing.");
                return;
            }

            string extractPath = Path.Combine(Path.GetDirectoryName(selectedFilePath), "Decompressed");
            Directory.CreateDirectory(extractPath);

            using (ZipInputStream zipStream = new ZipInputStream(File.OpenRead(selectedFilePath)))
            {
                ZipEntry entry;
                while ((entry = zipStream.GetNextEntry()) != null)
                {
                    string entryName = entry.Name;
                    string fullPath = Path.Combine(extractPath, entryName);

                    if (entry.IsDirectory)
                    {
                        Directory.CreateDirectory(fullPath);
                    }
                    else
                    {
                        using (FileStream fsOut = File.Create(fullPath))
                        {
                            byte[] buffer = new byte[4096];
                            int sourceBytes;
                            do
                            {
                                sourceBytes = zipStream.Read(buffer, 0, buffer.Length);
                                fsOut.Write(buffer, 0, sourceBytes);
                            } while (sourceBytes > 0);
                        }
                    }
                }
                AssetDatabase.Refresh();
            }

            Debug.Log($"File decompressed successfully to: {extractPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error decompressing file: {ex.Message}");
        }
    }
}
