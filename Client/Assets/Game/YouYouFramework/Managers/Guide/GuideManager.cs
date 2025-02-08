using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity;
using System;
using YouYou;
using System.Threading.Tasks;
using UnityEngine.UI;


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

    public event Action<GuideState, GuideState> OnStateChange;

    /// <summary>
    /// 触发下一步
    /// </summary>
    public event Action OnNextOne;

    public GuideGroup GuideGroup;

    private bool IsGuiding = false;
    public void OnUpdate()
    {
        if (!GameEntry.Net.IsLoginGame) return;
        if (IsGuiding) return;
        foreach (var pair in GameEntry.DataTable.Sys_GuideDBModel.IdByDic)
        {
            if (pair.Value.GuideType == 1)
            {
                if (GameEntry.Data.PlayerRoleData.level >= pair.Value.ToLevelTrigger && !GameEntry.Data.PlayerRoleData.guideIds.Contains(pair.Value.GuideId))
                {
                    GameEntry.Event.Dispatch(Constants.EventName.TriggerDialogue,new DialogueModel()
                    {
                        dialogueId = pair.Value.DialogueId,
                        finishAction = () =>
                        {
                            if (pair.Value.CompleteEvent != String.Empty)
                            {
                                IGuideClass guideClass = ClassMapper.GetInstance(pair.Value.CompleteEvent);
                                guideClass?.PlayGuide(() =>
                                {
                                    FinishCurGuide(pair.Value.GuideId);
                                },pair.Value.DetailMethod);
                            }
                            else
                            {
                                FinishCurGuide(pair.Value.GuideId);
                            }
                            //Guide_NewUser1.Instance.FirstEntryMain(QuickFightBtn,MoreBtn);
                        }
                    });
                    IsGuiding = true;
                }
            }
        }
    }

    private void FinishCurGuide(int guidId)
    {
        IsGuiding = false;
        GameEntry.Data.PlayerRoleData.curGuide = guidId;
        GameEntry.Data.PlayerRoleData.guideIds.Add(guidId);
    }

    public bool OnStateEnter(GuideState state)
    {
        int index = (int) state;
        if (Main.MainEntry.ParamsSettings.GetGradeParamData("ActiveGuide") == 0) return false;
        if (CurrentState == state) return false;
        if (GameEntry.Data.PlayerRoleData.curGuide == index) return false;
        switch (state)
        {
            //只触发一次的引导
            case GuideState.FirstEntryMain:
                if (NextGuide != index) return false;
                break;

            //每次引导结束
            case GuideState.None:
                GameEntry.Guide.GuideGroup = null;
                break;
        }

        OnStateChange?.Invoke(CurrentState, state);
        CurrentState = state;
        return true;
    }

    public bool NextGroup(GuideState descGroup)
    {
        if (Main.MainEntry.ParamsSettings.GetGradeParamData("ActiveGuide") == 0) return false;
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

    public int NextGuide => GameEntry.Data.PlayerRoleData.curGuide + 1;


    /// <summary>
    /// 新手引导 完成1个模块 存档
    /// </summary>
    public void GuideCompleteOne(GuideState guideState)
    {
        int index = (int) guideState;
        //只能保存后面的引导
        if (index >= GameEntry.Data.PlayerRoleData.curGuide + 1)
        {
            IsGuiding = false;
            GameEntry.Data.PlayerRoleData.curGuide = index;
            GameEntry.Data.SaveData(true);
            GameEntry.LogError(LogCategory.Guide, "GuideCompleteOne:" + guideState.ToString() + "===" + guideState.ToInt());
        }
    }
}