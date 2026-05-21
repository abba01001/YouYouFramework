using System.Text;
using DG.Tweening.Core;
using UnityEditor;
using UnityEngine;

public static class SelectionTools
{
    [MenuItem("Tools/Log Selected Names %g")] // %g 代表 Ctrl+G
    private static void LogNames()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("未选中任何物体！");
            return;
        }

        Debug.Log($"<color=#00FF00>--- 选中了 {selectedObjects.Length} 个物体 ---</color>");
        StringBuilder nameBuilder = new StringBuilder();
        StringBuilder pathsBuilder = new StringBuilder();
        StringBuilder speedBuilder = new StringBuilder();
        StringBuilder damageBuilder = new StringBuilder();
        StringBuilder hpBuilder = new StringBuilder();
        StringBuilder enemyTypeBuilder = new StringBuilder();
        StringBuilder lifeTimeBuilder = new StringBuilder();
        
        foreach (var obj in selectedObjects)
        {
            // 打印名字，点击 Log 可以直接在 Hierarchy 中高亮该物体
            Debug.Log($"GameObject Name: <b>{obj.name}</b>", obj);
            nameBuilder.AppendLine(obj.name);
            string path = AssetDatabase.GetAssetPath(obj);
            pathsBuilder.AppendLine(path);
            
            var behavior = obj.GetComponent("EnemyBehavior");
            if (behavior != null)
            {
                // 使用 SerializedObject 访问私有字段
                SerializedObject so = new SerializedObject(behavior);
                // 查找属性
                var propSpeed = so.FindProperty("speed");
                var propDamage = so.FindProperty("damage");
                var propHP = so.FindProperty("hp");
                var enemyType = so.FindProperty("enemyType");

                if (propSpeed != null) speedBuilder.AppendLine(propSpeed.floatValue.ToString());
                if (propDamage != null) damageBuilder.AppendLine(propDamage.floatValue.ToString());
                if (propHP != null) hpBuilder.AppendLine(propHP.floatValue.ToString());
                enemyTypeBuilder.AppendLine(enemyType.enumNames[enemyType.enumValueIndex]);
            }
        }
        Debug.LogError(nameBuilder.ToString());
        Debug.LogError(pathsBuilder.ToString());
        Debug.LogError(speedBuilder.ToString());
        Debug.LogError(damageBuilder.ToString());
        Debug.LogError(hpBuilder.ToString());
        Debug.LogError(enemyTypeBuilder.ToString());
    }
}