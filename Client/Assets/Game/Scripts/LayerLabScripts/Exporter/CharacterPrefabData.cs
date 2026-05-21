using System;
using System.Collections;
using System.Collections.Generic;
using LayerLab.ArtMaker;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

namespace LayerLab.ArtMaker
{
   public class CharacterPrefabData : MonoBehaviour
   {
       [HorizontalGroup("EditorPreview")]
       [Button(ButtonSizes.Large, Name = "预览换装")]
       [InfoBox("在编辑器下实时应用当前的皮肤配置", InfoMessageType.Info)]
       public void PreviewSkin()
       {
           // 确保在编辑器下也能获取到引用
           if (partsManager == null) 
               partsManager = GetComponentInChildren<PartsManager>();

           if (partsManager == null) return;

           // 如果 PartsManager 的 Init 逻辑包含寻找组件，这里必须调用
           partsManager.Init();
           ApplyPartData();
           ApplyColorData();
    
           // 注意：编辑器下不能运行协程，所以跳过 ApplyColorPickerPositionsDelayed
           Debug.Log("Editor Preview Applied.");
       }
       
       #region Data Classes
       
       // 皮肤部件数据类
       [Serializable]
       public class SkinPartData
       {
           public PartsType partType;
           public int selectedIndex;
           public bool isHidden;
       }

       // 插槽颜色数据类
       [Serializable]
       public class SlotColorData
       {
           public string slotName;
           public Color color;
       }

       // 选色器位置数据类
       [Serializable]
       public class ColorPickerPositionData
       {
           public PartsType partType;
           public Vector2 position;
       }
       
       #endregion

       #region Fields and Properties
       
       [SerializeField] public List<SkinPartData> skinParts = new();
       [SerializeField] public List<SlotColorData> slotColors = new();
       [SerializeField] public List<ColorPickerPositionData> colorPickerPositions = new();

       private PartsManager partsManager;
       
       #endregion

       #region Unity Lifecycle
       
       // 初始化组件
       private void Awake()
       {
           partsManager = GetComponentInChildren<PartsManager>();
       }

       // 启动时应用保存的数据
       private void Start()
       {
           ApplySavedSkinData();
       }
       
       #endregion

       #region Data Loading
       
       // 应用保存的皮肤数据
       public void ApplySavedSkinData()
       {
           if (partsManager == null) return;

           partsManager.Init();
           ApplyPartData();
           ApplyColorData();
           ApplyColorPickerPositions();
       }

       // 应用部件数据
       private void ApplyPartData()
       {
           var activeIndices = new Dictionary<PartsType, int>();

           foreach (var partData in skinParts)
           {
               activeIndices[partData.partType] = partData.selectedIndex;
               partsManager.SetHideItem(partData.partType, partData.isHidden);
           }

           partsManager.SetSkinActiveIndex(activeIndices);
       }

       // 应用颜色数据
       private void ApplyColorData()
       {
           foreach (var colorData in slotColors)
           {
               if (colorData.slotName.StartsWith("hair"))
               {
                   partsManager.ChangeHairColor(colorData.color);
               }
               else if (colorData.slotName.StartsWith("beard"))
               {
                   partsManager.ChangeBeardColor(colorData.color);
               }
               else if (colorData.slotName.StartsWith("brow"))
               {
                   partsManager.ChangeBrowColor(colorData.color);
               }
               else if (colorData.slotName.StartsWith("body"))
               {
                   partsManager.ChangeSkinColor(colorData.color);
               }
           }
       }

       // 应用选色器位置
       private void ApplyColorPickerPositions()
       {
           if (colorPickerPositions.Count > 0)
           {
               StartCoroutine(ApplyColorPickerPositionsDelayed());
           }
       }

       // 延迟后应用选色器位置
       private IEnumerator ApplyColorPickerPositionsDelayed()
       {
           // 等待选色器实例初始化
           yield return new WaitForSeconds(0.1f);
           
           if (ColorPicker.Instance != null)
           {
               foreach (var positionData in colorPickerPositions)
               {
                   ColorPicker.Instance.SetPartPosition(positionData.partType, positionData.position);
                   Debug.Log($"Applied color picker position for {positionData.partType}: {positionData.position}");
               }
           }
           else
           {
               Debug.LogWarning("ColorPicker.Instance is null. Cannot apply color picker positions.");
           }
       }
       
       #endregion

       #region Data Saving
       
       // 将当前皮肤数据保存到组件中
       public void SaveCurrentSkinData()
       {
           if (partsManager == null) return;

           ClearAllData();
           SavePartData();
           SaveColorData();
           SaveColorPickerPositionData();
       }

       // 清除所有数据
       private void ClearAllData()
       {
           skinParts.Clear();
           slotColors.Clear();
           colorPickerPositions.Clear();
       }

       // 保存部件数据
       private void SavePartData()
       {
           foreach (var kvp in partsManager.ActiveIndices)
           {
               bool isHidden = IsPartHidden(kvp.Key);

               var partData = new SkinPartData
               {
                   partType = kvp.Key,
                   selectedIndex = kvp.Value,
                   isHidden = isHidden
               };
               skinParts.Add(partData);
           }
       }

       // 保存颜色数据
       private void SaveColorData()
       {
           AddColorData("hair", partsManager.GetColorBySlotType("hair"));
           AddColorData("beard", partsManager.GetColorBySlotType("beard"));
           AddColorData("brow", partsManager.GetColorBySlotType("brow"));
           AddColorData("body", partsManager.GetColorBySlotType("body"));
       }

       // 保存选色器位置数据
       private void SaveColorPickerPositionData()
       {
           if (ColorPicker.Instance != null)
           {
               AddColorPickerPositionData(PartsType.Hair_Short);
               AddColorPickerPositionData(PartsType.Beard);
               AddColorPickerPositionData(PartsType.Brow);
               AddColorPickerPositionData(PartsType.Skin);
           }
       }
       
       #endregion

       #region Utility Methods
       
       // 检查部件是否隐藏
       private bool IsPartHidden(PartsType partType)
       {
           // 使用公共 API 而非反射
           return partsManager != null && partsManager.IsPartHidden(partType);
       }

       // 添加颜色数据
       private void AddColorData(string slotPrefix, Color color)
       {
           slotColors.Add(new SlotColorData
           {
               slotName = slotPrefix,
               color = color
           });
       }

       // 保存选色器位置数据
       private void AddColorPickerPositionData(PartsType partType)
       {
           if (ColorPicker.Instance != null)
           {
               var position = ColorPicker.Instance.GetPartPosition(partType);
               if (position.x >= 0) // 仅当位置值有效时保存
               {
                   colorPickerPositions.Add(new ColorPickerPositionData
                   {
                       partType = partType,
                       position = position
                   });
               }
           }
       }
       
       #endregion

       #region Public Accessors
       
       // 获取保存的选色器位置
       public Vector2 GetSavedColorPickerPosition(PartsType partType)
       {
           foreach (var positionData in colorPickerPositions)
           {
               if (positionData.partType == partType)
               {
                   return positionData.position;
               }
           }
           return new Vector2(-1, -1); // 表示没有保存的位置
       }

       // 设置选色器位置
       public void SetColorPickerPosition(PartsType partType, Vector2 position)
       {
           // 移除旧数据
           RemoveExistingPositionData(partType);

           // 添加新数据
           AddNewPositionData(partType, position);
       }

       // 移除现有的位置数据
       private void RemoveExistingPositionData(PartsType partType)
       {
           for (int i = colorPickerPositions.Count - 1; i >= 0; i--)
           {
               if (colorPickerPositions[i].partType == partType)
               {
                   colorPickerPositions.RemoveAt(i);
               }
           }
       }

       // 添加新的位置数据
       private void AddNewPositionData(PartsType partType, Vector2 position)
       {
           if (position.x >= 0)
           {
               colorPickerPositions.Add(new ColorPickerPositionData
               {
                   partType = partType,
                   position = position
               });
           }
       }
       
       #endregion
   }
}