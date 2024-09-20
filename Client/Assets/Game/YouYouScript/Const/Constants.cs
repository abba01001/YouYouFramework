static partial class Constants
{
#if TestMode
    public const string ResVersion = "0.0.1";
#else
    public const string ResVersion = "0.2.14";
#endif

    // 全局可变变量
    public static float MainRoleJumpHeight = 1.0f;//主角跳跃高度
    public static float MainRoleMoveSpeed = 2f;
    
    // 定义全局常量，常量值不可修改
    public const float GRAVITY = -9.81f;
    public const float GroundCheckDistance = 0.1f; // 地面检测距离
    
    // 定义全局的 readonly 变量，可以在运行时初始化，但之后不能修改
    
}
