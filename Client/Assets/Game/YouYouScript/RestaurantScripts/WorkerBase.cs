using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public abstract class WorkerBase : MonoBehaviour
{
    public bool IsLiving { get; set; }
    public WorkerData WorkerData { get; set; }

    public virtual void Init(WorkerData data)
    {
        WorkerData = data;
        IsLiving = true;
    }
    
    public abstract void Tick(); // 替代 OnUpdate

}
