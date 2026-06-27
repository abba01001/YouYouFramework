// ========================================================
// 此配置由python工具自动生成，请勿手动修改！
// 如需拓展请在 Extend 目录下Sys_AbilityDBModelExt.cs对应类进行扩展接口
// ========================================================

using System.Collections.Generic;

namespace GameScripts
{
    // Sys_Ability Entity
    public partial class Sys_AbilityEntity : ConfigEntityBase
    {
        // 编号
        public int Id;
        // 技能类型
        public string AbilityType;
        // 解锁关卡
        public string UnlockLv;
        // 技能名称
        public string Title;
        // 技能描述
        public string Description;
        // UI 图标路径
        public string IconPath;
        // 技能预制体路径
        public string PrefabPath;
        // 是否主动技能
        public string IsActiveAbility;
        // 是否武器技能
        public string IsWeaponAbility;
        // 是否保底技能
        public string IsEndgameAbility;
        // 是否超武
        public string IsEvolution;
    }

    public partial class Sys_AbilityDBModel : ConfigBase<Sys_AbilityDBModel, Sys_AbilityEntity>
    {
        public override string ConfigName => "Sys_Ability";

        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();
            for (int i = 0; i < rows; i++)
            {
                var entity = new Sys_AbilityEntity();
                entity.Id = ms.ReadInt();
                entity.AbilityType = ms.ReadUTF8String();
                entity.UnlockLv = ms.ReadUTF8String();
                entity.Title = ms.ReadUTF8String();
                entity.Description = ms.ReadUTF8String();
                entity.IconPath = ms.ReadUTF8String();
                entity.PrefabPath = ms.ReadUTF8String();
                entity.IsActiveAbility = ms.ReadUTF8String();
                entity.IsWeaponAbility = ms.ReadUTF8String();
                entity.IsEndgameAbility = ms.ReadUTF8String();
                entity.IsEvolution = ms.ReadUTF8String();
                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}