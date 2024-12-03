using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Collections;
using UnityEngine;

public class GridCharacter : MonoBehaviour
{
    private Coroutine moveCoroutine;
    private Vector3 initScale;
    private Animator animator;
    private void Awake()
    {
        initScale = transform.localScale;
        animator = transform.GetComponentInChildren<Animator>();
    }

    public void RefreshParentGrid(BattleGrid grid)
    {
        this.transform.SetParent(grid.ModelRoot);
    }

    public void SetTargetPos(Vector2 endPosition)
    {
        transform.position = endPosition;
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
        transform.localScale = new Vector3(endPosition.x > startPosition.x ? -initScale.x : initScale.x, initScale.y, initScale.z);
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
    
    
}
