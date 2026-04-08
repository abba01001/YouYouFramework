using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Main
{
    public class HotfixManager
    {
        private static AssetBundle hotfixAb;
        public static HotfixManager Instance { get; private set; } = new();

        public HotfixManager()
        {
            //这里防止热更工程找不到AOT工程的类
            System.Data.AcceptRejectRule acceptRejectRule = System.Data.AcceptRejectRule.None;
            System.Net.WebSockets.WebSocketReceiveResult webSocketReceiveResult = null;
        }

        public async UniTask LoadHotifx()
        {
#if UNITY_EDITOR
            Debugger.Log("编辑器模式 不需要加载HybridCLR");
            return;
#endif
            await LoadMetadataForAOTAssemblies();

            //加载热更Dll
            var operation = CheckVersionCtrl.Instance.DefaultPackage.LoadAssetAsync("Assets/Game/Download/Hotfix/Assembly-CSharp.dll.bytes");
            await operation.Task;
            var hotfixAsset = operation.AssetObject as TextAsset;
            System.Reflection.Assembly.Load(hotfixAsset.bytes);
            Debugger.Log("Assembly-CSharp.dll加载完毕");
        }
        
        /// <summary>
        /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
        /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
        /// </summary>
        private async UniTask LoadMetadataForAOTAssemblies()
        {
            /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
            /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
            /// 
            HomologousImageMode mode = HomologousImageMode.SuperSet;
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var aotDllName in AOTGenericReferences.PatchedAOTAssemblyList)
            {
                var operation = CheckVersionCtrl.Instance.DefaultPackage.LoadAssetAsync($"Assets/Game/Download/Hotfix/{aotDllName}.bytes");
                await operation.Task;
                var asset = operation.AssetObject as TextAsset;
                // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(asset.bytes, mode);
                stringBuilder.Append(aotDllName + "\n");
            }
            Debugger.Log("补充元数据Dll加载完毕==\n" + stringBuilder.ToString());
        }
        
    }
}