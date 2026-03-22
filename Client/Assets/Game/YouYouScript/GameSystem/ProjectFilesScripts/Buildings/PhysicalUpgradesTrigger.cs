using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class PhysicalUpgradesTrigger : UpgradesTrigger
    {
        public override void ShowUpgradesPanel()
        {
            if (IsLocalTrigger)
            {
                GlobalUpgradesController.Instance.OpenUpgradesPage(upgrades);
            }
            else
            {
                GlobalUpgradesController.Instance.OpenMainUpgradesPage();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // processing player interaction with the trigger
            if (other.gameObject.layer == PhysicsHelper.LAYER_CHARACTER)
            {
                ShowUpgradesPanel();
            }
        }
    }
}
