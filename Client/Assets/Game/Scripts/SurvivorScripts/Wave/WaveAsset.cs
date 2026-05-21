using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace OctoberStudio.Timeline
{
    public abstract class WaveAsset : PlayableAsset
    {
        [ShowInInspector, LabelText("预计敌人总数"), DisplayAsString]public abstract int EnemiesCount { get; }
        [HideInInspector]public EnemyType EnemyType;
        [LabelText("围绕玩家位置刷新")][PropertyOrder(998)][SerializeField] protected bool circularSpawn;
        [LabelText("属性覆盖配置")][PropertyOrder(999)][SerializeField] protected WaveOverride waveOverride;
    }

    [System.Serializable]
    public class WaveOverride
    {
        [HorizontalGroup("Damage")]
        [ToggleLeft, LabelText("覆盖攻击力")]
        [SerializeField] protected bool useDamageOverride;
        [HorizontalGroup("Damage")]
        [HideLabel, EnableIf("useDamageOverride")]
        [SerializeField] protected float damageOverride;

        public float ApplyDamageOverride(float damage)
        {
            return useDamageOverride ? damageOverride : damage;
        }

        [HorizontalGroup("HP")]
        [ToggleLeft, LabelText("覆盖生命值")]
        [SerializeField] protected bool useHPOverride;
        [HorizontalGroup("HP")]
        [HideLabel, EnableIf("useHPOverride")]
        [SerializeField] protected float hpOverride;

        public float ApplyHPOverride(float hp)
        {
            return useHPOverride ? hpOverride : hp;
        }

        [HorizontalGroup("Speed")]
        [ToggleLeft, LabelText("覆盖移动速度")]
        [SerializeField] protected bool useSpeedOverride;
        [HorizontalGroup("Speed")]
        [HideLabel, EnableIf("useSpeedOverride")]
        [SerializeField] protected float speedOverride;

        public float ApplySpeedOverride(float speed)
        {
            return useSpeedOverride ? speedOverride : speed;
        }

        [ToggleLeft, LabelText("覆盖掉落列表")]
        [SerializeField] protected bool useDropOverride;
        [ShowIf("useDropOverride")]
        [SerializeField] protected List<EnemyDropData> dropOverride;

        public List<EnemyDropData> ApplyDropOverride(List<EnemyDropData> drop)
        {
            return useDropOverride ? dropOverride : drop;
        }

        [ToggleLeft, LabelText("禁用屏幕外传送")]
        [SerializeField] protected bool disableOffscreenTeleport;
        public bool DisableOffscreenTeleport => disableOffscreenTeleport;
    }
}