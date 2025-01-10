using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using YouYou;

public class MainPanel : MonoBehaviour
{
    [SerializeField] private Button YouLiBtn;

    private void Awake()
    {
        YouLiBtn.SetButtonClick(() =>
        {
            GameEntry.UI.OpenUIForm<FormYouLi>();
        });
    }
    
    private void OnEnable()
    {

    }
}