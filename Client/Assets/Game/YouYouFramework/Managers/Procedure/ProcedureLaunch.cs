using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace YouYou
{
    /// <summary>
    /// 启动流程
    /// </summary>
    public class ProcedureLaunch : ProcedureBase
    {
        private string[] permissions = new string[]
        {
            "android.permission.WRITE_EXTERNAL_STORAGE"
        };
        internal override void OnEnter()
        {
            base.OnEnter();
            //初始画质设置
            GameEntry.Quality.SetQuality((QualityManager.Quality) 2);//GameEntry.Player.GetInt(Constants.StorgeKey.QualityLevel));
            //GameEntry.Quality.SetScreen((QualityManager.ScreenLevel)GameEntry.Player.GetInt(Constants.StorgeKey.Screen));
            GameEntry.Quality.SetFrameRate((QualityManager.FrameRate)4);//GameEntry.Player.GetInt(Constants.StorgeKey.FrameRate));

            //获取安卓权限
            permissions.ToList().ForEach(s =>
            {
                //if (!Permission.HasUserAuthorizedPermission(s)) Permission.RequestUserPermission(s);
            });
#if EDITORLOAD
            GameEntry.Procedure.ChangeState(ProcedureState.Preload);
#elif ASSETBUNDLE
            GameEntry.Procedure.ChangeState(ProcedureState.CheckVersion);
#endif
        }
    }
}