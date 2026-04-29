using System.Collections.Generic;
using GameScripts;

namespace GameScripts
{
    public partial class Sys_AbilityDBModel : DataTableDBModelBase<Sys_AbilityDBModel, Sys_AbilityEntity>
    {
        public override string DataTableName => "Sys_Ability";

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