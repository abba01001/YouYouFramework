using GameScripts;

namespace GameScripts
{
    // Sys_Guide Entity
    public partial class Sys_GuideEntity : DataTableEntityBase
    {
        // 编号
        public int Id;
        // 引导ID
        public int GuideId;
        // 引导类型(1对话)(2点击)(3点击+对话)(4弹窗)
        public int GuideType;
        // 通过事件进行触发
        public string EventTrigger;
        // 达到X级触发(-1不是)
        public int ToLevelTrigger;
        // 下一个引导id
        public int NextGuideId;
        // 点击遮罩组件
        public string ClickWidth;
        // X时间后关闭遮罩
        public float TimeToClose;
        // 弹出界面
        public string ShowForm;
        // 点击是否需要箭头
        public string ClickArrow;
        // 触发场景(1,Launch 2,CheckVersion  3,Preload  4,Game  5,Battle  6,MapEditor)
        public int TriggerScene;
        // 触发对话ID(-1不是)
        public int DialogueId;
        // 策划备注全部任务流程
        public string Progress;
        // 引导是否启用
        public int IsEnable;
    }
}