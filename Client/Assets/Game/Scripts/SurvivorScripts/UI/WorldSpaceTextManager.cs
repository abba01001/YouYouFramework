using OctoberStudio.Pool;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace OctoberStudio.UI
{
    public class WorldSpaceTextManager : MonoBehaviour
    {
        // 1. 业务字段：留在热更，方便拖拽关联
        [SerializeField] protected RectTransform canvasRect;
        [SerializeField] protected GameObject textIndicatorPrefab;
        [SerializeField] protected AnimationCurve scaleCurve;
        [SerializeField] protected AnimationCurve positionCurve;
        [SerializeField] protected float maxScale;
        [SerializeField] protected float maxY;
        [SerializeField] protected float duration;

        protected PoolComponent<TextIndicatorBehavior> indicatorsPool;
        protected List<IndicatorData> indicators = new List<IndicatorData>();
        protected List<IndicatorData> waitingIndicators = new List<IndicatorData>();

        // 2. 数据容器：NativeList
        protected NativeArray<float2> scalePositionCurve;
        protected NativeList<float> spawnTimes;
        protected NativeList<float2> worldPositions;
        protected NativeList<float4> scalePosition;
        protected NativeList<bool> isFinished;

        protected JobHandle indicatorJobHandle;
        protected bool isJobRunning;

        private void Start()
        {
            indicatorsPool = new PoolComponent<TextIndicatorBehavior>(textIndicatorPrefab, 500, canvasRect);
            scalePositionCurve = CreateApproximateCurve(scaleCurve, positionCurve, 50, Allocator.Persistent);

            spawnTimes = new NativeList<float>(100, Allocator.Persistent);
            worldPositions = new NativeList<float2>(100, Allocator.Persistent);
            scalePosition = new NativeList<float4>(100, Allocator.Persistent);
            isFinished = new NativeList<bool>(100, Allocator.Persistent);
        }

        protected virtual void OnDestroy()
        {
            if (scalePositionCurve.IsCreated) scalePositionCurve.Dispose();
            if (spawnTimes.IsCreated) spawnTimes.Dispose();
            if (worldPositions.IsCreated) worldPositions.Dispose();
            if (scalePosition.IsCreated) scalePosition.Dispose();
            if (isFinished.IsCreated) isFinished.Dispose();
        }

        public void SpawnText(Vector2 worldPos, string text)
        {
            var indicator = indicatorsPool.GetEntity();
            indicator.SetText(text);

            var data = new IndicatorData { indicator = indicator, spawnTime = Time.time, worldPosition = worldPos };
            
            if (isJobRunning) waitingIndicators.Add(data);
            else AddIndicatorData(data);
        }

        protected void AddIndicatorData(IndicatorData data)
        {
            indicators.Add(data);
            spawnTimes.Add(data.spawnTime);
            worldPositions.Add(data.worldPosition);
            scalePosition.Add(new float4());
            isFinished.Add(false);
        }

        protected virtual void Update()
        {
            if (indicators.Count == 0) return;

            // 调用 AOT 层的 Job
            var job = new Main.IndicatorJob
            {
                scalePositionCurve = this.scalePositionCurve,
                spawnTimes = this.spawnTimes.AsDeferredJobArray(),
                worldPositions = this.worldPositions.AsDeferredJobArray(),
                scalePosition = this.scalePosition.AsDeferredJobArray(),
                isFinished = this.isFinished.AsDeferredJobArray(),
                viewProjMatrix = Camera.main.projectionMatrix * Camera.main.worldToCameraMatrix,
                time = Time.time,
                invertedDuration = 1f / duration,
                maxY = maxY,
                maxScale = maxScale
            };

            indicatorJobHandle = job.Schedule(indicators.Count, 32);
            JobHandle.ScheduleBatchedJobs();
            isJobRunning = true;
        }

        protected virtual void LateUpdate()
        {
            if (isJobRunning)
            {
                isJobRunning = false;
                indicatorJobHandle.Complete();

                for (int i = 0; i < indicators.Count; i++)
                {
                    if (isFinished[i])
                    {
                        indicators[i].indicator.gameObject.SetActive(false);
                        
                        // 移除数据
                        indicators.RemoveAtSwapBack(i);
                        spawnTimes.RemoveAtSwapBack(i);
                        worldPositions.RemoveAtSwapBack(i);
                        scalePosition.RemoveAtSwapBack(i);
                        isFinished.RemoveAtSwapBack(i);
                        i--;
                        continue;
                    }
                    // 更新 UI 表现
                    indicators[i].indicator.SetAnimationParameters(scalePosition[i]);
                }
            }

            if (waitingIndicators.Count > 0)
            {
                foreach (var item in waitingIndicators) AddIndicatorData(item);
                waitingIndicators.Clear();
            }
        }

        public NativeArray<float2> CreateApproximateCurve(AnimationCurve c1, AnimationCurve c2, int res, Allocator alloc)
        {
            var s = new NativeArray<float2>(res, alloc);
            for (int i = 0; i < res; i++) {
                float t = i / (float)(res - 1);
                s[i] = new float2(c1.Evaluate(t), c2.Evaluate(t));
            }
            return s;
        }

        protected class IndicatorData {
            public TextIndicatorBehavior indicator;
            public float spawnTime;
            public Vector2 worldPosition;
        }
    }
}