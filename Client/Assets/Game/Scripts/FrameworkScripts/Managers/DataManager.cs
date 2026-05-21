using System;
using MessagePack;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Protocols;
using UnityEngine;

namespace GameScripts
{

    public enum PropEnum
    {
        Coin = 1,
        BattleCoin = 2,
        Energy = 3,
    }
    
    public interface IDataManager
    {
        void SaveData(bool writeLocal = true, bool ignoreLocalTime = false, bool writeCloud = false, bool ignoreCloudTime = false);
        void SaveData(bool forceWriteCloud);
        PlayerRoleData PlayerRoleData { get; set; }
        int DataUpdateTime { get; set; }
        string PrintUserData();
    }


    [MessagePackObject(keyAsPropertyName: true)]
    public class DataManager : IDataManager
    {
        #region 持久化数据
        public string UserId { get; set; } = Guid.NewGuid().ToString("N");
        public bool IsFirstLoginTime { get; set; }
        public int DataUpdateTime { get; set; }
        public int LastRefreshTime { get; set; }
        public PlayerRoleData PlayerRoleData { get; set; } = new();
        public StageSaveData StageSaveData { get; set; } = new();
        #endregion

        #region 临时数据
        [IgnoreMember] public List<List<ChatMsg>> TempChatMsgs { get; set; } = new();
        [IgnoreMember] private float lastWriteTime;
        [IgnoreMember] private float lastUploadTime;
        [IgnoreMember] private const float WriteCooldown = 5f;
        [IgnoreMember] private const float UploadCooldown = 10f;
        
        public static bool IsDataDirty { get; set; } 
        [IgnoreMember] public bool IsDirty { get => IsDataDirty; set => IsDataDirty = value; }
        #endregion

        #region 初始化逻辑
        public void InitGameData(byte[] datas)
        {
            if (datas == null || datas.Length == 0)
            {
                InitializeDefault();
                return;
            }

            try
            {
                // 优化：直接反序列化覆盖当前对象，无需反射
                var newData = MessagePackSerializer.Deserialize<DataManager>(datas);
                CoverData(newData);
                IsDirty = false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Data] 反序列化失败: {ex.Message}");
                InitializeDefault();
            }
        }

        private void InitializeDefault()
        {
            PlayerRoleData = new PlayerRoleData();
            StageSaveData = new StageSaveData();
            IsDirty = true;
        }

        private void CoverData(DataManager source)
        {
            this.UserId = source.UserId;
            this.PlayerRoleData = source.PlayerRoleData ?? new();
            this.StageSaveData = source.StageSaveData ?? new();
            this.DataUpdateTime = source.DataUpdateTime;
            this.LastRefreshTime = source.LastRefreshTime;
            this.IsFirstLoginTime = source.IsFirstLoginTime;
        }
        #endregion

        #region 保存逻辑

        public void SaveData(bool forceWriteCloud) => SaveData(true, true, forceWriteCloud, true);

        // [Button("手动保存 (编辑器用)")]
        public void SaveData(bool writeLocal = true, bool ignoreLocalTime = false, bool writeCloud = true, bool ignoreCloudTime = false)
        {
            // 核心逻辑：数据没变 且 没开启强制保存，则直接跳过
            bool isForced = ignoreLocalTime || ignoreCloudTime;
            if (!IsDirty && !isForced) 
            {
                return;
            }

            this.DataUpdateTime = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var binaryData = MessagePackSerializer.Serialize(this);
            var base64Str = Convert.ToBase64String(binaryData);

            // 本地保存
            if (writeLocal && (Time.time - lastWriteTime >= WriteCooldown || ignoreLocalTime))
            {
                PlayerPrefs.SetString("SaveData", base64Str);
                lastWriteTime = Time.time;
                IsDirty = false; // 重置标记
                Debug.Log("<color=green>本地数据已保存</color>");
            }

            // 云端保存
            if (writeCloud && NetManager.Instance.IsConnectServer)
            {
                if (Time.time - lastUploadTime >= UploadCooldown || ignoreCloudTime)
                {
                    UploadTask(base64Str).Forget(); // 使用 UniTask 替代 Observable.Interval
                }
            }
        }

        private async UniTaskVoid UploadTask(string data)
        {
            SDKManager.Instance.UploadGameData(UserId, data);
            lastUploadTime = Time.time;
            Debug.Log("<color=cyan>云端数据已上传</color>");
        }
        #endregion
        
        public string PrintUserData()
        {
            var str = MessagePackSerializer.SerializeToJson(this, MessagePackSerializer.DefaultOptions);
            Debug.Log(str);
            return str;
        }
    }
}