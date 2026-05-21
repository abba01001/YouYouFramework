#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
namespace GameScripts
{
    public class DataDebugTool : MonoBehaviour
    {
        private IDataManager DataService => GameEntry.Data;

        [PropertyOrder(1)]
        [BoxGroup("Time", LabelText = " 时间调试")]
        [HorizontalGroup("Time/Input", LabelWidth = 40), OnValueChanged(nameof(AutoApply))]
        public int Year, Month, Day, Hour, Minute;

        [BoxGroup("Time")]
        [HorizontalGroup("Time/Display")]
        [ReadOnly, ShowInInspector, LabelText("当前时间")]
        private string TimePreview => TimeUtils.ServerDateTime.ToString("yyyy-MM-dd HH:mm:ss");

        [HorizontalGroup("Time/Display", Width = 80), Button("恢复实时")]
        private void Restore()
        {
            // TimeSyncManager.Instance.SetFakeTime(0);
            OnEnable();
            Refresh();
        }


        [PropertyOrder(2)]
        [BoxGroup("Timestamp", LabelText = " 时间戳快速查询")]
        [HorizontalGroup("Timestamp/Query", LabelWidth = 45), OnValueChanged(nameof(UpdateTimestampPreview))]
        public long inputTimestamp;

        [HorizontalGroup("Timestamp/Query"), ReadOnly, HideLabel]
        public string timestampResult;


        [PropertyOrder(10)]
        [BoxGroup("Data", LabelText = " 数据编辑器")]
        [ShowInInspector]
        [ListDrawerSettings(
            ListElementLabelName = "TitleName",
            ShowIndexLabels = false,
            IsReadOnly = true,
            Expanded = true,
            DraggableItems = false,
            HideAddButton = true)]
        [Searchable]
        private List<DataModuleWrapper> _dataModules = new List<DataModuleWrapper>();

        [Serializable]
        public class DataModuleWrapper
        {
            [HideInInspector] public string TitleName;

            [HideReferenceObjectPicker] [ShowInInspector, InlineProperty, HideLabel]
            public object Data;

            public DataModuleWrapper(string name, object data)
            {
                TitleName = name;
                Data = data;
            }

            public override string ToString() => TitleName;
        }

        private void Refresh()
        {
            EditorUtility.SetDirty(this);
            ActiveEditorTracker.sharedTracker.ForceRebuild();
            // TypeEventSystem.Send<TriggerPopupOnHomeEvent>();
        }

        [PropertyOrder(100)]
        [BoxGroup("Ops")]
        [HorizontalGroup("Ops/Btns"), Button(ButtonSizes.Large, Name = "重置"), GUIColor(0.4f, 0.8f, 1f)]
        private void ResetData()
        {
            Refresh();
        }

        private async UniTask InitDataModules()
        {
            while(DataService == null)
            {
                await UniTask.Yield();
            }
            _dataModules.Clear();
            _dataModules.Add(new DataModuleWrapper("每日登录数据", DataService.PlayerRoleData));
        }

        private void OnEnable()
        {
            var now = TimeUtils.ServerDateTime;
            Year = now.Year;
            Month = now.Month;
            Day = now.Day;
            Hour = now.Hour;
            Minute = now.Minute;
            InitDataModules();
            UpdateTimestampPreview();
        }

        private void UpdateTimestampPreview()
        {
            if (inputTimestamp <= 0)
            {
                timestampResult = "<- 等待输入";
                return;
            }
        
            try
            {
                timestampResult = "-> " + TimeUtils.IntToDateTime(inputTimestamp).ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch
            {
                timestampResult = "-> 格式错误";
            }
        }

        private void AutoApply()
        {
            try
            {
                var target = new DateTime(Year, Month, Day, Hour, Minute, 0);
                // TimeSyncManager.Instance.SetFakeTime(TimeUtils.DateTimeToLong(target));
                // TypeEventSystem.Send<RefreshActivityTimer>();
            }
            catch
            {
            }
        }
    }
}
#endif