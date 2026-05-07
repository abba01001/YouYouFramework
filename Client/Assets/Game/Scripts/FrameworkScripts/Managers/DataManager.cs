using System;
using Main;
using MessagePack;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using GameScripts;
using Protocols;
using UniRx;
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
        int GetProps(int prop_id);
        void AddProp(int prop_id, int value);
        void DelProp(int prop_id, int value);
        void DelPropAll(int prop_id);
        bool CanAfford(int prop_id, int value);
    }


    [MessagePackObject(keyAsPropertyName: true)]
    public class DataManager : IDataManager

    {
        #region 持久化数据
        private string _user_id;
        public string UserId
        {
            get
            {
                if (string.IsNullOrEmpty(_user_id)) _user_id = Guid.NewGuid().ToString("N");
                return _user_id;
            }
            set { _user_id = value; }
        }


        private bool _is_first_login_time;
        public bool IsFirstLoginTime
        {
            get => _is_first_login_time;
            set => _is_first_login_time = value;
        }

        private int _data_update_time; //保存数据时的时间戳
        public int DataUpdateTime
        {
            get => _data_update_time;
            set { _data_update_time = value; }
        }


        private int _last_refresh_time; //刷新时间
        public int LastRefreshTime
        {
            get => _last_refresh_time;
            set { _last_refresh_time = value; }
        }


        private PlayerRoleData _playerRoleData;
        public PlayerRoleData PlayerRoleData
        {
            get => _playerRoleData;
            set => _playerRoleData = value;
        }

        #endregion


        #region 临时public数据
        [IgnoreMember] public List<List<ChatMsg>> TempChatMsgs { get; set; } = new List<List<ChatMsg>>();
        #endregion


        #region 临时private数据
        [IgnoreMember] private float lastWriteTime = 0f; // 上次写入时间
        [IgnoreMember] private float lastUploadTime = 0f; // 上次上传时间
        [IgnoreMember] private float writeCooldown = 5f; // 写入的冷却时间（5秒）
        [IgnoreMember] private float uploadCooldown = 10f; // 上传的冷却时间（10秒）
        #endregion


        #region Public方法
        public string PrintUserData()

        {
            var str = MessagePackSerializer.SerializeToJson(this, MessagePackSerializer.DefaultOptions);

            Debug.Log(str);

            return str;
        }

        public int GetProps(int prop_id)
        {
            _playerRoleData.propDic.TryGetValue(prop_id, out var value);
            return value;
        }

        public bool CanAfford(int prop_id, int value)
        {
            int hasValue = GetProps(prop_id);
            return hasValue >= value;
        }

        public void AddProp(int prop_id, int value)
        {
            if (!_playerRoleData.propDic.TryAdd(prop_id, value))
            {
                _playerRoleData.propDic[prop_id] += value;
            }

            PropChangeModel model = GameEntry.ClassObjectPool.Dequeue<PropChangeModel>();
            model.PropType = (PropEnum)prop_id;
            model.PropValue = GetProps(prop_id);
            GameEntry.Event.Dispatch(Constants.EventName.PropsChangedEvent,model);
            GameEntry.ClassObjectPool.Enqueue(model);
        }

        public void DelProp(int prop_id, int value)
        {
            if (_playerRoleData.propDic.ContainsKey(prop_id))
            {
                _playerRoleData.propDic[prop_id] -= value;
            }
            PropChangeModel model = GameEntry.ClassObjectPool.Dequeue<PropChangeModel>();
            model.PropType = (PropEnum)prop_id;
            model.PropValue = GetProps(prop_id);
            GameEntry.Event.Dispatch(Constants.EventName.PropsChangedEvent,model);
            GameEntry.ClassObjectPool.Enqueue(model);
        }
        
        public void DelPropAll(int prop_id)
        {
            if (_playerRoleData.propDic.ContainsKey(prop_id))
            {
                _playerRoleData.propDic[prop_id] = 0;
            }
            PropChangeModel model = GameEntry.ClassObjectPool.Dequeue<PropChangeModel>();
            model.PropType = (PropEnum)prop_id;
            model.PropValue = GetProps(prop_id);
            GameEntry.Event.Dispatch(Constants.EventName.PropsChangedEvent,model);
            GameEntry.ClassObjectPool.Enqueue(model);
        }

        public void OnUpdate()

        {
        }


        public void InitGameData(byte[] datas)
        {
            InitializeWithDefaultData();
            if (datas is { Length: > 0 })
            {
                try
                {
                    // StringBuilder logBuilder = new StringBuilder();
                    DataManager mc2 = MessagePackSerializer.Deserialize<DataManager>(datas);
                    PropertyInfo[] properties = mc2.GetType().GetProperties();
                    // logBuilder.AppendLine($"========初始化GameData========");
                    foreach (var property in properties)
                    {
                        var value = property.GetValue(mc2);
                        var targetProperty = this.GetType().GetProperty(property.Name);
                        // logBuilder.AppendLine($"设置 {property.Name} =======> {value}");
                        if (targetProperty != null && targetProperty.CanWrite && value != null)
                        {
                            targetProperty.SetValue(this, value);
                        }
                    }
                    
                    // string json = MessagePackSerializer.SerializeToJson(this, MessagePackSerializer.DefaultOptions);
                    // logBuilder.AppendLine(json);
                    // Debugger.LogError(logBuilder.ToString());
                }

                catch (MessagePackSerializationException ex)
                {
                    Debugger.LogError($"初始化失败 {ex.Message}===>使用默认值初始化");
                }
            }
            else
            {
                Debugger.LogError($"数据库没有数据====>使用默认值初始化");
            }
        }


        public void SaveData(bool forceWriteCloud)
        {
            SaveData(true, true, forceWriteCloud, true);
        }


        private IDisposable SaveAction = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writeLocal">写入本地</param>
        /// <param name="ignoreLocalTime">无视cd写入</param>
        /// <param name="writeCloud">写入云端</param>
        /// <param name="ignoreCloudTime">无视cd写入</param>
        public void SaveData(bool writeLocal = true, bool ignoreLocalTime = false, bool writeCloud = false,bool ignoreCloudTime = false)
        {
            _data_update_time = 1;
            var binaryData = MessagePackSerializer.Serialize(this, MessagePackSerializer.DefaultOptions);
            var str = Convert.ToBase64String(binaryData);
            if (writeLocal)
            {
                if (Time.time - lastWriteTime >= writeCooldown || ignoreLocalTime)
                {
                    PlayerPrefs.SetString("SaveData", str);
                    string json = MessagePackSerializer.SerializeToJson(this, MessagePackSerializer.DefaultOptions);
                    lastWriteTime = Time.time; // 更新写入的时间
                    Debugger.Log($"保存本地数据=={json}");
                }
            }

            if (!GameEntry.Net.IsConnectServer) return;
            if (writeCloud)
            {
                string json = MessagePackSerializer.SerializeToJson(this, MessagePackSerializer.DefaultOptions);
                Debugger.Log($"上传云端?{Time.time - lastUploadTime >= uploadCooldown || ignoreCloudTime}");
                Debugger.Log($"数据====>{json}");
                if (Time.time - lastUploadTime >= uploadCooldown || ignoreCloudTime)
                {
                    if (SaveAction != null) SaveAction.Dispose();
                    SaveAction = Observable.Interval(TimeSpan.FromSeconds(0.02f)).Subscribe(_ =>
                    {
                        Debugger.Log($"上传数据=={str}");
                        GameEntry.SDK.UploadGameData(UserId, str);
                        lastUploadTime = Time.time; // 更新上传的时间
                    });
                }
            }
        }

        #endregion


        #region Private方法

        private void InitializeWithDefaultData()
        {
            _playerRoleData = new PlayerRoleData();
            // 继续为其他属性初始化默认值
        }

        #endregion
    }
}