using UnityEngine;
using Cysharp.Threading.Tasks;

using OctoberStudio;
using OctoberStudio.Timeline;
using UnityEngine.UI;

namespace GameScripts
{
    // 边缘冲锋：固定轨迹版（轨迹生成后永不移动）
    public class FlashComponent : MonoBehaviour
    {
        private EnemyBehavior _enemy;
        private float _flashDistance;
        private float _flashDelay;
        private float _spawnDistanceRange;
        [SerializeField] private GameObject flashTarget;
        public async UniTask StartFlash(EnemyBehavior enemyBehavior,FlashWave flashWave)
        {
            flashTarget.MSetActive(false);
            _enemy = enemyBehavior;
            _flashDistance = GameUtil.RandomRange(flashWave.FlashDistance.x, flashWave.FlashDistance.y);
            _flashDelay = GameUtil.RandomRange(flashWave.FlashDelay.x, flashWave.FlashDelay.y);
            _spawnDistanceRange = GameUtil.RandomRange(flashWave.SpawnDistanceRange.x, flashWave.SpawnDistanceRange.y);
            // flashWave.
            await RushCoroutine();
        }
    
        // 冲锋逻辑：沿固定轨迹冲，轨迹全程静止不动
        private async UniTask RushCoroutine()
        {
            float distance = Vector2.Distance(PlayerBehavior.Player.transform.position, transform.position);
            while (distance > _flashDistance)
            {
                if (_enemy == null || !_enemy.gameObject.activeSelf || !_enemy.IsAlive) break;
    
                await UniTask.Yield();
                distance = Vector2.Distance(PlayerBehavior.Player.transform.position, transform.position);
            }
            
            _enemy.SetEnemyAnimType(EnemyAnimType.Flash);
            Vector3 position = Vector3.zero;
            if (_enemy != null && _enemy.gameObject.activeSelf && _enemy.IsAlive)
            {
                var player = PlayerBehavior.Player.transform;
                float randomAngle = Random.Range(0f, 360f);
                Vector2 randomDir = Quaternion.Euler(0, 0, randomAngle) * Vector2.right;
                position = (Vector2)player.position + randomDir * _spawnDistanceRange;
                flashTarget.transform.position = position;
                flashTarget.MSetActive(true);
                //这里可以补充 一些UI表现
            }
    
            if (_flashDelay != 0f) await UniTask.Delay(Mathf.RoundToInt(_flashDelay * 1000));
    
            if (_enemy != null && _enemy.gameObject.activeSelf && _enemy.IsAlive)
            {
                _enemy.transform.position = position;
                _enemy?.ResetEnemyAnimType();
            }
    
            flashTarget.MSetActive(false);
            GameEntry.Pool.GameObjectPool.Despawn(this.gameObject);
        }
    }
}