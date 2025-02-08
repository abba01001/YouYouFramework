using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    public class GuideGroup
    {
        public TaskGroup TaskGroup { get; private set; }

        public GuideGroup()
        {
            TaskGroup = new TaskGroup();
        }

        public void AddGuide(Action onEnter)
        {
            TaskGroup.AddTask(taskRoutine =>
            {
                onEnter?.Invoke();
            });
        }

        public void AddGuide(GuideRoutine guideRoutine)
        {
            TaskGroup.AddTask(taskRoutine =>
            {
                taskRoutine.OnCompleteStack.Push(() =>
                {
                    GameUtil.LogError($"完成===={guideRoutine.GuideName}");
                    guideRoutine.OnExit?.Invoke();
                });
                guideRoutine.OnEnter?.Invoke();
                GameUtil.LogError($"开始===={guideRoutine.GuideName}");
            });
        }

        public void Run(Action onComplete = null)
        {
            TaskGroup.OnComplete = () =>
            {
                onComplete?.Invoke();
                GameEntry.Log(LogCategory.Guide, "GroupComplete:" + GameEntry.Guide.CurrentState);
                GameEntry.Guide.OnStateEnter(GuideState.None);
            };
            TaskGroup.Run();
        }
    }
}