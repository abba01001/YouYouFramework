using System.Collections;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    private Animator anim;
    private float lastClickTime;
    [Tooltip("连击时间限制,超过此时间重置攻击次数")] public float comboTimeLimit;
    [Tooltip("点击时间限制，防止连续点按造成动画播放不连续")] public float clickTimeLimit;
    private float comboTimer;
    [Tooltip("当前处于第几段攻击")] public int attackCount;

    private bool IsAttacking = false;
    private bool CanCombo = false;
    private bool DoAttackCommand = false;
    private bool InComboTime = false;
    private bool DoComboCommand = false;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        attackCount = 0;
        comboTimer = clickTimeLimit;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            InputAttack();
            CheckAttack();
        }
        TimeCounter();
    }

    private void InputAttack()
    {
        if (comboTimer > 0f) return; 
        if (InComboTime)
        {
            DoComboCommand = true;
        }

        if (attackCount > 3)
        {
            return;
        }

        if (!IsAttacking || DoComboCommand)
        {
            Debug.LogError($"{IsAttacking}---{DoComboCommand}---{attackCount}");
            attackCount++;
            DoAttackCommand = true;
            DoComboCommand = false;
            StopAllCoroutines();
        }
    }

    public void CheckAttack()
    {
        if (!DoAttackCommand) return;

        if (attackCount == 1)
        {
            anim.SetFloat("AttackCombo", 0f);
            anim.SetTrigger("swordLeftAttack");
            StartCoroutine(CheckCombo(1));
        }
        else if (attackCount == 2)
        {
            StartCoroutine(SmoothTransitionToAttackCombo(0, 1, 0.1f));
            StartCoroutine(CheckCombo(2));
        }
        else if (attackCount == 3)
        {
            StartCoroutine(SmoothTransitionToAttackCombo(1, 2, 0.1f));
            StartCoroutine(CheckCombo(3));
        }

        DoAttackCommand = false;
    }

    // 用协程替代 while 循环来检测组合动作状态
    private IEnumerator CheckCombo(int attackIndex)
    {
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        IsAttacking = true;
        while (stateInfo.normalizedTime < 1.0f) // 确保动画未播放完
        {
            stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            InComboTime = stateInfo.normalizedTime >= 0.8f;
                
            yield return null; // 每帧等待
        }
        if (attackCount > 3)
        {
            attackCount = 0;
        }
        // 动画播放完成后，重置状态
        IsAttacking = false;
        InComboTime = false;
        DoComboCommand = false;
    }

    private IEnumerator SmoothTransitionToAttackCombo(float initValue, float targetValue, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newValue = Mathf.Lerp(initValue, targetValue, elapsedTime / duration);
            anim.SetFloat("AttackCombo", newValue);
            yield return null; // 等待下一帧
        }

        anim.SetFloat("AttackCombo", targetValue); // 确保最终值为目标值
    }

    private void TimeCounter()
    {
        comboTimer -= Time.deltaTime;
        return;
        if (attackCount > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0)
            {
                attackCount = 0;
                comboTimer = comboTimeLimit;
            }
        }
    }
}
