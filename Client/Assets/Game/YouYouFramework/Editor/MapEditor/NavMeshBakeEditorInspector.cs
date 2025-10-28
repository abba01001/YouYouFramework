#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NavMeshBakeEditor))]
public class NavMeshBakeEditorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        // 绘制默认 Inspector
        DrawDefaultInspector();

        // 获取目标对象
        NavMeshBakeEditor script = (NavMeshBakeEditor)target;

        // 加个空行美化一下
        EditorGUILayout.Space(10);

        // 加按钮
        if (GUILayout.Button("更新所有的BuyPoint"))
        {
            // script.Refresh(); // 调用你类里的方法
        }

        // // 你还可以加更多按钮
        // if (GUILayout.Button("重新查找 BuyPoints"))
        // {
        //     script.buyPoints = Object.FindObjectsOfType<BuyPoint>();
        //     Debug.Log("🔍 重新查找完毕，共找到: " + script.buyPoints.Length + " 个");
        // }
    }
}
#endif