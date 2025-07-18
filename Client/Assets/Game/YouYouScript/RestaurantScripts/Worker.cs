using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class Worker : MonoBehaviour
{
    public bool IsActive { get; set; }
    public WorkerData WorkerData { get; set; }
    public virtual void OnUpdate()
    {
        
    }

    public virtual void Init(WorkerData data)
    {
        WorkerData = data;
        IsActive = true;
    }
}
