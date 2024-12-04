using System.Collections;
using UnityEngine;
using System;


public class GridCharacterBase : CharacterBase
{
    private Coroutine moveCoroutine;
    private BattleGrid parentGrid;
    public float attackCd = 0.75f;
    public float timer = 0.75f;
    private Transform detection;
    public override void Awake()
    {
        base.Awake();
        detection = transform.Find("Detection");
        RangeDetection rangeDetection = GetComponentInChildren<RangeDetection>();
        rangeDetection.OnObjectEnterRange += HandleEnterRange;
        rangeDetection.OnObjectStayInRange += HandleStayInRange;
        rangeDetection.OnObjectExitRange += HandleExitRange;
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
    
    private void HandleEnterRange(Collider2D other)
    {
        GameUtil.LogError("Enemy entered range: " + other.name);
        // 你可以在这里处理进入范围时的逻辑
    }

    private void Update()
    {
        timer += Time.deltaTime;
    }

    private void HandleStayInRange(Collider2D other)
    {
        GameUtil.LogError("Enemy is staying within range: " + other.name);
        if (timer >= attackCd)
        {
            timer = 0;
            animator.Play("attack");
        }
        // 你可以在这里处理物体在范围内停留时的逻辑
    }

    private void HandleExitRange(Collider2D other)
    {
        GameUtil.LogError("Enemy exited range: " + other.name);
        // 你可以在这里处理物体离开范围时的逻辑
    }
    
}
