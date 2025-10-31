using UnityEngine;
using System;
using System.Collections.Generic;

public class TriggerDetector : MonoBehaviour
{
    [Header("检测设置")]
    public float detectionRadius = 5f;
    public float checkInterval = 0.1f;

    private float lastCheckTime;
    private readonly HashSet<Collider> enteredColliders = new();
    private readonly HashSet<Collider> currentFrame = new();
    private readonly List<Collider> toRemove = new();
    private readonly Collider[] overlapBuffer = new Collider[64];

    public event Action<Collider> OnEnter;
    public event Action<Collider> OnStay;
    public event Action<Collider> OnExit;

    private void Update()
    {
        if (Time.time - lastCheckTime < checkInterval)
            return;

        lastCheckTime = Time.time;
        currentFrame.Clear();

        int count = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, overlapBuffer);
        if (count >= overlapBuffer.Length)
            Debug.LogWarning("TriggerDetector: 检测对象数量超出缓存大小！");

        for (int i = 0; i < count; i++)
        {
            var col = overlapBuffer[i];
            if (col == null) continue;
            currentFrame.Add(col);

            if (enteredColliders.Add(col))
                OnEnter?.Invoke(col);
            else
                OnStay?.Invoke(col);
        }

        toRemove.Clear();
        foreach (var col in enteredColliders)
        {
            if (col == null || !currentFrame.Contains(col))
                toRemove.Add(col);
        }

        foreach (var col in toRemove)
        {
            enteredColliders.Remove(col);
            OnExit?.Invoke(col);
        }
    }

    public void UnsubscribeAll()
    {
        OnEnter = null;
        OnStay = null;
        OnExit = null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
#endif
}