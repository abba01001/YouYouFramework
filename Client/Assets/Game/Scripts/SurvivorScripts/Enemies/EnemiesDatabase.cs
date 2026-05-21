using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        
        [Button("打印 Excel 专用掉落格式"), GUIColor(0.4f, 0.8f, 0.4f)]
        public void PrintDropsForExcel()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("敌人类型\t掉落配置字符串");

            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;

                // 获取所有掉落项并转为 "数字|概率" 格式
                var dropPairs = enemy.EnemyDrop
                    .Select(d => $"{(int)d.dropType}:{d.chance}") // (int) 强制转换枚举为数字
                    .ToArray();

                // 用 | 连接所有项
                string dropString = string.Join("|", dropPairs);

                // 打印到控制台，方便一键复制
                Debug.Log($"【{enemy.Type}】 掉落配置: {dropString}");
        
                // 同时存入 StringBuilder 方便整表导出
                sb.AppendLine($"{enemy.Type}\t{dropString}");
            }

            // 如果你想一次性复制整表，可以看最后这一条日志
            Debug.Log("<color=cyan>--- 以下为整表内容 (可直接粘入 Excel) ---</color>\n" + sb.ToString());
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
        public DropType dropType;

        [HorizontalGroup]
        [SerializeField, Range(0, 100), LabelText("概率%")] 
        public float chance;
    }

    public enum EnemyType
    {
        Null = -1,
        MonsterPumpkin = 0,
        MonsterBat = 1,
        MonsterSlime = 2,
        MonsterVampire = 3,
        MonsterPlant = 4,
        MonsterJellyfish = 5,
        MonsterBug = 8,
        MonsterWasp = 9,
        MonsterHand = 10,
        MonsterEye = 11,
        MonsterFireSlime = 12,
        MonsterPurpleJellyfish = 13,
        MonsterStagBeetle = 14,
        MonsterShade = 15,
        MonsterShadeJellyfish = 16,
        MonsterShadeBat = 17,
        MonsterShadeVampire = 18,
        MonsterSalamander = 19,
        MonsterSkulledSalamander = 20,
        MonsterChargedSalamander = 21,
        MonsterArmoredSalamander = 22,
        MonsterYeti = 23,
        MonsterBeastYeti = 24,
        MonsterChainedYeti = 25,
        MonsterNewSlime = 26,
        MonsterEvilSlime = 27,
        MonsterBirdSlime = 28,
        BossCrab = 29,
        BossMask = 30,
        BossMegaSlime = 31,
        BossQueenWasp = 32,
        BossVoid = 33,
        BossBell = 34,
        MonsterPureSlime = 35,
        MonsterGlopSlime = 36,
        MonsterStoneGolem = 37,
        MonsterMagmaGolem = 38,
        MonsterCrystalGolem = 39,
        MonsterSwordShadow = 40,
        MonsterMaceShadow = 41,
    }
}