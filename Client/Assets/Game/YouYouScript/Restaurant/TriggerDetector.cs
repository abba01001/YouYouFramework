using UnityEngine;
using System;
using System.Collections.Generic;

public class TriggerDetector : MonoBehaviour
{
    public float detectionRadius = 5f;  // 检测范围
    public float checkInterval = 0.1f;  // 检查间隔
    private float lastCheckTime = 0f;
    private HashSet<Collider> enteredColliders = new HashSet<Collider>();  // 记录进入触发器的物体

    // 事件声明
    public event Action<Collider> OnEnter;
    public event Action<Collider> OnStay;
    public event Action<Collider> OnExit;

    private void Update()
    {
        if (Time.time - lastCheckTime >= checkInterval)
        {
            lastCheckTime = Time.time;

            // 获取当前范围内的所有碰撞体
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);

            // 记录需要移除的物体
            HashSet<Collider> toRemove = new HashSet<Collider>(enteredColliders);

            // 检查物体进入触发器
            foreach (var collider in colliders)
            {
                if (!enteredColliders.Contains(collider))
                {
                    enteredColliders.Add(collider);
                    OnEnter?.Invoke(collider);  // 触发进入事件
                }

                // 检查物体是否依然有效
                if (collider != null && collider.gameObject != null)
                {
                    OnStay?.Invoke(collider);  // 持续停留事件
                }
                else
                {
                    toRemove.Add(collider); // 将销毁的对象标记为需要移除
                }
            }
            
            // 检查物体是否离开触发器
            foreach (var collider in toRemove)
            {
                // 如果 collider 已被销毁或者无效，则跳过
                if (collider == null || collider.gameObject == null)
                {
                    enteredColliders.Remove(collider);  // 从已进入的物体中移除
                    continue;  // 直接跳过，不触发退出事件
                }

                if (Array.IndexOf(colliders, collider) == -1)  // 已经不在检测范围内
                {
                    enteredColliders.Remove(collider);
                    OnExit?.Invoke(collider);  // 触发退出事件
                }
            }
        }
    }

    // 清除绑定的事件
    public void UnsubscribeAll()
    {
        OnEnter = null;
        OnStay = null;
        OnExit = null;
    }
}