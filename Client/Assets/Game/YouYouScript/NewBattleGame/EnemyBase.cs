using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyBase : CharacterBase, IDamageable
{
    [HideInInspector] protected int healthValue = 20;
    [HideInInspector] protected int defenseValue = 0;
    [HideInInspector] public bool isAlive => healthValue > 0;

    public List<Transform> waypoints; // 存储路径点
    [HideInInspector] private float totalTime = 15f; // 总共走完路径所需的时间（单位秒）
    [HideInInspector] private float moveSpeed = 0f; // 计算出的每秒移动距离
    [HideInInspector] private int currentWaypointIndex = 0; // 当前目标路径点的索引
    private float totalPathLength = 0f; // 总路径长度
    private float speedFactor = 1f; // 速度因子（默认 1，增速时 > 1，减速时 < 1）

    private void Start()
    {
        modelDi.gameObject.SetActive(false);
        blood.gameObject.SetActive(false);
        defense.gameObject.SetActive(false);
        
        if (waypoints.Count > 0)
        {
            modelRoot.transform.localScale = Vector3.one * 2;
            // 计算路径总长度
            totalPathLength = CalculateTotalPathLength();
            moveSpeed = totalPathLength / totalTime; // 每秒需要移动的距离
            transform.position = waypoints[currentWaypointIndex].position;
            StartCoroutine(FollowPath());
        }
    }

    // 计算路径总长度
    private float CalculateTotalPathLength()
    {
        float length = 0f;
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            length += Vector3.Distance(waypoints[i].position, waypoints[i + 1].position);
        }

        // 如果是循环路径，最后一个点到第一个点的距离
        if (waypoints.Count > 1)
        {
            length += Vector3.Distance(waypoints[waypoints.Count - 1].position, waypoints[0].position);
        }

        return length;
    }

    // 沿路径移动
    private IEnumerator FollowPath()
    {
        SetFace(1);
        while (isAlive) // 只要敌人存活，持续执行
        {
            animator.SetBool("Run",true);
            // 获取目标路径点
            Transform targetWaypoint = waypoints[currentWaypointIndex];

            // 计算目标位置
            Vector3 direction = targetWaypoint.position - transform.position;
            direction.z = 0; // 2D 游戏中忽略 Z 轴
            float distance = direction.magnitude;
            direction.Normalize();

            // 计算每帧的移动距离（根据速度因子调整移动速度）
            float moveDistancePerFrame = moveSpeed * speedFactor * Time.deltaTime;
            if (distance <= moveDistancePerFrame)
            {
                transform.position = targetWaypoint.position;
            }
            else
            {
                transform.position += direction * moveDistancePerFrame;
            }

            if (Vector3.Distance(transform.position, targetWaypoint.position) <= 0.1f)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex is 3 or 4)
                {
                    SetFace(-1);
                }
                else
                {
                    SetFace(1);
                }
                if (currentWaypointIndex >= waypoints.Count)
                {
                    currentWaypointIndex = 0; // 如果到达最后一个路径点，重置到第一个路径点
                }
            }
            yield return null; // 等待下一帧
        }
    }

    // 设置敌人速度因子（增益/减益效果）
    public void SetSpeedFactor(float factor)
    {
        speedFactor = factor;
    }

    public void TakeNormalDamage(int damage)
    {
        // 减少血量
        healthValue -= damage;
        // 如果血量小于等于零，则死亡
        if (healthValue <= 0)
        {
            Die();
        }
        else
        {
            // 播放被攻击的动画
            //animator.SetTrigger("Hit");
        }

        // 更新血条（如果有的话）
        UpdateHealthBar();
    }

    public void TakeSkillDamage(int damage)
    {
        
    }

    private async UniTask Die()
    {
        // 播放死亡动画
        animator.SetBool("Die",true);
        // 进行死亡逻辑处理（如销毁敌人对象等）
        await UniTask.Delay(2000);
        transform.gameObject.SetActive(false);
    }

    private void UpdateHealthBar()
    {
        SetBloodValue((float) healthValue / 100f);
    }
}