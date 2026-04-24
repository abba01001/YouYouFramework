using HybridCLR;
using System.Net.Sockets;
using System.Text;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Main
{
    public class HotfixManager
    {
        private static AssetBundle hotfixAb;
        public static HotfixManager Instance { get; private set; } = new();

        public HotfixManager()
        {
            Init();
            
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
            var operation = CheckVersionCtrl.Instance.DefaultPackage.LoadAssetAsync("Assets/Game/Download/Hotfix/GameScripts.dll.bytes");
            await operation.Task;
            var hotfixAsset = operation.AssetObject as TextAsset;
            System.Reflection.Assembly.Load(hotfixAsset.bytes);
            Debugger.Log("GameScripts.dll加载完毕");
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
        
        [BoxGroup("通用参数设置")][LabelText("本地服务器IP")] public string LocalServerUrl { get; private set; }
        [BoxGroup("通用参数设置")][LabelText("本地AB包资源IP")] public string LocalAssetUrl{ get; private set; }
        [BoxGroup("通用参数设置")][LabelText("本地AB包资源IP")] public string LocalVersionUrl{ get; private set; }
        
        [BoxGroup("通用参数设置")][LabelText("云端服务器IP")] public string RemoteServerUrl{ get; private set; }
        [BoxGroup("通用参数设置")][LabelText("云端AB包资源IP")] public string RemoteAssetUrl{ get; private set; }
        [BoxGroup("通用参数设置")][LabelText("本地AB包资源IP")] public string RemoteVersionUrl{ get; private set; }
        public void Init()
        {
            LocalServerUrl = GetLocalIPAddress();
            LocalAssetUrl = GetLocalAssetIpAddress();
            LocalVersionUrl = GetLocalVersionIpAddress();
                
            RemoteServerUrl = "43.134.133.178:17888";
            RemoteAssetUrl = $"http://storage.abba01001.cn/private_files/ServerBundles/{Application.platform}/{Application.version}";
            RemoteVersionUrl = $"http://storage.abba01001.cn/private_files/ServerBundles/{Application.platform}";
        }
        
        public string GetLocalAssetIpAddress()
        {
            // return "http://" + "192.168.18.130" + $":8000/{Application.platform}/{Application.version}";
            
#if UNITY_ANDROID && !UNITY_EDITOR
            return "http://" + "10.0.2.2" + $":8000/{Application.platform}/{Application.version}";
#endif
            foreach (var netInterface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                if (netInterface.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up)
                    continue;

                var props = netInterface.GetIPProperties();
                foreach (var addr in props.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == AddressFamily.InterNetwork &&
                        !addr.Address.ToString().StartsWith("127"))
                    {
                        return "http://" + addr.Address.ToString() + $":8000/{Application.platform}/{Application.version}";
                    }
                }
            }
            return "http://" + "127.0.0.1" + $":8000/Android/{Application.version}";
        }
    
        public string GetLocalVersionIpAddress()
        {
            // return "http://" + "192.168.18.130" + $":8000/{Application.platform}";
            
#if UNITY_ANDROID && !UNITY_EDITOR
            return "http://" + "10.0.2.2" + $":8000/{Application.platform}";
#endif
            foreach (var netInterface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                if (netInterface.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up)
                    continue;

                var props = netInterface.GetIPProperties();
                foreach (var addr in props.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == AddressFamily.InterNetwork &&
                        !addr.Address.ToString().StartsWith("127"))
                    {
                        return "http://" + addr.Address.ToString() + $":8000/{Application.platform}";
                    }
                }
            }
            return "http://" + "127.0.0.1" + $":8000/Android";
        }

        
        public string GetLocalIPAddress()
        {
            // return "192.168.18.130:17888";
#if UNITY_ANDROID && !UNITY_EDITOR
            return "10.0.2.2:17888";
#endif
            foreach (var netInterface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                if (netInterface.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up)
                    continue;

                var props = netInterface.GetIPProperties();
                foreach (var addr in props.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == AddressFamily.InterNetwork &&
                        !addr.Address.ToString().StartsWith("127"))
                    {
                        return addr.Address.ToString() + ":17888";
                    }
                }
            }
            return "127.0.0.1:17888";
        }

        public string GetAssetIP()
        {
#if SERVERMODE
            return RemoteAssetUrl;
#elif LOCALMODE
            return LocalAssetUrl;
#endif
            return LocalAssetUrl;
        }
        
        public string GetVersionIP()
        {
#if SERVERMODE
            return RemoteVersionUrl;
#elif LOCALMODE
            return LocalVersionUrl;
#endif
            return LocalVersionUrl;
        }
        
        public string GetServerIP()
        {
#if SERVERMODE
            return RemoteServerUrl;
#elif LOCALMODE
            return LocalServerUrl;
#endif
            return LocalServerUrl;
        }
    }
}