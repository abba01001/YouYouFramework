using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using FrameWork;
using UniRx;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// �ο����֣�ȫ���ɵ��
/// </summary>
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(HollowOutMask))]
public class FormHollow2 : UIFormBase
{
    [Header("ǿ�ƹۿ�ʱ��")] [SerializeField] float DelayTime;

    private Button button;

    public static void ShowDialog(string formName)
    {
        GameEntry.UI.OpenUIForm<FormHollow2>(formName);
    }

    protected override void Start()
    {
        base.Start();
        GetComponent<HollowOutMask>().IsAcross = false;

        button = GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            //�ر��Լ�
            Close();

            //������һ������
            GameEntry.Guide.NextGroup(GameEntry.Guide.CurrentState);
        });
    }

    protected override void OnShow()
    {
        base.OnShow();
        //ǿ����ҿ�һ���
        if (DelayTime > 0)
        {
            button.enabled = false;
            Observable.Timer(TimeSpan.FromSeconds(DelayTime)).Subscribe(_ => { button.enabled = true; });
        }
    }
}