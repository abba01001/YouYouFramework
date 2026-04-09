using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Watermelon
{
    [CustomOverlayElement("Ground locked/unlocked", "OnToggleValueChanged")]
    public class GroundTileComplexBehavior : AbstractComplexBehavior<GroundTileBehavior, PurchasePoint> 
    {
        private async UniTask Start()
        {
#if UNITY_EDITOR
            if(string.IsNullOrEmpty(ID))
            {
                Debug.LogError("Uninitialized ID", this);
            }
#endif

            PlayerBehavior playerBehavior = PlayerBehavior.GetBehavior();
            while (playerBehavior == null)
            {
                if(playerBehavior != null) break;
                await UniTask.NextFrame();
                playerBehavior = PlayerBehavior.GetBehavior();
            }
            Init();
        }

        public void OnToggleValueChanged(bool enabled)
        {
            if (unlockable == null) return;

            if (unlockable.OpenedVisuals != null)
                unlockable.OpenedVisuals.SetActive(enabled);

            if ((!enabled && isOpenFromStart))
            {
                if (unlockable.OpenedVisuals != null)
                    unlockable.OpenedVisuals.SetActive(true);
            }

            if (unlockable.ClosedVisuals != null)
                unlockable.ClosedVisuals.SetActive(!enabled);
        }
    }
}