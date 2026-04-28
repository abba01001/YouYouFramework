using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace OctoberStudio
{
    [CreateAssetMenu(menuName = "October/Enemies Database", fileName = "Enemies Database")]
    public class EnemiesDatabase : ScriptableObject
    {
        [Title("敌人数据列表")]
        [LabelText("敌人库")]
        [Searchable]
        [ListDrawerSettings(
            NumberOfItemsPerPage = 20, 
            ShowIndexLabels = false,          // 1. 关闭索引显示
            ListElementLabelName = "type",    // 2. 指定使用 'type' 字段作为列表标签
            CustomAddFunction = "AddNewEnemy"
        )]
        [SerializeField] List<EnemyData> enemies = new List<EnemyData>();

        public int EnemiesCount => enemies.Count;

        private void AddNewEnemy()
        {
            enemies.Add(new EnemyData());
        }

        public EnemyData GetEnemyData(int index) => enemies[index];

        public EnemyData GetEnemyData(EnemyType type)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i].Type == type) return enemies[i];
            }
            return null;
        }

        public Dictionary<EnemyType, EnemyData> GetEnemyDataDictionary()
        {
            var dictionary = new Dictionary<EnemyType, EnemyData>();
            for (int i = 0; i < enemies.Count; i++)
            {
                if (!dictionary.ContainsKey(enemies[i].Type))
                {
                    dictionary.Add(enemies[i].Type, enemies[i]);
                }
            }
            return dictionary;
        }
    }

    [System.Serializable]
    public class EnemyData
    {
        [HorizontalGroup("Split", 0.7f)]
        [BoxGroup("Split/Basic", LabelText = "基础配置")]
        [SerializeField, LabelText("敌人类型")] 
        EnemyType type; // 上面的 ListElementLabelName 会直接引用这个字段的值

        [BoxGroup("Split/Basic")]
        [SerializeField, LabelText("预制体")] 
        GameObject prefab;

        [VerticalGroup("Split/Right")]
        [PreviewField(60, ObjectFieldAlignment.Center)]
        [SerializeField, HideLabel] 
        Sprite icon;

        [PropertySpace(10)]
        [TableList(AlwaysExpanded = true)]
        [SerializeField, LabelText("掉落表")] 
        List<EnemyDropData> enemyDrop;

        public EnemyType Type => type;
        public GameObject Prefab => prefab;
        public Sprite Icon => icon;
        public List<EnemyDropData> EnemyDrop => enemyDrop;
    }

    [System.Serializable]
    public class EnemyDropData
    {
        [HorizontalGroup]
        [SerializeField, HideLabel] 
        DropType dropType;

        [HorizontalGroup]
        [SerializeField, Range(0, 100), LabelText("概率%")] 
        float chance;

        public DropType DropType => dropType;
        public float Chance => chance;
    }

    public enum EnemyType
    {
        SampleType = -2,
        Null = -1,
        Pumpkin = 0,
        Bat = 1,
        Slime = 2,
        Vampire = 3,
        Plant = 4,
        Jellyfish = 5,
        Bug = 8,
        Wasp = 9,
        Hand = 10,
        Eye = 11,
        FireSlime = 12,
        PurpleJellyfish = 13,
        StagBeetle = 14,
        Shade = 15,
        ShadeJellyfish = 16,
        ShadeBat = 17,
        ShadeVampire = 18,
        MonsterSalamander = 19,
        MonsterSkulledSalamander = 20,
        MonsterChargedSalamander = 21,
        MonsterArmoredSalamander = 22,
        MonsterYeti = 23,
        MonsterBeastYeti = 24,
        MonsterChainedYeti = 25,
        MonsterSlime = 26,
        MonsterEvilSlime = 27,
        MonsterBirdSlime = 28,
        BossCrab = 29,
        BossMask = 30,
        BossMegaSlime = 31,
        BossQueenWasp = 32,
        BossVoid = 33,
        EnemyBell = 34
    }
}