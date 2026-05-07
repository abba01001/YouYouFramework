using System.Collections.Generic;
using Protocols;

namespace GameScripts
{
    static partial class Constants
    {

        // 全局可变变量
        public static bool HasLoadAllAsset = false; //已经加载过资源
        public static bool IsLoadDataTable = false;
        public static bool IsLoginGame = false;
        public static bool IsEntryGame = false;
        public static bool IsShieldGuide = true;
        public static bool IsEntryFormMain = false;

        

        // 定义全局常量，常量值不可修改

        public const float GRAVITY = -9.81f;//重力
        public const string CASTLEPATH = "Assets/Game/Download/DunGenMap/Dungeon/Castle/Tiles/";
        public const string GRAVEYARDPATH = "Assets/Game/Download/DunGenMap/Dungeon/Graveyard/Tiles/";
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

        

        public static readonly char[] FirstSeparator = ",".ToCharArray();
        public static readonly char[] SecondSeparator = ";".ToCharArray();
        public static readonly char[] ThirdSeparator = "#".ToCharArray();

        public struct EventName
        {
            public const string PropsChangedEvent = "PropsChangedEvent";
            
            public const string SetKilledEnemiesCount = "SetKilledEnemiesCount";
            public const string SetExperienceProgress = "SetExperienceProgress";
            public const string SetExperienceLevel = "SetExperienceLevel";
            public const string RefreshEnemiesDiedCounter = "RefreshEnemiesDiedCounter";
            public const string ShowChestWindow = "ShowChestWindow";
            public const string ShowAbilitiesPanel = "ShowAbilitiesPanel";
            public const string TestEvent = "TestEvent";

            public const string GameEntryOnUpdate = "GameEntryOnUpdate";

            public const string GameEntryOnApplicationQuit = "GameEntryOnApplicationQuit";

            public const string StartNewRound = "StartNewRound";

            public const string RefreshGameTimer = "RefreshGameTimer";

            public const string GameEntryOnApplicationPause = "GameEntryOnApplicationPause";

            public const string LoadingSceneUpdate = "LoadingSceneUpdate";
            public const string LoadingSceneComplete = "LoadingSceneComplete";

            public const string PopupAction = "PopupAction";

            public const string ViewAction = "ViewAction";

            public const string EventMessage = "EventMessage";

            public const string UpdatePlayerPos = "UpdatePlayerPos";

            public const string LoginSuccess = "LoginSuccess";

    

            public const string FinishInputName = "FinishInputName";

            public const string FirstCustomer = "FirstCustomer";

            public const string TriggerGuideEvent = "TriggerGuideEvent";

            public const string TriggerDialogue = "TriggerDialogue";

            public const string InitBattleData = "InitBattleData";

            public const string UpdateBattleTimer = "UpdateBattleTimer";

            public const string UpdateBattleRound = "UpdateBattleRound";

            public const string UpdateEnemyCount = "UpdateEnemyCount";

    

            public const string SpawnCustomer = "SpawnCustomer";

    

            public const string UpdateBuildingsObj = "UpdateBuildingsObj";

            public const string UpdateBuildingSpend = "UpdateBuildingSpend";

            public const string SetMoneyText = "SetMoneyText";

            public const string UpdateFoodPlayerCarry = "UpdateFoodPlayerCarry";

            public const string GetSuspendReward = "GetSuspendReward";

            public const string UpdateChatText = "UpdateChatText";

            public const string UpdateBtnUnlockStatus = "UpdateBtnUnlockStatus";
        }

    

    

        public struct ItemPath
        {

        }
    }
}