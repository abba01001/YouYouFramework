using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class HelperBuy_UpgradePoint : TriggetBase
{
    private void Start()
    {
        // HelperSpawner helperSpawner = window.GetComponent<HelperSpawner>();
        // GameObject helperPrefab = helperSpawner.helperPrefab;
        // Transform helperSpawnPoint = helperSpawner.helperSpawnPoint;
        //
        // if (PlayerPrefs.HasKey(helperSpawner.srNo + "Helper"))
        // {
        //     helperSpawner.helper = Instantiate(helperPrefab, helperSpawnPoint.position, helperSpawnPoint.rotation).GetComponent<Helper>();
        // }
    }

    private bool isOpenging = false;
    private bool isCloseing = false;
    public void OpenWindow()
    {
        if(isOpenging) return;
        isOpenging = true;
        isCloseing = false;
        GameEntry.UI.OpenUIForm<FormUpgrade>();
    }

    public void CloseWindow()
    {
        if(isCloseing) return;
        isCloseing = true;
        isOpenging = false;
        GameEntry.UI.CloseUIForm<FormUpgrade>();
    }
}
