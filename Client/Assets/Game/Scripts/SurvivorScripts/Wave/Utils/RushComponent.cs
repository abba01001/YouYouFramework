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
        public async UniTask StartRush(EnemyBehavior enemyBehavior, RushWave rushWave)
        {
            _enemy = enemyBehavior;
            _rushSpeed = GameUtil.RandomRange(rushWave.RushSpeed.x, rushWave.RushSpeed.y);
            float delay = GameUtil.RandomRange(rushWave.RushDelay.x, rushWave.RushDelay.y);
            if (delay != 0f)
            {
                await UniTask.Delay(Mathf.RoundToInt(delay * 1000));
            }

            _enemy.SetEnemyAnimType(BehaviorType.RushBehavior);
            _rushDirection = (PlayerBehavior.Player.transform.position - transform.position).normalized;

            await CreateFixedPathLine();
            await RushCoroutine();
        }

        private async UniTask CreateFixedPathLine()
        {
            _pathLine = transform.GetComponentInChildren<LineRenderer>(true);
            _pathLine.gameObject.MSetActive(true);
            _pathLine.useWorldSpace = true;
            _fixedPathStart = transform.position; // 敌人出生位置 = 轨迹起点
            _fixedPathEnd = PlayerBehavior.Player.transform.position;

            _pathLine.SetPosition(0, _fixedPathStart);
            _pathLine.SetPosition(1, _fixedPathEnd);

            _pathLine.sortingOrder = -1;
            await UniTask.Yield();
        }

        private async UniTask RushCoroutine()
        {
            float traveled = 0f;
            var distance = Mathf.Abs(Vector2.Distance(_fixedPathEnd, _fixedPathStart));
            while (traveled < distance)
            {
                if (_enemy == null || !_enemy.gameObject.activeSelf || !_enemy.IsAlive) break;
                _enemy.transform.Translate(_rushDirection * _rushSpeed * Time.deltaTime);
                traveled += _rushSpeed * Time.deltaTime;
                await UniTask.Yield();
            }

            _enemy?.ResetEnemyAnimType();
            _pathLine?.gameObject?.MSetActive(false);
            GameEntry.Pool.GameObjectPool.Despawn(this.gameObject);
        }
    }
}