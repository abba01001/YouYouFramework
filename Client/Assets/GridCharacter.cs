using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Collections;
using UnityEngine;

public class GridCharacter : MonoBehaviour
{
    private Vector2Int currentPosition;
    private Coroutine moveCoroutine;

    public void MoveTo(Vector2Int targetPosition, Action onComplete)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine = StartCoroutine(MoveCoroutine(targetPosition, onComplete));
    }

    private IEnumerator MoveCoroutine(Vector2Int targetPosition, Action onComplete)
    {
        // 当前格子的局部位置
        Vector3 startPosition = transform.localPosition;
        Vector3 endPosition = BattleGridManager.Instance.GetGrid(targetPosition).transform.localPosition;
        float time = 0;
        float duration = 1f; // 移动时间

        while (time < duration)
        {
            time += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(startPosition, endPosition, time / duration);
            yield return null;
        }

        transform.localPosition = endPosition;
        currentPosition = targetPosition;

        onComplete?.Invoke();
    }
    
    
}
