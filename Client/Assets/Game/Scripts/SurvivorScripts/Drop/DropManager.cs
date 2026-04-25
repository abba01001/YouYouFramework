using OctoberStudio.Drop;
using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using System.Collections.Generic;
using Main;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace OctoberStudio
{
    public class DropManager : MonoBehaviour
    {
        [SerializeField] DropDatabase database;

        public Dictionary<DropType, PoolComponent<DropBehavior>> dropPools = new Dictionary<DropType, PoolComponent<DropBehavior>>();
        public Dictionary<DropType, float> lastTimeDropped = new Dictionary<DropType, float>();
        
        public List<DropBehavior> dropList = new List<DropBehavior>();
        protected List<DropBehavior> waitingDrop = new List<DropBehavior>();

        protected NativeList<float2> dropPositions;
        protected NativeList<bool> isInside;

        protected JobHandle insideMagnetJobHandle;
        protected InsideMagnetJob insideMagnetJob;

        protected bool isJobRunning;
        protected int cachedCapacity;

        protected bool pickUpAllWhenFinished = false;

        public virtual void Init()
        {
            for (int i = 0; i < database.GemsCount; i++)
            {
                var data = database.GetGemData(i);

                var pool = new PoolComponent<DropBehavior>($"Drop_{data.DropType}", data.Prefab, 100);

                dropPools.Add(data.DropType, pool);
                lastTimeDropped.Add(data.DropType, 0);
            }

            dropPositions = new NativeList<float2>(500, Allocator.Persistent);
            isInside = new NativeList<bool>(500, Allocator.Persistent);

            cachedCapacity = dropPositions.Capacity;

            insideMagnetJob = new InsideMagnetJob();
            insideMagnetJob.positions = dropPositions.AsDeferredJobArray();
            insideMagnetJob.isInside = isInside.AsDeferredJobArray();
        }

        protected virtual void Update()
        {
            if (dropList.Count == 0) return;

            insideMagnetJob.playerPosition = PlayerBehavior.CenterPosition;
            insideMagnetJob.magnetDistanceSqr = PlayerBehavior.Player.MagnetRadiusSqr;

            insideMagnetJobHandle = insideMagnetJob.Schedule(dropList.Count, 64);
            JobHandle.ScheduleBatchedJobs();

            isJobRunning = true;
        }

        protected virtual void LateUpdate()
        {
            if (!isJobRunning)
            {
                pickUpAllWhenFinished = false;

                MoveWaitingDropToActive();

                return;
            }
            isJobRunning = false;

            insideMagnetJobHandle.Complete();

            if (pickUpAllWhenFinished)
            {
                PickUpAll();
            } else
            {
                var delay = 0f;
                for (int i = 0; i < dropList.Count; i++)
                {
                    if (isInside[i])
                    {
                        dropList[i].transform.DoPositionJob(PlayerBehavior.CenterTransform, 0.25f, delay, false, EasingType.BackIn).SetOnFinish(dropList[i].OnPickedUp);
                        delay += 0.002f;

                        dropList.RemoveAtSwapBack(i);
                        dropPositions.RemoveAtSwapBack(i);
                        isInside.RemoveAtSwapBack(i);

                        i--;
                    }
                }
            }

            MoveWaitingDropToActive();
        }

        protected virtual void MoveWaitingDropToActive()
        {
            if (waitingDrop != null && waitingDrop.Count > 0)
            {
                for (int i = 0; i < waitingDrop.Count; i++)
                {
                    AddDropToList(waitingDrop[i]);
                }

                waitingDrop.Clear();
            }
        }

        public virtual void PickUpAllDrop()
        {
            if (isJobRunning)
            {
                pickUpAllWhenFinished = true;
            } else
            {
                PickUpAll();
            }   
        }

        protected virtual void PickUpAll()
        {
            var delay = 0f;
            for (int i = 0; i < dropList.Count; i++)
            {
                if (dropList[i].DropData.AffectedByMagnet)
                {
                    dropList[i].transform.DoPositionJob(PlayerBehavior.CenterTransform, 0.25f, delay, false, EasingType.BackIn).SetOnFinish(dropList[i].OnPickedUp);
                    delay += 0.001f;

                    dropList.RemoveAtSwapBack(i);
                    dropPositions.RemoveAtSwapBack(i);
                    isInside.RemoveAtSwapBack(i);

                    i--;
                }
            }
        }

        public void CheckDropDown(EnemyBehavior enemy,int enemyCount)
        {
            foreach(var dropData in enemy.GetDropData())
            {
                if (dropData.Chance == 0) continue;
                var baseData = database.GetGemData<DropData>(dropData.DropType);
                if (!CheckDropCooldown(baseData)) continue;
                if(Random.value * 100 <= dropData.Chance)
                {
                    Drop(dropData.DropType, enemy.transform.position.XY() + Random.insideUnitCircle * 0.2f);
                }
            }
            
            TimeStopDropData timeStopDropData = database.GetGemData<TimeStopDropData>(DropType.TimeStop);
            if (CheckDropCooldown(timeStopDropData))
            {
                HandleSpecialConditionDrop(timeStopDropData,enemy,enemyCount);
            }
        }

        private void HandleSpecialConditionDrop(DropData baseData,EnemyBehavior enemy,int enemyCount)
        {
            switch (baseData.DropType)
            {
                case DropType.TimeStop:
                    if (baseData is TimeStopDropData timeStopDropData)
                    {
                        if (enemyCount >= timeStopDropData.EnemyCount && Random.value * 100 <= timeStopDropData.Chance)
                        {
                            Drop(baseData.DropType, enemy.transform.position.XY() + Random.insideUnitCircle * 0.2f);
                            Debugger.LogError("掉落TimeStop");
                        }
                    }
                    break;
            }
        }
        
        public virtual bool CheckDropCooldown(DropData baseData)
        {
            return Time.time - lastTimeDropped[baseData.DropType] >= baseData.DropCooldown;
        }

        public virtual void Drop(DropType dropType, Vector3 position)
        {
            var drop = dropPools[dropType].GetEntity();

            drop.Init(database.GetGemData<DropData>(dropType));
            drop.transform.position = position;

            lastTimeDropped[dropType] = Time.time;

            if (!isJobRunning)
            {
                AddDropToList(drop);
            }
            else
            {
                waitingDrop.Add(drop);
            }
        }

        protected virtual void AddDropToList(DropBehavior drop)
        {
            dropList.Add(drop);
            dropPositions.Add(drop.transform.position.XY());
            isInside.Add(false);

            if (cachedCapacity != isInside.Capacity)
            {
                insideMagnetJob.positions = dropPositions.AsDeferredJobArray();
                insideMagnetJob.isInside = isInside.AsDeferredJobArray();
            }
        }

        protected virtual void OnDestroy()
        {
            dropPositions.Dispose();
            isInside.Dispose();
        }

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        protected struct InsideMagnetJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float2> positions;
            [ReadOnly] public float2 playerPosition;
            [ReadOnly] public float magnetDistanceSqr;

            [WriteOnly] public NativeArray<bool> isInside;

            public void Execute(int index)
            {
                isInside[index] = math.distancesq(positions[index], playerPosition) <= magnetDistanceSqr;
            }
        }
    }
}