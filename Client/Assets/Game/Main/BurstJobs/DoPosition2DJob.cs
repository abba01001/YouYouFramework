using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Main
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct DoPosition2DJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float2> timeData;
        [ReadOnly] public NativeArray<float> useUnscaledTime;
        // 核心修改：移除 #if UNITY_EDITOR，真机必须支持指针
        [ReadOnly] public NativeArray<FunctionPointer<EasingFunctions.EasingFunction>> easingFunctions;
        [ReadOnly] public NativeArray<float2> startPositions;
        [ReadOnly] public NativeArray<float2> targets;
        [WriteOnly] public NativeArray<float2> positions;

        [ReadOnly] public float scaledTime;
        [ReadOnly] public float unscaledTime;

        public void Execute(int i)
        {
            var time = math.select(unscaledTime, scaledTime, useUnscaledTime[i] == 0f);
            var t = math.unlerp(timeData[i].x, timeData[i].y, time);
            
            // 使用函数指针计算 Easing
            t = easingFunctions[i].Invoke(math.saturate(t));

            positions[i] = startPositions[i] + (targets[i] - startPositions[i]) * t;
        }
    }
}