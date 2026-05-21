using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OctoberStudio
{
    [CreateAssetMenu(fileName = "Characters Database", menuName = "October/Characters Database")]
    public class CharactersDatabase : ScriptableObject
    {
        [Title("角色数据列表")]
        [LabelText("角色库")]
        [Searchable]
        [ListDrawerSettings(
            NumberOfItemsPerPage = 20, 
            ShowIndexLabels = false,          // 1. 关闭索引显示
            ListElementLabelName = "name"    // 2. 指定使用 'type' 字段作为列表标签
            // CustomAddFunction = "AddNewEnemy"
        )]
        [SerializeField] protected List<CharacterData> characters = new List<CharacterData>();

        
        
        public int CharactersCount => characters.Count;
        public virtual CharacterData GetCharacterData(int index)
        {
            return characters[index];
        }
    }
    
    
    
    [System.Serializable]
    public class CharacterData
    {
        [SerializeField] protected string name;
        public string Name => name;

        [SerializeField] protected int cost;
        public int Cost => cost;

        [SerializeField] protected Sprite icon;
        public Sprite Icon => icon;

        [SerializeField] protected GameObject prefab;
        public GameObject Prefab => prefab;

        [Space]
        [SerializeField] protected bool hasStartingAbility = false;
        public bool HasStartingAbility => hasStartingAbility;

        [SerializeField] protected AbilityType startingAbility;
        public AbilityType StartingAbility => startingAbility;

        [Space]
        [SerializeField, Min(1)] protected float baseHP;
        public float BaseHP => baseHP;

        [SerializeField, Min(1f)] protected float baseDamage;
        public float BaseDamage => baseDamage;
    }
}