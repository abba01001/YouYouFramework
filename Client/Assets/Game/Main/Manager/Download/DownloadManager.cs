// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.IO;
// using System.Threading.Tasks;
// using Cysharp.Threading.Tasks;
// using UnityEngine;
// using UnityEngine.Networking;
//
// namespace Main
// {
//     /// <summary>
//     /// 下载管理器
//     /// </summary>
//     public class DownloadManager
//     {
//         public int FlushSize { get; private set; }
//
//         public int DownloadRoutineCount { get; private set; }
//
//         /// <summary>
//         /// 连接失败后的重试次数
//         /// </summary>
//         public int Retry { get; private set; }
//
//         /// <summary>
//         /// 下载单文件器链表
//         /// </summary>
//         private LinkedList<DownloadRoutine> m_DownloadSingleRoutineList;
//
//         /// <summary>
//         /// 下载多文件器链表
//         /// </summary>
//         private LinkedList<DownloadMulitRoutine> m_DownloadMulitRoutineList;
//
//         internal DownloadManager()
//         {
//             m_DownloadSingleRoutineList = new LinkedList<DownloadRoutine>();
//             m_DownloadMulitRoutineList = new LinkedList<DownloadMulitRoutine>();
//             
//             Retry = MainEntry.ParamsSettings.GetGradeParamData(YFConstDefine.Download_Retry, MainEntry.CurrDeviceGrade);
//             DownloadRoutineCount = MainEntry.ParamsSettings.GetGradeParamData(YFConstDefine.Download_RoutineCount, MainEntry.CurrDeviceGrade);
//             FlushSize = MainEntry.ParamsSettings.GetGradeParamData(YFConstDefine.Download_FlushSize, MainEntry.CurrDeviceGrade);
//         }
//         internal void Dispose()
//         {
//             m_DownloadSingleRoutineList.Clear();
//
//             //调用下载多文件器的Dispose()
//             var mulitRoutine = m_DownloadMulitRoutineList.First;
//             while (mulitRoutine != null)
//             {
//                 mulitRoutine.Value.Dispose();
//                 mulitRoutine = mulitRoutine.Next;
//             }
//             m_DownloadMulitRoutineList.Clear();
//         }
//         /// <summary>
//         /// 更新
//         /// </summary>
//         internal void OnUpdate()
//         {
//             //调用下载单文件器的OnUpdate()
//             var singleRoutine = m_DownloadSingleRoutineList.First;
//             while (singleRoutine != null)
//             {
//                 singleRoutine.Value.OnUpdate();
//                 singleRoutine = singleRoutine.Next;
//             }
//
//             //调用下载多文件器的OnUpdate()
//             var mulitRoutine = m_DownloadMulitRoutineList.First;
//             while (mulitRoutine != null)
//             {
//                 mulitRoutine.Value.OnUpdate();
//                 mulitRoutine = mulitRoutine.Next;
//             }
//         }
//         internal async UniTask Init()
//         {
//
//         }
//
//         /// <summary>
//         /// 下载单个文件
//         /// </summary>
//         /// <param name="url"></param>
//         /// <param name="onUpdate"></param>
//         public void BeginDownloadSingle(string url, Action<string, ulong, float> onUpdate = null, Action<string> onComplete = null)
//         {
//             VersionFileEntity entity = MainEntry.Assets.VersionFile.GetVersionFileEntity(url);
//             if (entity == null)
//             {
//                 MainEntry.LogError(MainEntry.LogCategory.Assets, "无效资源包=>" + url);
//                 return;
//             }
//
//             DownloadRoutine routine = DownloadRoutine.Create();
//             routine.BeginDownload(url, entity, onUpdate, onComplete: (string fileUrl, DownloadRoutine r) =>
//             {
//                 m_DownloadSingleRoutineList.Remove(routine);
//                 if (onComplete != null) onComplete(fileUrl);
//             });
//             m_DownloadSingleRoutineList.AddLast(routine);
//         }
//         
//         
//         //TODO 这里需要优化版本存储信息
//         public async Task<string> GetAPKVersion(string url)
//         {
//             string result = null;
//             MainEntry.IsOfflineMode = true;
//             return "1.0.0\n1.0.0";
//             string firstInstallFlag = Path.Combine(Application.persistentDataPath, "first_install.txt");
//             bool isFirstInstall = !File.Exists(firstInstallFlag);
//             string targetFolder = Path.Combine(Application.streamingAssetsPath, "AssetBundles");
//             string versionFilePath = Path.Combine(targetFolder, "version.txt");
//             // isFirstInstall = true;
//             if (isFirstInstall)
//             {
//                 using (UnityWebRequest request = UnityWebRequest.Get(url))
//                 {
//                     request.timeout = 1;
//                     try
//                     {
//                         // 异步发送请求并等待
//                         await request.SendWebRequest();
//                         // 如果请求成功
//                         if (request.result == UnityWebRequest.Result.Success)
//                         {
//                             result = request.downloadHandler.text;
//                             // MainEntry.IsOfflineMode = true;
//                         }
//                         else
//                         {
//                             // 捕获超时或其他错误
//                             DownloadRoutine routine = DownloadRoutine.Create();
//                             result = await routine.DownAPKVersion(url, 3);
//                             Debug.LogError("Request failed: " + request.error);
//                         }
//                     }
//                     catch (UnityWebRequestException e)
//                     {
//                         // 捕获超时或其他错误
//                         DownloadRoutine routine = DownloadRoutine.Create();
//                         result = await routine.DownAPKVersion(url, 3);
//                         Debug.LogError("Request failed with error: " + e.Message);
//                     }
//                 };
//                 
//                 if (string.IsNullOrEmpty(result) && File.Exists(versionFilePath))
//                 {
//                     result = await File.ReadAllTextAsync(versionFilePath);
//                     MainEntry.IsOfflineMode = true;
//                 }
//                 Debug.LogError("2222===>" + result);
//             }
//             else
//             {
//                 DownloadRoutine routine = DownloadRoutine.Create();
//                 result = await routine.DownAPKVersion(url, 3);
//                 Debug.LogError("1111===>" + result);
//                 if (string.IsNullOrEmpty(result) && File.Exists(versionFilePath))
//                 {
//                     result = await File.ReadAllTextAsync(versionFilePath);
//                     MainEntry.IsOfflineMode = true;
//                 }
//             }
//             return result;
//         }
//
//
//         /// <summary>
//         /// 下载多个文件
//         /// </summary>
//         /// <param name="lstUrl"></param>
//         /// <param name="onDownloadMulitUpdate"></param>
//         /// <param name="onDownloadMulitComplete"></param>
//         public void BeginDownloadMulit(LinkedList<string> lstUrl, Action<int, int, ulong, ulong> onDownloadMulitUpdate = null, Action onDownloadMulitComplete = null)
//         {
//             DownloadMulitRoutine mulitRoutine = DownloadMulitRoutine.Create();
//             mulitRoutine.BeginDownloadMulit(lstUrl, onDownloadMulitUpdate, (DownloadMulitRoutine r) =>
//             {
//                 m_DownloadMulitRoutineList.Remove(r);
//                 onDownloadMulitComplete?.Invoke();
//             });
//             m_DownloadMulitRoutineList.AddLast(mulitRoutine);
//         }
//
//     }
// }