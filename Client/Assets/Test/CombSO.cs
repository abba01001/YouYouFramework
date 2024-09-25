using UnityEngine;

[CreateAssetMenu(menuName = "Attack/CombSO")]
public class CombSO : ScriptableObject 
{
    public AnimatorOverrideController animOVC;//需要覆盖的Animator
    public float damage;
}