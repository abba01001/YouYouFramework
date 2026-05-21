using GameScripts;

namespace GameScripts
{
    // Sys_Ability Entity
    public partial class Sys_AbilityEntity : DataTableEntityBase
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
}