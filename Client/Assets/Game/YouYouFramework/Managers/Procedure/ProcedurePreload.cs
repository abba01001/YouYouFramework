using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Main;
using UnityEngine;
using Object = UnityEngine.Object;

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
        private bool m_loadFinish = false;
        internal override void OnEnter()
        {
            base.OnEnter();
            MainEntry.Instance.PreloadBegin();

            m_CurrProgress = 0;
            m_loadFinish = false;
            BeginTask();
            CheckHasRelogin();
            GameEntry.SDK.InitTalkingData();
            GameEntry.Event.AddEventListener(Constants.EventName.LoginSuccess, userdata => 
            {
                _ = OnLoginSuccess(userdata); // 忽略返回值，直接运行异步任务
            });

        }

        //是否重登
        private void CheckHasRelogin()
        {
            if (Constants.HasLoadAllAsset)
            {
                
            }
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
            
            if (m_CurrProgress == 1 && !m_loadFinish)
            {
                Constants.HasLoadAllAsset = true;
                m_loadFinish = true;
                MainEntry.Instance.PreloadComplete();
                GameEntry.UI.OpenUIForm<FormLogin>();
            }
        }

        private async UniTask OnLoginSuccess(object userdata)
        {
            GameUtil.LogError("44444444");
            GameEntry.UI.CloseUIForm<FormLogin>();
            GameUtil.LogError("555555555");
            
            if (GameEntry.Data.IsFirstLoginTime) GameEntry.Data.IsFirstLoginTime = false;
            GameEntry.Audio.InitVolume();
            GameUtil.LogError("6666666666");
            
            GameEntry.Data.SaveData(true,true,true,true);
            GameEntry.Procedure.ChangeState(ProcedureState.Game);
            GameUtil.LogError("777777777777");
            
            //
            // if (Application.platform != RuntimePlatform.OSXEditor &&
            //     Application.platform != RuntimePlatform.WindowsEditor)
            // {
            //     TalkingDataProfile profile = TalkingDataProfile.CreateProfile();
            //     profile.SetName($"{GameEntry.Data.PlayerRoleData.name}");
            //     profile.SetType(TalkingDataProfileType.WEIXIN);
            //     profile.SetGender(TalkingDataGender.MALE);
            //     profile.SetAge(18);
            //     profile.SetProperty1("value1");
            //     profile.SetProperty2("value2");
            //     profile.SetProperty3("value3");
            //     profile.SetProperty4("value4");
            //     profile.SetProperty5("value5");
            //     profile.SetProperty6(0.01);
            //     profile.SetProperty7(99.8);
            //     profile.SetProperty8(100);
            //     profile.SetProperty9(10000);
            //     profile.SetProperty10(100000000000L);
            //     Dictionary<string, object> eventValue = new Dictionary<string, object>
            //     {
            //         {"key01", "value01"},
            //         {"key02", 0.1}
            //     };
            //     TalkingDataSDK.OnLogin($"{GameEntry.Data.UserId}", profile, eventValue);
            // }
        }

        private void OnConnectServerSuccess(object userdata)
        {
            
        }

        
        /// <summary>
        /// 开始任务
        /// </summary>
        private void BeginTask()
        {
            if (Constants.HasLoadAllAsset)
            {
                m_TargetProgress = 1;
                return;
            }
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
                    // Shader.WarmupAllShaders();
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
