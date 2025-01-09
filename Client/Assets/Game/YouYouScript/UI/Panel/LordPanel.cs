using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using YouYou;

public class LordPanel : MonoBehaviour
{
    private void OnEnable()
    {
        if (GoldPanel.Instance != null)
        {
            GoldPanel.Instance.RefreshPos(ShowType.LordPanel);
        }
    }
}