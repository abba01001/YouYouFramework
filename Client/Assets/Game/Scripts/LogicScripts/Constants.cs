using System.Collections.Generic;
using Protocols;

namespace GameScripts
{
    static partial class Constants
    {

        // 全局可变变量
        public static bool IsLoginGame = false;
        public static bool IsEntryGame = false;
        public static bool IsShieldGuide = true;
        public static bool IsEntryFormMain = false;

        

        // 定义全局常量，常量值不可修改

        public const string ENCRYPTEDKEY = "ENCRYPTED:";
        public const int ProtocalHeadLength = 41;
        public const int ProtocalTotalLength = 1024;
        public const int MapMaxLevelCount = 10;

        public const string ProvinceUrl = "https://api.live.bilibili.com/client/v1/Ip/getInfoNew";


        public const string TalkingDataAppid = "0F4749337D034F9B9F80E2B0DD31851D";
        public const float GroundCheckDistance = 0.1f; // 地面检测距离
        public const string SECURITYKEY = "3ZkPqF9hDjW8q2Z7";//钥匙
        public const int BLOCK_SIZE = 16; // AES块大小
        public const string REQUESTFAIL = "RequestFail";
        public const string GETREDPOINT = "GetRedPoint";
        public const string CLEARREDPOINT = "ClearRedPoint";

     
        public struct EventName
        {
            public const string FormMainChangePanelEvent = "FormMainChangePanelEvent";
            public const string LanguageChangedEvent = "LanguageChangedEvent";
            public const string SelectedStageEvent = "SelectedStageEvent";
            public const string SelectedCharacterEvent = "SelectedCharacterEvent";
            public const string CheckEnableJoystickEvent = "CheckEnableJoystickEvent";
            public const string PropsChangedEvent = "PropsChangedEvent";
            
            public const string SetExperienceProgress = "SetExperienceProgress";
            public const string SetExperienceLevel = "SetExperienceLevel";
            public const string RefreshKilledEnemiesCount = "RefreshEnemiesDiedCounter";
            

            public const string GameEntryOnApplicationPause = "GameEntryOnApplicationPause";

            public const string BattleSceneInitFinish = "BattleSceneInitFinish";

            public const string PopupAction = "PopupAction";
            public const string EventMessage = "EventMessage";
            public const string LoginSuccess = "LoginSuccess";
            public const string TriggerGuideEvent = "TriggerGuideEvent";
            public const string TriggerDialogue = "TriggerDialogue";
            public const string GetSuspendReward = "GetSuspendReward";
            public const string UpdateChatText = "UpdateChatText";
            public const string UpdateBtnUnlockStatus = "UpdateBtnUnlockStatus";
        }
    

        public struct AtlasNamePath
        {
            public const string PartsThumbnail = "Assets/Game/Download/LayerLabAsset/2D Art Maker/AMCasual Character/Demo/PartsThumbnail.spriteatlas";
        }

        public struct ItemPath
        {

        }
    }
}