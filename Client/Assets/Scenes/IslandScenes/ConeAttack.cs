using System.Collections.Generic;
using UnityEngine;

public class ConeAttack : MonoBehaviour
{
    public float attackRange = 5f; // 攻击范围
    public float attackAngle = 60f; // 扇形角度
    public LayerMask targetLayer; // 目标层

    void Update()
    {
        // 检测攻击输入（例如按下某个键）
        if (Input.GetKeyDown(KeyCode.Space)) // 空格键触发攻击
        {
            DetectTargetsInAttackArea();
        }
    }

    void DetectTargetsInAttackArea()
    {
        Vector3 position = transform.position;
        Collider[] hitColliders = Physics.OverlapSphere(position, attackRange, targetLayer);
        float cosAttackAngle = Mathf.Cos(attackAngle * Mathf.Deg2Rad);
        Vector3 attackDirection = transform.forward; // 玩家朝向
        foreach (var target in hitColliders) // 遍历场景中的所有潜在目标
        {
            Vector3 targetDirection = (target.transform.position - transform.position).normalized; // 目标方向
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
            if (distanceToTarget > attackRange) continue;
            float dot = Vector3.Dot(attackDirection, targetDirection); 
            // 如果点乘值大于 cos(attackAngle)，说明目标在扇形区域内
            if (dot >= cosAttackAngle)
            {
                // 目标在攻击范围内
                Debug.Log("目标在攻击范围内: " + target.name);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 position = transform.position;
        Vector3 forward = transform.forward;

        Gizmos.DrawWireSphere(position, attackRange);
        for (float i = -attackAngle / 2; i <= attackAngle / 2; i += 1)
        {
            Quaternion rotation = Quaternion.Euler(0, i, 0);
            Vector3 direction = rotation * forward;
            Gizmos.DrawLine(position, position + direction * attackRange);
        }
    }
}
