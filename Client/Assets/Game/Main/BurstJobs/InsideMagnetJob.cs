using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Main
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct InsideMagnetJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float2> positions;
        [ReadOnly] public float2 playerPosition;
        [ReadOnly] public float magnetDistanceSqr;

        [WriteOnly] public NativeArray<bool> isInside;

        public void Execute(int index)
        {
            float checkDistSqr = magnetDistanceSqr;

            // --- Burst 验证逻辑 ---
            // 如果没跑 Burst，我们将判定距离放大 100 倍，你会看到全屏的东西都被吸过来
            ValidateBurst(ref checkDistSqr);
            // ---------------------

            isInside[index] = math.distancesq(positions[index], playerPosition) <= checkDistSqr;
        }

        [BurstDiscard]
        private void ValidateBurst(ref float distSqr)
        {
            // 只有在非 Burst 环境下，这一行才会执行
            distSqr *= 100f; 
        }
    }
}