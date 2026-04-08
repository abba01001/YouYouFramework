using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// ��Ҫ���ڽ����Ч�޷���Ⱦ��UI�ϵ�����
/// </summary>
[RequireComponent(typeof(Canvas))] //�ű�����
public class UIFormEffect : MonoBehaviour
{
    [Header("�Ƿ񴰿ڶ���")] [SerializeField] bool isAnim = false;

    [Header("UI��Ч����")] [SerializeField] public List<UIEffectGroup> UIEffectGroups = new List<UIEffectGroup>();

    [Header("��ʼ���ŵ���Ч")] [SerializeField]
    List<ParticleSystem> effectOnOpenPlay = new List<ParticleSystem>();


    void Start()
    {
        Canvas CurrCanvas = GetComponent<Canvas>();
        //����UI�㼶, ������Ч�㼶
        for (int i = 0; i < UIEffectGroups.Count; i++)
        {
            UIEffectGroup effectGroup = UIEffectGroups[i];
            effectGroup.Group.ForEach(x =>
            {
                x.SetEffectOrder(CurrCanvas.sortingOrder + effectGroup.Order);
                x.gameObject.SetLayer("UI");
            });
        }

        //ֹͣ��Ч��ʼ����, ����Ҫ����ע��
        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < particles.Length; i++)
            particles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        //�����趨�ĳ�ʼ��Ч
        effectOnOpenPlay.ForEach(x => x.Play());
    }

    private void OnEnable()
    {
#if UNITY_EDITOR
        transform.SetAsLastSibling();
#endif
        if (isAnim) AnimOpen();
    }

    public void AnimOpen()
    {
        transform.DoShowScale(0.3f, 1);
    }
}

/// <summary>
/// UI��Ч����
/// </summary>
[Serializable]
public class UIEffectGroup
{
    /// <summary>
    /// ����
    /// </summary>
    public ushort Order;

    /// <summary>
    /// ��Ч��
    /// </summary>
    public List<Transform> Group = new List<Transform>();
}