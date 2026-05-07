using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity;
using System;

using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameScripts;
using Main;
using UniRx;
using UnityEngine.UI;

namespace GameScripts
{
    public enum GuideState
    {
        /// <summary>
        /// 未触发引导
        /// </summary>
        None,
    
        /// <summary>
        /// 第一关,局外
        /// </summary>
        FirstEntryMain,
    
        /// <summary>
        /// 结束
        /// </summary>
        Finish
    }
    
    /// <summary>
    /// 新手引导管理器
    /// </summary>
    public class GuideManager
    {
        public GuideState CurrentState { get; private set; } //当前处于哪个状态
    
    
        /// <summary>
        /// 触发下一步
        /// </summary>
        public event Action OnNextOne;
    
        public GuideGroup GuideGroup;
        private readonly Queue<string> TriggerEventQueue = new Queue<string>();
        private string CurTriggerEvent = string.Empty;
        private bool IsGuiding = false;
    
        public void Init()
        {
            GameEntry.Event.AddEventListener(Constants.EventName.TriggerGuideEvent, OnTriggerGuideEvent);
        }
    
        private void OnTriggerGuideEvent(object user_data)
        {
            string e = user_data as string;
            if (!GameEntry.Data.PlayerRoleData.guideEvent.Contains(e))
            {
                TriggerEventQueue.Enqueue(e);
            }
        }
    
        public void OnUpdate()
        {
            if (Constants.IsShieldGuide) return; //屏蔽引导系统
            if (!Constants.IsEntryGame) return;
            if (IsGuiding) return;
            if (TriggerEventQueue.Count > 0 && CurTriggerEvent == String.Empty)
            {
                CurTriggerEvent = TriggerEventQueue.Dequeue();
            }
    
            foreach (var pair in GameEntry.DataTable.Sys_GuideDBModel.IdByDic)
            {
                HandleGuideDetail(pair.Value);
                if (IsGuiding) break;
            }
        }
    
        private bool CheckCompleteGuideId(int guideId)
        {
            return GameEntry.Data.PlayerRoleData.guideIds.Contains(guideId);
        }
    
        private bool CheckCompletePreGuide(int guideId)
        {
            foreach (var pair in GameEntry.DataTable.Sys_GuideDBModel.IdByDic)
            {
                if (guideId == pair.Value.NextGuideId)
                {
                    return CheckCompleteGuideId(pair.Value.GuideId);
                }
            }
    
            return true;
        }
    
        private bool CheckGuideIsEnable(Sys_GuideEntity entity)
        {
            return entity.IsEnable == 1;
        }
    
        private async UniTask HandleGuideDetail(Sys_GuideEntity entity)
        {
            bool isEventGuide = !string.IsNullOrEmpty(entity.EventTrigger);
            // 验证引导是否可以继续
            if (!CheckGuideIsEnable(entity)) return; //引导是否启用
            if (!CheckInTriggerScene(entity)) return; //是否在触发场景里
            if (!Constants.IsEntryFormMain) return; //是否进入主界面
            // if (GameEntry.Data.RoleLevel < entity.ToLevelTrigger) return; // 未达到等级
            if (isEventGuide && CurTriggerEvent != entity.EventTrigger) return; // 事件未触发
            if (CheckCompleteGuideId(entity.GuideId)) return; // 引导已完成
            if (!CheckCompletePreGuide(entity.GuideId)) return; // 前置引导未完成
            IsGuiding = true;
    
            // 根据引导类型执行相应操作
            switch (entity.GuideType)
            {
                case 1: //对话引导
                    PlayDialogueGuide(entity);
                    break;
                case 2: //点击引导
                    PlayClickGuide(entity);
                    break;
                case 3: //对话+点击引导
                    PlayClickDialogueGuide(entity);
                    break;
                case 4: //弹窗引导
                    PlayFormGuide(entity);
                    break;
                default:
                    break;
            }
        }
    
    
    
        private bool CheckInTriggerScene(Sys_GuideEntity entity)
        {
            if (entity.TriggerScene == 0) return true;
            if (entity.TriggerScene == (int)GameEntry.Procedure.CurrProcedureState) return true;
            return false;
        }
    
        private void PlayFormGuide(Sys_GuideEntity entity)
        {
            if (entity.ShowForm != "")
            {
                GameEntry.UI.OpenUIFormByName(entity.ShowForm, null, () => { FinishCurGuide(entity.GuideId); });
            }
        }
    
        private void PlayClickDialogueGuide(Sys_GuideEntity entity)
        {
            GameEntry.Event.Dispatch(Constants.EventName.TriggerDialogue, new DialogueModel()
            {
                dialogueId = entity.DialogueId,
                delay = 0.5f
            });
            PlayClickGuide(entity);
        }
    
        private void PlayDialogueGuide(Sys_GuideEntity entity)
        {
            GameEntry.Event.Dispatch(Constants.EventName.TriggerDialogue, new DialogueModel()
            {
                dialogueId = entity.DialogueId,
                finishAction = () => { FinishCurGuide(entity.GuideId); }
            });
        }
    
        private async Task PlayClickGuide(Sys_GuideEntity entity)
        {
            bool hasWidth = false;
            int maxRetries = 30; // 最大尝试次数
            int retries = 0;
            GameObject width = GameUtil.FindObjectByPath(GameEntry.Instance.transform, entity.ClickWidth);
            while (width == null && retries < maxRetries)
            {
                await UniTask.Delay(500);
                width = GameUtil.FindObjectByPath(GameEntry.Instance.transform, entity.ClickWidth);
                retries++;
            }
    
            if (!width.activeInHierarchy)
            {
                await UniTask.Delay(500);
                Debugger.LogError($"挂起引导状态，等待{width.transform.name}激活");
            }
    
            GameEntry.Guide.GuideGroup = new GuideGroup();
            GuideRoutine guideRoutine = null;
            guideRoutine = new GuideRoutine();
            guideRoutine.GuideName = "第一步";
            guideRoutine.OnEnter = () =>
            {
                FormMask.Instance.ShowCircleMsak(width, () => { CheckShowClickArrow(entity.ClickArrow, width); });
    
                if (entity.TimeToClose != 0)
                {
                    Observable.Timer(TimeSpan.FromSeconds(entity.TimeToClose)).Subscribe(_ =>
                    {
                        GuideUtil.CheckDirectNext();
                    });
                }
                else
                {
                    GuideUtil.CheckBtnNext(width);
                }
            };
            guideRoutine.OnExit = () => { FormMask.Instance.CloseCircleMask(); };
            GuideGroup.AddGuide(guideRoutine);
            GuideGroup.Run(() => { FinishCurGuide(entity.GuideId); });
        }
    
        private void CheckShowClickArrow(string direction, GameObject obj)
        {
            if (direction == "down")
            {
                FormMask.Instance.ShowArrow(obj, ArrowDirection.down);
            }
        }
    
        private void FinishCurGuide(int guidId)
        {
            GameEntry.Data.PlayerRoleData.curGuide = guidId;
            GameEntry.Data.PlayerRoleData.guideIds.Add(guidId);
            Observable.Timer(TimeSpan.FromSeconds(0.1F)).Subscribe(_ =>
            {
                IsGuiding = false;
                if (CurTriggerEvent != string.Empty)
                {
                    GameEntry.Data.PlayerRoleData.guideEvent.Add(CurTriggerEvent);
                    CurTriggerEvent = String.Empty;
                }
    
                GameEntry.Data.SaveData(true);
            });
        }
    
        public bool OnStateEnter(GuideState state)
        {
            int index = (int)state;
            if (GameEntry.ParamsSettings.GetGradeParamData("ActiveGuide") == 0) return false;
            if (CurrentState == state) return false;
            if (GameEntry.Data.PlayerRoleData.curGuide == index) return false;
            switch (state)
            {
                //每次引导结束
                case GuideState.None:
                    GameEntry.Guide.GuideGroup = null;
                    break;
            }
    
            CurrentState = state;
            return true;
        }
    
        public bool NextGroup(GuideState descGroup)
        {
            if (GameEntry.ParamsSettings.GetGradeParamData("ActiveGuide") == 0) return false;
            if (CurrentState != descGroup) return false;
    
            //完成当前任务
            if (OnNextOne != null)
            {
                Action onNextOne = OnNextOne;
                OnNextOne = null;
                onNextOne();
            }
    
            GuideGroup.TaskGroup.LeaveCurrTask();
            return true;
        }
    }
}