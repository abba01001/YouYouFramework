using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OctoberStudio
{
    [CreateAssetMenu(fileName = "Drop Database", menuName = "October/Drop Database")]
    public class DropDatabase : ScriptableObject
    {
        // 隐藏旧列表，仅用于迁移数据
        [SerializeField, HideInInspector] 
        private List<DropData> gems = new List<DropData>();

        [Title("掉落物配置面板")]
        [SerializeReference]
        [TypeFilter("GetDropDataTypes")]
        [Searchable] // 支持搜索 DropType 或 名字
        [ListDrawerSettings(
            ListElementLabelName = "DisplayTitle", // 关键：显示我们定义的标题属性
            ShowIndexLabels = false,              // 隐藏 0, 1, 2...
            Expanded = true,                      // 默认全部展开
            ShowPaging = false)]              // 不分页，全部显示
        public List<DropData> gemDatas = new List<DropData>();

        private IEnumerable<Type> GetDropDataTypes()
        {
            return typeof(DropData).Assembly.GetTypes()
                .Where(x => !x.IsAbstract && !x.IsInterface && typeof(DropData).IsAssignableFrom(x));
        }

        #region 工具功能

        [Button("从旧 gems 迁移数据", ButtonSizes.Large)]
        [GUIColor(0.4f, 0.8f, 1f)]
        [InfoBox("迁移会将旧 gems 列表中的数据深拷贝到新列表中。")]
        public void MigrateGems()
        {
            if (gems == null || gems.Count == 0) return;
            
            // 使用 Json 序列化进行深拷贝，确保对象独立
            foreach (var oldGem in gems)
            {
                string json = JsonUtility.ToJson(oldGem);
                // 默认迁移为基础类型，后续可在面板手动更改
                var newGem = JsonUtility.FromJson<DropData>(json); 
                gemDatas.Add(newGem);
            }
            Debug.Log("数据迁移完成！");
        }

        #endregion

        #region 数据访问接口

        public int GemsCount => gemDatas.Count;

        public DropData GetGemData(int index)
        {
            if (index < 0 || index >= gemDatas.Count) return null;
            return gemDatas[index];
        }

        public T GetGemData<T>(DropType dropType) where T : DropData
        {
            var data = gemDatas.FirstOrDefault(g => g.DropType == dropType);
            return data as T;
        }

        #endregion
    }
    
    
    
    
    
    
    
    
    [Serializable]
    public class DropData
    {
        [SerializeField, BoxGroup("基础属性")]
        protected DropType dropType;
        
        [SerializeField, BoxGroup("基础属性")]
        protected GameObject prefab;
        
        [SerializeField, BoxGroup("基础属性")]
        protected bool affectedByMagnet;
        
        [SerializeField, Min(0), BoxGroup("基础属性")]
        protected float dropCooldown;

        [SerializeField, BoxGroup("基础属性")]
        protected bool isConditionDrop;
        
        
        public DropType DropType => dropType;
        public GameObject Prefab => prefab;
        public bool AffectedByMagnet => affectedByMagnet;
        public float DropCooldown => dropCooldown;
        public bool IsConditionDrop => isConditionDrop;
        // 用于 Odin 列表显示的标题
        public string DisplayTitle => $"[{GetDropTypeLabel()}] {(prefab != null ? prefab.name : "空预制体")}";
        
        private string GetDropTypeLabel()
        {
            // 获取枚举上的 [LabelText]
            var memberInfo = dropType.GetType().GetMember(dropType.ToString()).FirstOrDefault();
            if (memberInfo != null)
            {
                var labelText = memberInfo.GetCustomAttribute<LabelTextAttribute>();
                if (labelText != null)
                    return labelText.Text;
            }
            // 取不到就返回英文
            return dropType.ToString();
        }
    }

    [Serializable]
    public class TimeStopDropData : DropData
    {
        [SerializeField, BoxGroup("特殊属性"), LabelText("影响敌人数量")]
        private int enemyCount;
        [SerializeField,Range(0, 100), BoxGroup("特殊属性"), LabelText("掉落概率")]
        float chance;

        public int EnemyCount => enemyCount;
        public float Chance => chance;
    }
    
    // 你可以根据需要继续添加其他特殊类
    // [Serializable] public class GoldDropData : DropData { public int amount; }
}