using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class UIAdaptive : MonoBehaviour
{
    private float Offset = 30;
    private RectTransform rectTransform;

    private void OnDestroy()
    {
        //GameEntry.Player.RemoveEventListener(Constants.StorgeKey.Screen, OnCurrScreen);
    }
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        Offset = -Offset * 2;
        //GameEntry.Player.AddEventListener(Constants.StorgeKey.Screen, OnCurrScreen);
        OnCurrScreen(null);
    }

    private void OnCurrScreen(object userData)
    {
        rectTransform.sizeDelta = new Vector2(GameEntry.UI.CurrScreen > GameEntry.UI.StandardScreen ? Offset : 0, 0);
    }
}
