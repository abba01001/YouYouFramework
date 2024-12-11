using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.VisualScripting;
using YouYou;


public class GridCharacterBase : CharacterBase
{
    private Coroutine moveCoroutine;
    private BattleGrid parentGrid;
    public float attackCd = 0.75f;
    public float timer = 0.75f;
    private Transform detection;
    public Sys_RoleAttrEntity config { get; set; }
    public void InitParams(Sys_RoleAttrEntity _config)
    {
        config = _config;
    }
    
    public override void Awake()
    {
        base.Awake();
        detection = transform.Find("Detection");
        RangeDetection rangeDetection = GetComponentInChildren<RangeDetection>();
        rangeDetection.OnObjectsEnterRange += HandleEnterRange;
        rangeDetection.OnObjectsStayInRange += HandleStayInRange;
        rangeDetection.OnObjectsExitRange += HandleExitRange;
    }

    public void PlayBornAnim()
    {
        animator.Play("born");
    }

    public void SetTargetPos(Vector2 endPosition)
    {
        transform.position = endPosition;
    }

    public void SetParentGrid(BattleGrid grid)
    {
        parentGrid = grid;
        detection.position = parentGrid.transform.position;
    }
    
    public void MoveTo(Vector2 targetPosition,Action onComplete)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = StartCoroutine(MoveCoroutine(targetPosition, onComplete));
    }

    private IEnumerator MoveCoroutine(Vector2 targetPosition, Action onComplete)
    {
        // 当前格子的局部位置
        Vector3 startPosition = transform.position;
        Vector3 endPosition = new Vector3(targetPosition.x, targetPosition.y, 0);
        SetFace(endPosition.x > startPosition.x ? -1 : 1);
        animator.SetBool("Run",true);
        float distance = Vector3.Distance(startPosition, endPosition);
        float moveSpeed = 81f;
        float duration = distance / moveSpeed;
        float time = 0f;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            transform.position = Vector3.Lerp(startPosition, endPosition, time / duration);
            yield return null;
        }

        animator.SetBool("Run",false);
        transform.position = endPosition;

        onComplete?.Invoke();
    }
    
    private void HandleEnterRange(List<Collider2D> colliders)
    {
        // 你可以在这里处理进入范围时的逻辑
    }

    private void Update()
    {
        timer += Time.deltaTime;
    }

    private void HandleStayInRange(List<Collider2D> colliders)
    {
        if (timer >= attackCd)
        {
            timer = 0;
            GameObject enemy = GetNearEnemy(colliders);
            if (enemy != null)
            {
                SetFace(transform.position.x > enemy.transform.position.x ? 1 : -1);
                enemy.GetComponentInParent<EnemyBase>().TakeNormalDamage(5);
                animator.Play("attack");
                //if(ModelPath == Constants.ModelPath.Hero101) GenerateTx(enemy.gameObject);
            }
        }
    }

    private GameObject GetNearEnemy(List<Collider2D> colliders)
    {
        float d1 = 999999999999;
        GameObject obj = null;
        foreach (var col in colliders)
        {
            var d2 = Mathf.Abs(Vector3.Distance(col.gameObject.transform.position, transform.position));
            if (d2 <= d1)
            {
                d1 = d2;
                obj = col.gameObject;
            }
        }
        return obj;
    }
    
    private async void GenerateTx(GameObject target)
    {
        await UniTask.Delay(300);
        // 从对象池中获取特效对象
        PoolObj tx = await GameEntry.Pool.GameObjectPool.SpawnAsync("Assets/Game/Download/Prefab/Effect/tx_MergerGame_01.prefab");
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = target.transform.position;
    
        // 保证Z轴一致
        targetPosition = new Vector3(targetPosition.x, targetPosition.y, startPosition.z);
        tx.transform.position = startPosition;

        // 飞行总时间
        float flightDuration = 1f;
        float elapsedTime = 0f;

        // 计算飞行的方向和距离
        Vector3 direction = targetPosition - startPosition;
        float distance = direction.magnitude;

        // 根据目标相对位置调整控制点
        Vector3 controlPoint;
        if (direction.x > 0) // 目标在右侧
        {
            controlPoint = startPosition + new Vector3(distance / 2, 150, 0); // 向上抛物线
        }
        else if (direction.x < 0) // 目标在左侧
        {
            controlPoint = startPosition + new Vector3(-distance / 2, 150, 0); // 向上抛物线
        }
        else if (direction.y < 0) // 目标在下方
        {
            controlPoint = startPosition + new Vector3(0, -150, 0); // 向下抛物线
        }
        else // 目标在上方
        {
            controlPoint = startPosition + new Vector3(0, 150, 0); // 向上抛物线
        }

        while (elapsedTime < flightDuration)
        {
            elapsedTime += Time.deltaTime;
            targetPosition = target.transform.position;
            float t = elapsedTime / flightDuration;
            Vector3 currentPos = BezierUtils.CalculateBezierPoint(t, startPosition, controlPoint, controlPoint, targetPosition);
            tx.transform.position = currentPos;
            await UniTask.Yield();
        }
        tx.transform.position = target.transform.position;
        GameEntry.Pool.GameObjectPool.Despawn(tx);
    }


    
    private void HandleExitRange(List<Collider2D> colliders)
    {
        // 你可以在这里处理物体离开范围时的逻辑
    }
    
}
