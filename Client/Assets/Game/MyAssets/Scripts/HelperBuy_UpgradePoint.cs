using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelperBuy_UpgradePoint : MonoBehaviour
{
    public GameObject window;

    private void Start()
    {
        HelperSpawner helperSpawner = window.GetComponent<HelperSpawner>();
        GameObject helperPrefab = helperSpawner.helperPrefab;
        Transform helperSpawnPoint = helperSpawner.helperSpawnPoint;

        if (PlayerPrefs.HasKey(helperSpawner.srNo + "Helper"))
        {
            helperSpawner.helper = Instantiate(helperPrefab, helperSpawnPoint.position, helperSpawnPoint.rotation).GetComponent<Helper>();
        }
    }

    public void OpenWindow()
    {
        window.SetActive(true);
    }

    public void CloseWindow()
    {
        window.SetActive(false);
    }
}
