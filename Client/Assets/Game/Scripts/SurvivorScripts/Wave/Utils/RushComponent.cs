using UnityEngine;
using Cysharp.Threading.Tasks;

using OctoberStudio;
using OctoberStudio.Timeline;

namespace GameScripts
{
    // 边缘冲锋：固定轨迹版（轨迹生成后永不移动）
    public class RushComponent : MonoBehaviour
    {
        private EnemyBehavior _enemy;
        private Vector2 _rushDirection;
        private float _rushSpeed;
        private LineRenderer _pathLine;
        
        // 固定轨迹的起点&终点（生成后永不改变）
        private Vector3 _fixedPathStart;
        private Vector3 _fixedPathEnd;
        public async UniTask StartRush(EnemyBehavior enemyBehavior,RushWave rushWave)
        {
            _enemy = enemyBehavior;
            _rushSpeed = rushWave.RushSpeed;
            
            if (rushWave.RushDelay != 0f)
            {
                await UniTask.Delay(Mathf.RoundToInt(rushWave.RushDelay * 1000));
            }
            
            // 冲锋方向：指向玩家（生成瞬间计算，后续不变）
            _enemy.SetEnemyAnimType(EnemyAnimType.Rush);
            
            _rushDirection = (PlayerBehavior.Player.transform.position - transform.position).normalized;
    
            // 🔥 关键1：先生成【固定不动的轨迹】（一次性画好，永不更新）
            await CreateFixedPathLine();
    
            // 🔥 关键2：敌人沿固定轨迹冲锋（轨迹全程静止）
            await RushCoroutine();
        }
    
        // 【固定轨迹】生成：只画一次，永不移动
        private async UniTask CreateFixedPathLine()
        {
            _pathLine = transform.GetComponentInChildren<LineRenderer>(true);
            _pathLine.gameObject.MSetActive(true);
            
            _pathLine.useWorldSpace = true;
    
            _fixedPathStart = transform.position; // 敌人出生位置 = 轨迹起点
            _fixedPathEnd = PlayerBehavior.Player.transform.position;
    
            // 一次性设置轨迹，后续绝不修改
            _pathLine.SetPosition(0, _fixedPathStart);
            _pathLine.SetPosition(1, _fixedPathEnd);
    
            // 6. 层级（避免被挡住）
            _pathLine.sortingOrder = -1;
    
            await UniTask.Yield();
        }
    
        // 冲锋逻辑：沿固定轨迹冲，轨迹全程静止不动
        private async UniTask RushCoroutine()
        {
            float traveled = 0f;
            // 冲锋阶段：禁用原AI，直线冲刺
            var distance = Mathf.Abs(Vector2.Distance(_fixedPathEnd, _fixedPathStart));
            while (traveled < distance)
            {
                if (_enemy == null || !_enemy.gameObject.activeSelf || !_enemy.IsAlive) break;
                // 沿固定方向冲锋
                _enemy.transform.Translate(_rushDirection * _rushSpeed * Time.deltaTime);
                traveled += _rushSpeed * Time.deltaTime;
    
                // ✅ 删掉了所有轨迹更新代码！轨迹彻底固定不动
                await UniTask.Yield();
            }
            _enemy?.ResetEnemyAnimType();
            // 冲锋结束：销毁轨迹和组件
            _pathLine?.gameObject?.MSetActive(false);
            GameEntry.Pool.GameObjectPool.Despawn(this.gameObject);
        }
    }
}