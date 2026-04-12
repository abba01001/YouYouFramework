using UnityEngine;
using Cysharp.Threading.Tasks;
using OctoberStudio;

// 边缘冲锋：固定轨迹版（轨迹生成后永不移动）
public class EnemyRush : MonoBehaviour
{
    private EnemyBehavior _enemy;
    private Vector2 _rushDirection;
    private float _rushSpeed;
    private float _rushDistance;
    private LineRenderer _pathLine;
    public Material material;
    // 固定轨迹的起点&终点（生成后永不改变）
    private Vector3 _fixedPathStart;
    private Vector3 _fixedPathEnd;

    public async UniTask StartRush(float speed, float distance, Color pathColor)
    {
        _enemy = GetComponent<EnemyBehavior>();
        _enemy.SetDoRushing(true);
        _rushSpeed = speed;
        _rushDistance = distance;

        // 冲锋方向：指向玩家（生成瞬间计算，后续不变）
        _rushDirection = (PlayerBehavior.Player.transform.position - transform.position).normalized;

        // 🔥 关键1：先生成【固定不动的轨迹】（一次性画好，永不更新）
        await CreateFixedPathLine(pathColor);

        // 🔥 关键2：敌人沿固定轨迹冲锋（轨迹全程静止）
        await RushCoroutine();
    }

    // 【固定轨迹】生成：只画一次，永不移动
    private async UniTask CreateFixedPathLine(Color color)
    {
        GameObject obj = await GameEntry.Pool.GameObjectPool.Spawn("Assets/Game/Download/SurvivorAsset/Prefabs/Other/PathArrow.prefab");
        obj.transform.SetParent(transform);
        obj.transform.localPosition = Vector3.zero;
        _pathLine = obj.GetComponent<LineRenderer>();
        _pathLine.useWorldSpace = true;

        _fixedPathStart = transform.position; // 敌人出生位置 = 轨迹起点
        _fixedPathEnd = PlayerBehavior.Player.transform.position + (Vector3)_rushDirection * _rushDistance;

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
        _enemy.DoFacePlayerDirection();
        // 冲锋阶段：禁用原AI，直线冲刺
        var distance = Mathf.Abs(Vector2.Distance(_fixedPathEnd, _fixedPathStart));
        while (traveled < distance)
        {
            if (_enemy == null || !_enemy.gameObject.activeSelf || !_enemy.IsAlive) break;
            // 沿固定方向冲锋
            transform.Translate(_rushDirection * _rushSpeed * Time.deltaTime);
            traveled += _rushSpeed * Time.deltaTime;

            // ✅ 删掉了所有轨迹更新代码！轨迹彻底固定不动
            await UniTask.Yield();
        }
        _enemy?.SetDoRushing(false);
        // 冲锋结束：销毁轨迹和组件
        Destroy(_pathLine.gameObject);
        Destroy(this);
    }
}