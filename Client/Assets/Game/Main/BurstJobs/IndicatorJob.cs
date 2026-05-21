using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using System.Runtime.CompilerServices;

namespace Main
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct IndicatorJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<float2> scalePositionCurve;
        [ReadOnly] public NativeArray<float> spawnTimes;
        [ReadOnly] public NativeArray<float2> worldPositions;

        [WriteOnly] public NativeArray<float4> scalePosition;
        [WriteOnly] public NativeArray<bool> isFinished;

        [ReadOnly] public float4x4 viewProjMatrix;
        [ReadOnly] public float invertedDuration;
        [ReadOnly] public float time;
        [ReadOnly] public float maxY;
        [ReadOnly] public float maxScale;

        public void Execute(int index)
        {
            // 默认倍率是 1
            float testMultiplier = 1.0f;

            // 如果没跑 Burst，这个方法会把倍率改成 50
            CheckBurstStatus(ref testMultiplier);

            var t = (time - spawnTimes[index]) * invertedDuration;
            isFinished[index] = t >= 1f;

            var curveData = EvaluateCurve(t);

            var worldPos = new float4(worldPositions[index].x, worldPositions[index].y, 0f, 1f);
            float4 clip = math.mul(viewProjMatrix, worldPos);
            float3 ndc = clip.xyz / clip.w;

            var result = new float4();
            
            // 这里的 testMultiplier 如果生效，缩放会变得巨大
            result.x = curveData.x * maxScale * testMultiplier;   
            result.y = curveData.y * maxY;       
            result.z = ndc.x * 0.5f + 0.5f;      
            result.w = ndc.y * 0.5f + 0.5f;      

            scalePosition[index] = result;
        }

        // [BurstDiscard] 关键点：如果是汇编代码执行，这一段会被直接删掉
        [BurstDiscard]
        private void CheckBurstStatus(ref float multiplier)
        {
            // 只有走慢速 C# 逻辑时，这里才会执行，把倍率强行拉到 50 倍
            multiplier = 50.0f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float2 EvaluateCurve(float t)
        {
            var normalized = math.saturate(t);
            var fIndex = normalized * (scalePositionCurve.Length - 1);
            var i0 = (int)math.floor(fIndex);
            var i1 = math.min(i0 + 1, scalePositionCurve.Length - 1);
            var frac = fIndex - i0;

            return math.lerp(scalePositionCurve[i0], scalePositionCurve[i1], frac);
        }
    }
}