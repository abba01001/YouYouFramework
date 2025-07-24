using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public abstract class WorkerBase : MonoBehaviour
{
    public TriggerDetector triggerDetector;
    public bool IsLiving { get; set; }
    public WorkerData WorkerData { get; set; }

    public virtual void Init(WorkerData data)
    {
        if (triggerDetector == null)
        {
            triggerDetector = gameObject.AddComponent<TriggerDetector>();
        }
        else
        {
            triggerDetector.UnsubscribeAll();
        }
        triggerDetector.OnEnter += HandleOnEnter;
        triggerDetector.OnStay += HandleOnStay;
        triggerDetector.OnExit += HandleOnExit;
        WorkerData = data;
        IsLiving = true;
    }
    
    public abstract void Tick(); // 替代 OnUpdate

    public virtual void HandleOnEnter(Collider other)
    {
    }

    public virtual void HandleOnStay(Collider other)
    {
    }

    public virtual void HandleOnExit(Collider other)
    {
    }
}
