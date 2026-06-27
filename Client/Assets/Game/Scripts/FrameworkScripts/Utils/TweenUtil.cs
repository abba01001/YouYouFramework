using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace GameScripts
{
    public class TweenUtil
    {
        public static void SetPivotAndKeepPosition(RectTransform rect, Vector2 newPivot)
        {
            if (rect == null) return;
            Vector2 size = rect.rect.size;
            Vector2 deltaPivot = rect.pivot - newPivot;
            Vector3 deltaPosition = new Vector3(
                deltaPivot.x * size.x * rect.localScale.x,
                deltaPivot.y * size.y * rect.localScale.y,
                0);

            rect.pivot = newPivot;
            rect.localPosition -= deltaPosition;
        }

        // 多个物品弹入按钮动画
        public static async UniTask PlayMultiplyItemBounceAnim(Transform target)
        {
            int showCount = 20;
            int flyThreshold = 5; // 生成第 5 个时，第 0 个开始飞
            List<GameObject> spawnedItems = new List<GameObject>();
            Sequence flySeq = DOTween.Sequence();
            
            float interVal = 0.1f;
            float flyDuration = 0.5f;

            for (int i = 0; i < showCount; i++)
            {
                // 1. 生成与弹跳动画
                GameObject obj = await GameEntry.Pool.GameObjectPool.Spawn("Assets/Game/Download/Prefab/Item/FlyRewardItem_One.prefab");
                // obj.transform.SetParent(FormMain.Instance.transform);
                
                var rect = obj.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                SetPivotAndKeepPosition(rect, new Vector2(0.5f, 0.5f));
                
                // 随机散开位置逻辑
                Vector3 targetLocalPos = Vector3.zero;
                if (i != 0) {
                    float angleRad = (GameUtil.RandomRange(0f, 360f)) * Mathf.Deg2Rad;
                    float finalRadius = 50f + GameUtil.RandomRange(-15f, 15f);
                    targetLocalPos.x += Mathf.Cos(angleRad) * finalRadius;
                    targetLocalPos.y += Mathf.Sin(angleRad) * finalRadius;
                }
                rect.localPosition = targetLocalPos;
                rect.localScale = Vector3.zero;
                obj.SetActive(true);
                rect.DOScale(1.1f, 0.15f).SetEase(Ease.Linear).OnComplete(() => rect.DOScale(1.0f, 0.1f).SetEase(Ease.Linear));
                
                spawnedItems.Add(obj);

                // 2. 达到阈值或循环结束时，启动飞行
                // 当 i >= threshold 时，触发 i - threshold 的那个物体飞行
                if (i >= flyThreshold)
                {
                    int flyIndex = i - flyThreshold;
                    AddFlyAnimation(spawnedItems[flyIndex], target, flySeq, flyIndex, flyDuration);
                }

                await UniTask.Delay(100);
            }

            // 3. 处理循环结束后剩余的物体
            for (int i = showCount - flyThreshold; i < showCount; i++)
            {
                AddFlyAnimation(spawnedItems[i], target, flySeq, i, flyDuration);
            }

            // 等待所有飞行任务完成
            await UniTask.WaitUntil(() => flySeq.IsComplete());
        }

        private static void AddFlyAnimation(GameObject obj, Transform target, Sequence seq, int index, float duration)
        {
            float startFlyTime = index * 0.1f;
            Vector3 controlPos = obj.transform.position + new Vector3((target.position.x - obj.transform.position.x) * 0.4f, 200);
            Vector3[] bezierArray = BezierUtils.GetBeizerList(obj.transform.position, controlPos, target.position, 10);
            
            seq.Insert(startFlyTime, obj.transform.DOPath(bezierArray, duration, PathType.CatmullRom).SetEase(Ease.InQuad));
            seq.InsertCallback(startFlyTime + duration, () =>
            {
                PlayBtnScaleAnim(target);
                GameEntry.Pool.GameObjectPool.Despawn(obj);
            });
        }
        
        public static async UniTask PlayBtnScaleAnim(Transform btnTrans, Action onComplete = null)
        {
            if (btnTrans == null) return;
            btnTrans.DOKill(complete: false);
            btnTrans.localScale = Vector3.one;
            btnTrans.DOPunchScale(new Vector3(0.15f, 0.15f, 0), 0.18f, 2, 0.5f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    onComplete?.Invoke();
                });
        }
        
        public static float ConvertPercent(float percent, float floor, float ceil)
        {
            percent = Mathf.Clamp01(percent);
            float mappedValue = Mathf.Lerp(floor, ceil, percent);
            return mappedValue;
        }
    }
}