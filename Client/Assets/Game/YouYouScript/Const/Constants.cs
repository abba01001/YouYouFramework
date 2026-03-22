using System.Collections.Generic;

static partial class Constants
{
#if TestMode
    public const string ResVersion = "0.0.1";
#else
    public const string ResVersion = "0.2.14";
#endif

    // 全局可变变量
    public static bool HasLoadAllAsset = false; //已经加载过资源
    public static float MainRoleJumpHeight = 1.5f;//主角跳跃高度
    public static float MainRoleMoveSpeed = 4f;//玩家移动速度
    public static bool IsLoadDataTable = false;
    public static bool IsLoginGame = false;
    public static bool IsEntryGame = true;
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
    
    // 定义全局的 readonly 变量，可以在运行时初始化，但之后不能修改
    public static readonly char[] FirstSeparator = ",".ToCharArray();
    public static readonly char[] SecondSeparator = ";".ToCharArray();
    public static readonly char[] ThirdSeparator = "#".ToCharArray();
    
    public struct StorgeKey
    {
        public const string MasterVolume = "MasterVolume";
        public const string BGMVolume = "BGMVolume";
        public const string AudioVolume = "AudioVolume";
        public const string GamePause = "GamePause";
        public const string FrameRate = "FrameRate";
        public const string Screen = "Screen";
        public const string QualityLevel = "QualityLevel";
    }

    public struct EventName
    {
        public const string TestEvent = "TestEvent";
        public const string GameEntryOnUpdate = "GameEntryOnUpdate";
        public const string GameEntryOnApplicationQuit = "GameEntryOnApplicationQuit";
        public const string StartNewRound = "StartNewRound";
        public const string RefreshGameTimer = "RefreshGameTimer";
        public const string GameEntryOnApplicationPause = "GameEntryOnApplicationPause";
        public const string LoadingSceneUpdate = "LoadingSceneUpdate";
        public const string PopupAction = "PopupAction";
        public const string ViewAction = "ViewAction";
        public const string EventMessage = "EventMessage";
        public const string UpdatePlayerPos = "UpdatePlayerPos";
        public const string FinishLoadDataTable = "FinishLoadDataTable";
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


        public const string EnergyChangedEvent = "EnergyChangedEvent";
        public const string ConsumeMatEvent = "ConsumeMatEvent";
    }


    public struct ItemPath
    {
        public const string CardObj = "Assets/Game/Download/Prefab/Item/Card.prefab";
        public const string ArrowLine = "Assets/Game/Download/Prefab/Item/ArrowLine.prefab";
        public const string GoldPanel = "Assets/Game/Download/Prefab/UI/Panel/GoldPanel.prefab";
        public const string GuildPanel = "Assets/Game/Download/Prefab/UI/Panel/GuildPanel.prefab";
        public const string HeroPanel = "Assets/Game/Download/Prefab/UI/Panel/HeroPanel.prefab";
        public const string LordPanel = "Assets/Game/Download/Prefab/UI/Panel/LordPanel.prefab";
        public const string RankPanel = "Assets/Game/Download/Prefab/UI/Panel/RankPanel.prefab";
        public const string MainPanel = "Assets/Game/Download/Prefab/UI/Panel/MainPanel.prefab";
        public const string ShopPanel = "Assets/Game/Download/Prefab/UI/Panel/ShopPanel.prefab";
    }

    public struct TexturePath
    {
        public const string HeroPanel = "Assets/Game/Download/Textures/HeroPanel";
    }
    
    public struct AtlasPath
    {
        public const string HeroPanel = "Assets/Game/Download/Textures/HeroPanel/HeroPanel.spriteatlas";
        public const string LordPanel = "Assets/Game/Download/Textures/LordPanel/LordPanel.spriteatlas";
        public const string Equip = "Assets/Game/Download/Textures/Equip/Equip.spriteatlas";
        public const string Daoju = "Assets/Game/Download/Textures/DaoJu/DaoJu.spriteatlas";
        public const string Game = "Assets/Game/Download/Textures/Game/Game.spriteatlas";
        public const string Common = "Assets/Game/Download/Textures/Common/Common.spriteatlas";
    }
    
    public struct ModelPath
    {
        public const string Path000001 = "Assets/Game/Download/Prefab/Model/Path000001.prefab";

        
        
        public const string Hero101 = "Assets/Game/Download/Prefab/Model/Hero101.prefab";
        public const string Hero102 = "Assets/Game/Download/Prefab/Model/Hero102.prefab";
        public const string Hero103 = "Assets/Game/Download/Prefab/Model/Hero103.prefab";
        public const string Hero104 = "Assets/Game/Download/Prefab/Model/Hero104.prefab";
        public const string Hero105 = "Assets/Game/Download/Prefab/Model/Hero105.prefab";
        
        public const string Enemy21001 = "Assets/Game/Download/Prefab/Model/Enemy21001.prefab";
    }
    
}
