using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( menuName = "Attack/CombAttack")]
public class CombAttack : ScriptableObject 
{ 
    public List<CombSO> combAttackList = new List<CombSO>();  
}