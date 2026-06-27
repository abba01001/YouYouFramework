using System.Linq;
using UnityEngine.Android;

namespace GameScripts
{
    // 启动流程
    public class ProcedureLaunch : ProcedureBase
    {
        public override ProcedureState StateType => ProcedureState.Launch;
        private string[] permissions = new string[]
        {
            "android.permission.WRITE_EXTERNAL_STORAGE",
            "android.permission.POST_NOTIFICATIONS"
        };

        public override void OnEnter()
        {
            base.OnEnter();
            //获取安卓权限
            #if UNITY_ANDROID
            permissions.ToList().ForEach(s =>
            {
                if (!Permission.HasUserAuthorizedPermission(s)) Permission.RequestUserPermission(s);
            });
            #endif

            GameEntry.Procedure.ChangeState(ProcedureState.Preload);
        }
    }
}