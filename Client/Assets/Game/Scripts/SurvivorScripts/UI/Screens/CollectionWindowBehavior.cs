using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OctoberStudio;
using OctoberStudio.UI;
using OctoberStudio.Upgrades.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameScripts
{
    public class CollectionWindowBehavior : PanelBase
    {

        protected override void OnAwake()
        {
            base.OnAwake();
        }

        protected override void OnShow()
        {
            base.OnShow();
            // GameController.SaveManager.StageData.onSelectedStageChanged += InitStage;
        }

        protected override void OnHide()
        {
            base.OnHide();
            // GameController.SaveManager.StageData.onSelectedStageChanged -= InitStage;
        }
               
    }
}