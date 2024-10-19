static partial class Constants
{
#if TestMode
    public const string ResVersion = "0.0.1";
#else
    public const string ResVersion = "0.2.14";
#endif

    // 全局可变变量
    public static float MainRoleJumpHeight = 1.5f;//主角跳跃高度
    public static float MainRoleMoveSpeed = 4f;//玩家移动速度
    public static string UserID = string.Empty;
    public static bool IsLoadDataTable = false; 
#if UNITY_EDITOR
    public static bool IsLoadCouldData = true;
#else
    public static bool IsLoadCouldData = false;
#endif
    
    
    // 定义全局常量，常量值不可修改
    public const float GRAVITY = -9.81f;//重力
    public const string CASTLEPATH = "Assets/Game/Download/DunGenMap/Dungeon/Castle/Tiles/";
    public const string GRAVEYARDPATH = "Assets/Game/Download/DunGenMap/Dungeon/Graveyard/Tiles/";
    public const string ENCRYPTEDKEY = "ENCRYPTED:";

    public const float GroundCheckDistance = 0.1f; // 地面检测距离
    public const string SECURITYKEY = "3ZkPqF9hDjW8q2Z7";//钥匙
    public const int BLOCK_SIZE = 16; // AES块大小
    public const string EMPTYGAMEDATA = "EmptyGameData";
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
    
}
