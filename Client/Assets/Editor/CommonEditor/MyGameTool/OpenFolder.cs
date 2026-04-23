using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace UnityGameFramework.Editor
{
    /// <summary>
    /// 打开文件夹相关的实用函数。
    /// </summary>
    public static class OpenFolder
    {
        /// <summary>
        /// 打开 Data Path 文件夹。
        /// </summary>
        [MenuItem("工具类/Other/Data Path", false, 99910)]
        public static void OpenFolderDataPath()
        {
            Execute(Application.dataPath);
        }

        /// <summary>
        /// 打开 Persistent Data Path 文件夹。
        /// </summary>
        [MenuItem("工具类/Other/Persistent Data Path", false, 99911)]
        public static void OpenFolderPersistentDataPath()
        {
            Execute(Application.persistentDataPath);
        }

        /// <summary>
        /// 打开 Streaming Assets Path 文件夹。
        /// </summary>
        [MenuItem("工具类/Other/Streaming Assets Path", false, 99912)]
        public static void OpenFolderStreamingAssetsPath()
        {
            Execute(Application.streamingAssetsPath);
        }

        /// <summary>
        /// 打开 Temporary Cache Path 文件夹。
        /// </summary>
        [MenuItem("工具类/Other/Temporary Cache Path", false, 99913)]
        public static void OpenFolderTemporaryCachePath()
        {
            Execute(Application.temporaryCachePath);
        }

#if UNITY_2018_3_OR_NEWER

        /// <summary>
        /// 打开 Console Log Path 文件夹。
        /// </summary>
        [MenuItem("工具类/Other/Console Log Path", false, 99914)]
        public static void OpenFolderConsoleLogPath()
        {
            Execute(System.IO.Path.GetDirectoryName(Application.consoleLogPath));
        }

#endif

        /// <summary>
        /// 打开指定路径的文件夹。
        /// </summary>
        /// <param name="folder">要打开的文件夹的路径。</param>
        public static void Execute(string folder)
        {
            folder = string.Format("\"{0}\"", folder);
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    Process.Start("Explorer.exe", folder.Replace('/', '\\'));
                    break;

                case RuntimePlatform.OSXEditor:
                    Process.Start("open", folder);
                    break;

                default:
                    throw new System.Exception();
            }
        }
    }
}
