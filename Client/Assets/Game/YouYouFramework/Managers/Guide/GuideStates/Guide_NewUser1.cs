using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace YouYou
{
    public class Guide_NewUser1 : Singleton<Guide_NewUser1>
    {
        private GuideGroup GuideGroup;

        //第一关 新手引导
        public void FirstEntryMain(Button button1,Button button2)
        {
            if (!GameEntry.Guide.OnStateEnter(GuideState.FirstEntryMain)) return;

            GameEntry.Guide.GuideGroup = GuideGroup = new GuideGroup();
            GuideRoutine guideRoutine = null;

            //第一步
            guideRoutine = new GuideRoutine();
            guideRoutine.GuideName = "第一步";
            guideRoutine.OnEnter = () =>
            {
                FormMask.ShowCircleMsak(button1.gameObject, () =>
                {
                    FormMask.ShowArrow(button1.gameObject, ArrowDirection.down);
                });
                GuideUtil.CheckBtnNext(button1);
                // GameEntry.Time.CreateTimer(this, 2f, () =>
                // {
                //     GuideUtil.CheckDirectNext();
                // });
            };
            guideRoutine.OnExit = () =>
            {
                //步骤结束时， 把镂空遮罩关了
                FormMask.CloseCircleMask();
            };
            GuideGroup.AddGuide(guideRoutine);
            //启动新手引导组（大步骤）
            
            //第二步
            guideRoutine = new GuideRoutine();
            guideRoutine.GuideName = "第二步";
            guideRoutine.OnEnter = () =>
            {
                FormMask.ShowCircleMsak(button2.gameObject);
                GameEntry.Time.CreateTimer(this, 2f, () =>
                {
                    GuideUtil.CheckDirectNext();
                });
            };
            guideRoutine.OnExit = () =>
            {
                //步骤结束时， 把镂空遮罩关了
                FormMask.CloseCircleMask();
            };
            guideRoutine.GuideKey = "TestKey";//名字可填可不填， 填了就可以用于外部映射
            GuideGroup.AddGuide(guideRoutine);

            // //第三步
            // GuideGroup.AddGuide(() =>
            // {
            //     //监听开关打开, 触发下一步
            //     Toggle toggle = null;
            //     GuideUtil.CheckToggleNext(toggle);
            // });
            //
            // //第四步
            // GuideGroup.AddGuide(() =>
            // {
            //     //监听事件 触发下一步
            //     GuideUtil.CheckEventNext("EventName");
            // });

            //启动新手引导组（大步骤）
            GuideGroup.Run(() =>
            {
                //多个小步骤全部做完了， 网络存档
                GameEntry.Guide.GuideCompleteOne(GuideState.FirstEntryMain);
            });
        }
    }
}