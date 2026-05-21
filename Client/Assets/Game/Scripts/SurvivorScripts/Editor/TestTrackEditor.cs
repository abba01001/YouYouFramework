using System.Collections.Generic;
using GameScripts;
using OctoberStudio.Timeline;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.U2D;

namespace OctoberStudio.Timeline.Editor
{
    [CustomTimelineEditor(typeof(WaveTrack))]
    public class TestTrackEditor : TrackEditor
    {
        private Dictionary<EnemyType, Sys_ModelEntity> config;
        public Dictionary<EnemyType, Sys_ModelEntity> Config
        {
            get
            {
                if (config == null)
                {
                    config = new Dictionary<EnemyType, Sys_ModelEntity>();
                    LoadBytesFile();
                }
                return config;
            }
        }

        private SpriteAtlas spriteAtlas;
        public override TrackDrawOptions GetTrackOptions(TrackAsset track, Object binding)
        {
            var waveTrack = (WaveTrack)track;

            waveTrack.name = $"Track ({waveTrack.EnemyType})";

            var options = base.GetTrackOptions(track, binding);
            options.trackColor = Color.green;
            options.minimumHeight = 30;
            Config.TryGetValue(waveTrack.EnemyType,out Sys_ModelEntity entity);
            if(entity != null)
            {
                Sprite enemyIcon = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Game/Download/SurvivorAsset/Sprites/Editor/Enemies/{entity.EnemyType}.png");
                options.icon = enemyIcon.texture;
            }

            return options;
        }

        private bool testFlag = false;
        public void LoadBytesFile()
        {
            if(testFlag) return;
            testFlag = true;
            // 路径必须以 Assets/ 开头，包含后缀名
            string path = "Assets/Game/Download/DataTable/Sys_Model.bytes";
    
            // 加载为 TextAsset
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);

            if (asset != null)
            {
                // 方案 A：如果你想拿二进制字节流
                byte[] rawData = asset.bytes;
        
                // 方案 B：如果你想拿文本内容
                string content = asset.text;
                using (MMO_MemoryStream ms = new MMO_MemoryStream(rawData))
                {
                    LoadList(ms);
                }
            }
        }
        
            
        public void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();
            for (int i = 0; i < rows; i++)
            {
                var entity = new Sys_ModelEntity();
                entity.Id = ms.ReadInt();
                entity.ModelType = ms.ReadUTF8String();
                entity.EnemyType = ms.ReadUTF8String();
                entity.Speed = ms.ReadFloat();
                entity.Damage = ms.ReadFloat();
                entity.Hp = ms.ReadFloat();
                entity.LifeTime = ms.ReadFloat();
                if (System.Enum.TryParse(entity.EnemyType, out EnemyType type))
                {
                    config.Add(type,entity);
                }
            }
        }
    }
}