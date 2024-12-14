using Cysharp.Threading.Tasks;
using Main;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// 预加载流程
    /// </summary>
    public class ProcedurePreload : ProcedureBase
    {
        /// <summary>
        /// 目标进度(实际进度)
        /// </summary>
        private float m_TargetProgress;
        /// <summary>
        /// 当前进度(模拟进度)
        /// </summary>
        private float m_CurrProgress;

        private bool ShowLoginPanel = false;
        internal override void OnEnter()
        {
            base.OnEnter();
            MainEntry.Instance.PreloadBegin();

            m_CurrProgress = 0;

            BeginTask();
            GameEntry.Event.AddEventListener(Constants.EventName.LoginSuccess, userdata => 
            {
                _ = OnLoginSuccess(userdata); // 忽略返回值，直接运行异步任务
            });

        }

        internal override void OnLeave()
        {
            base.OnLeave();
            GameEntry.Event.RemoveEventListener(Constants.EventName.LoginSuccess, userdata => 
            {
                _ = OnLoginSuccess(userdata); // 忽略返回值，直接运行异步任务
            });
        }

        internal override void OnUpdate()
        {
            base.OnUpdate();

            //模拟加载进度条
            if (m_CurrProgress < m_TargetProgress)
            {
                //根据实际情况调节速度, 加载已完成和未完成, 模拟进度增值速度分开计算!
                if (m_TargetProgress < 1)
                {
                    m_CurrProgress += Time.deltaTime * 0.5f;
                }
                else
                {
                    m_CurrProgress += Time.deltaTime * 0.8f;
                }
                m_CurrProgress = Mathf.Min(m_CurrProgress, m_TargetProgress);//这里是为了防止进度超过100%， 比如完成了显示102%
                MainEntry.Instance.PreloadUpdate(m_CurrProgress);
            }

            if (m_CurrProgress == 1 && !ShowLoginPanel)
            {
                ShowLoginPanel = true;
                MainEntry.Instance.PreloadComplete();

#if UNITY_EDITOR
                if (MainEntry.ParamsSettings.IsMapEditorMode)
                {
                    GameEntry.Procedure.ChangeState(ProcedureState.MapEditor);
                    return;
                }
#endif
                GameEntry.SDK.InitSqlConnect();
                GameEntry.UI.OpenUIForm<FormLogin>();
            }
        }

        private async UniTask OnLoginSuccess(object userdata)
        {
            await GameEntry.Net.ConnectServerAsync();
            GameEntry.UI.CloseUIForm<FormLogin>();
            if (GameEntry.Data.IsFirstLoginTime)
            {
                GameEntry.Data.InitPlayData();
                GameEntry.Data.IsFirstLoginTime = false;
            }
            GameEntry.SDK.InitTalkingData();
            GameEntry.Time.InitNetTime();
            GameEntry.Data.SaveData(true,true,true,true);
            GameEntry.Procedure.ChangeState(ProcedureState.Game);
        }

        private void OnConnectServerSuccess(object userdata)
        {
            
        }

        
        /// <summary>
        /// 开始任务
        /// </summary>
        private void BeginTask()
        {
            TaskGroup taskGroup = GameEntry.Task.CreateTaskGroup();
#if ASSETBUNDLE
            //初始化资源信息
            taskGroup.AddTask((taskRoutine) =>
            {
                GameEntry.Loader.AssetInfo.InitAssetInfo(taskRoutine.Leave);
            });

            //加载自定义Shader
            taskGroup.AddTask((taskRoutine) =>
            {
                GameEntry.Loader.LoadAssetBundleAction(YFConstDefine.CusShadersAssetBundlePath, onComplete: (AssetBundle bundle) =>
                {
                    bundle.LoadAllAssets();
                    Shader.WarmupAllShaders();
                    taskRoutine.Leave();
                });
            });
#endif
            //加载Excel
            taskGroup.AddTask((taskRoutine) =>
            {
                GameEntry.DataTable.LoadDataAllTable(taskRoutine.Leave);
            });

            taskGroup.OnCompleteOne = () =>
            {
                m_TargetProgress = taskGroup.CurrCount / (float)taskGroup.TotalCount;
            };
            taskGroup.Run();
        }

    }
}