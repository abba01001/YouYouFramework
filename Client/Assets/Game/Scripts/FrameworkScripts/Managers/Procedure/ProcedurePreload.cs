using System;
using Cysharp.Threading.Tasks;
using Main;
using MessagePack;
using UnityEngine;

namespace GameScripts
{
    public class ProcedurePreload : ProcedureBase
    {
        public override ProcedureState StateType => ProcedureState.Preload;

        public override void OnEnter()
        {
            base.OnEnter();
            GameEntry.Event.AddEventListener(Constants.EventName.LoginSuccess, userdata =>
            {
                _ = OnLoginSuccess(userdata); // 忽略返回值，直接运行异步任务
            });
            _ = BeginLoadDataTables();
        }
    
    
        public override void OnLeave()
        {
            base.OnLeave();
            GameEntry.Event.RemoveEventListener(Constants.EventName.LoginSuccess, userdata =>
            {
                _ = OnLoginSuccess(userdata); // 忽略返回值，直接运行异步任务
            });
        }
    
        private async UniTask OnLoginSuccess(object userdata)
        {
            string savedStr = PlayerPrefs.GetString("SaveData", "");
            if (!string.IsNullOrEmpty(savedStr))
            {
                byte[] binaryData = Convert.FromBase64String(savedStr);
                float sizeInKb = binaryData.Length / 1024f;
                
                var data = MessagePackSerializer.Deserialize<DataManager>(binaryData);
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
                Debugger.Log($"<color=cyan>存档数据详情:\n{json}</color>");
                
                Debugger.Log($"<color=yellow>读取本地存档成功，大小: {sizeInKb:F2} KB</color>");
                GameEntry.Data.InitGameData(binaryData);
            }
            
            if (GameEntry.Data.IsFirstLoginTime) GameEntry.Data.IsFirstLoginTime = false;
            GameEntry.Instance.StartAutoSave().Forget();
            GameEntry.Procedure.ChangeState(ProcedureState.Main);
            GameEntry.UI.CloseUIForm<FormLogin>();
        }
    
    
        //开始加载表格
        private async UniTaskVoid BeginLoadDataTables()
        {
            FormLoading.Instance.Show();
            FormLoading.Instance.UpdateProgress("配置加载中",0f);
            var realProgress = new Progress<float>(value =>
            {
                FormLoading.Instance.UpdateProgress("配置加载中",value);
            });
            await GameEntry.Config.LoadDataTableAsync(realProgress);
            await GameEntry.UI.OpenUIForm<FormMask>();
            await GameEntry.UI.OpenUIForm<FormLogin>();
            FormLoading.Instance.Hide();
        }
    }
}