using System;
using System.Collections;
using System.Collections.Generic;
using FrameWork;
using Main;
using UnityEngine;

namespace FrameWork
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
            TaskGroup.AddTask(taskRoutine => { onEnter?.Invoke(); });
        }

        public void AddGuide(GuideRoutine guideRoutine)
        {
            TaskGroup.AddTask(taskRoutine =>
            {
                taskRoutine.OnCompleteStack.Push(() => { guideRoutine.OnExit?.Invoke(); });
                guideRoutine.OnEnter?.Invoke();
            });
        }

        public void Run(Action onComplete = null)
        {
            TaskGroup.OnComplete = () =>
            {
                onComplete?.Invoke();
                Debugger.Log("GroupComplete:" + GameEntry.Guide.CurrentState);
                GameEntry.Guide.OnStateEnter(GuideState.None);
            };
            TaskGroup.Run();
        }
    }
}