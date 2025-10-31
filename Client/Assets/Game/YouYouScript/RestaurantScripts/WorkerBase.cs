using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using YouYou;

public abstract class WorkerBase : MonoBehaviour
{
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public enum WorkerState
    {
        None,
        Idle,
        GoToProduceBuilding,
        GoToShelfBuilding
    }

    public WorkerState currentState = WorkerState.None;
    public WorkerState CurrentState
    {
        get
        {
            return currentState;
        }
        set
        {
            if(currentState != value) isStateChanged = true;
            currentState = value;
        }
    }
    public NavMeshAgent agent;
    public bool isStateChanged;
    public List<Food> collectedFood = new List<Food>();
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
        collectedFood.Clear();
        agent = GetComponent<NavMeshAgent>();
        IsLiving = true;
        currentState = WorkerState.Idle;
        isStateChanged = true;
        CheckStartWork();
    }
    
    public abstract void Tick(); // 替代 OnUpdate


    public void AddCollectFood(Food food)
    {
        collectedFood.Add(food);
    }
    
    public void ClearCollectFood(Food food)
    {
        collectedFood.Remove(food);
    }
    
    private async UniTask CheckStartWork()
    {
        while (!BuildingSystem.Instance.InitFinish)
        {
            await UniTask.Yield();
        }
        StartWork();
    }
    public virtual void StartWork()
    {
        
    }
    
    public virtual void HandleOnEnter(Collider other)
    {
    }

    public virtual void HandleOnStay(Collider other)
    {
    }

    public virtual void HandleOnExit(Collider other)
    {
    }
    
        
    public async UniTask MoveToDestination(Vector3 pos,Action action)
    {
        SetDestinationWithOffset(pos);
        // 等待直到目标到达
        while (agent.remainingDistance == 0)
        {
            await UniTask.Yield(); // 等待下一帧
        }
        while (agent.remainingDistance > agent.stoppingDistance)
        {
            await UniTask.Yield(); // 等待下一帧
        }
        // while (Vector3.Distance(agent.transform.position, pos) > 5)
        // {
        //     await UniTask.Yield(); // 等待下一帧
        // }
        action?.Invoke();
    }

    public void SetDestinationWithOffset(Vector3 targetPosition)
    {
        // 检查目标位置是否在NavMesh上
        NavMeshHit hit;
        // 尝试获取目标位置的有效 NavMesh 点
        if (NavMesh.SamplePosition(targetPosition, out hit, 1f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);  // 设置目标位置为有效的 NavMesh 点
            Debug.Log("目标位置有效，设置目标");
        }
        else
        {
            Debug.LogWarning("目标位置不在NavMesh上，开始逐步偏移");

            // 如果目标位置不在 NavMesh 上，逐步偏移目标
            Vector3 adjustedPosition = targetPosition;
            float offsetAmount = 0.1f; // 每次偏移的距离
            int maxAttempts = 200; // 最大尝试次数，避免死循环
            int attempts = 0;
            while (attempts < maxAttempts)
            {
                // 偏移目标位置
                adjustedPosition += new Vector3(GameUtil.RandomRange(-offsetAmount, offsetAmount), 0, GameUtil.RandomRange(-offsetAmount, offsetAmount));

                // 检查新的偏移位置是否在 NavMesh 上
                if (NavMesh.SamplePosition(adjustedPosition, out hit, 1f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position); // 设置为找到的有效 NavMesh 点
                    Debug.Log($"目标位置已调整，设置目标：{hit.position}");
                    return; // 找到有效位置后退出
                }

                attempts++;  // 增加尝试次数
            }
            // 如果超过最大尝试次数，仍然没有找到有效的 NavMesh 点
            Debug.LogWarning("无法找到有效的 NavMesh 位置");
        }
    }
}
