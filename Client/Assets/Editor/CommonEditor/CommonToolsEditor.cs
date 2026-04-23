using Sirenix.OdinInspector;
using UnityEngine;
[CreateAssetMenu(menuName = "框架ScriptableObject/CommonToolsSettings")]
public class CommonToolsEditor : ScriptableObject
{
    [HorizontalGroup("Common", LabelWidth = 150)]
    [VerticalGroup("Common/Left")]
    [Button("GM面板",ButtonSizes.Medium)]
    public void Test1()
    {
        GMEditorPanel.OpenWindow();
    }

    [VerticalGroup("Common/Right")]
    [Button("生成多语言图集",ButtonSizes.Medium)]
    public void Test2()
    {
        TMPAtlasBuilder.ShowWindow();
    }
}
