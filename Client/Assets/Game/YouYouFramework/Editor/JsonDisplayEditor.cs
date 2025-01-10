using UnityEngine;
using UnityEditor;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(JsonDisplay))]
public class JsonDisplayEditor : Editor
{
    private JObject parsedJson;    // 用于存储解析后的JSON对象
    private Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>(); // 存储每个节点的展开状态
    private List<string> SpecialKeys = new List<string> {"ActivitySaveData"}; // 指定的特殊键名

    public override void OnInspectorGUI()
    {
        // 绘制默认的Inspector
        DrawDefaultInspector();

        // 获取目标脚本
        JsonDisplay jsonDisplay = (JsonDisplay)target;
        parsedJson = JObject.Parse(jsonDisplay.jsonString);

        // 显示/隐藏JSON
        if (parsedJson != null)
        {
            DisplayJson(parsedJson, "");
        }
    }

    // 递归显示JSON的函数，按属性名排序
    private void DisplayJson(JToken token, string path)
    {
        if (token is JObject obj)
        {
            // 按名字对属性进行排序
            foreach (var property in obj.Properties().OrderBy(p => p.Name))
            {
                string propertyPath = path + "/" + property.Name;

                // 初始化展开状态
                if (!foldoutStates.ContainsKey(propertyPath))
                {
                    foldoutStates[propertyPath] = false;
                }

                // 使用Foldout来处理展开/收缩
                foldoutStates[propertyPath] = EditorGUILayout.Foldout(foldoutStates[propertyPath], property.Name);

                if (foldoutStates[propertyPath])
                {
                    EditorGUI.indentLevel++;
                    DisplayJson(property.Value, propertyPath); // 递归调用显示子项
                    EditorGUI.indentLevel--;
                }
            }
        }
        else if (token is JArray array)
        {
            for (int i = 0; i < array.Count; i++)
            {
                string arrayPath = path + "[" + i + "]";

                // 初始化展开状态
                if (!foldoutStates.ContainsKey(arrayPath))
                {
                    foldoutStates[arrayPath] = false;
                }

                // 使用Foldout来处理展开/收缩
                foldoutStates[arrayPath] = EditorGUILayout.Foldout(foldoutStates[arrayPath], $"Element {i}");

                if (foldoutStates[arrayPath])
                {
                    EditorGUI.indentLevel++;
                    DisplayJson(array[i], arrayPath); // 递归调用显示子项
                    EditorGUI.indentLevel--;
                }
            }
        }
        else
        {
            // 当前路径上的属性名，如果它是目标key，则进行解析
            string value = token.ToString();
            string[] pathSegments = path.Split('/');
            string currentKey = pathSegments.Length > 0 ? pathSegments.Last() : "";

            // 检查当前键是否为指定的特殊键（例如AAA）
            if (SpecialKeys.Contains(currentKey) && IsJsonString(value))
            {
                string propertyPath = path + "/innerJson";

                // 初始化展开状态
                if (!foldoutStates.ContainsKey(propertyPath))
                {
                    foldoutStates[propertyPath] = false;
                }

                // 使用Foldout来处理嵌套的JSON展开/收缩
                foldoutStates[propertyPath] = EditorGUILayout.Foldout(foldoutStates[propertyPath], "Parsed JSON");

                if (foldoutStates[propertyPath])
                {
                    EditorGUI.indentLevel++;
                    DisplayJson(JToken.Parse(value), propertyPath); // 递归调用显示嵌套的JSON
                    EditorGUI.indentLevel--;
                }
            }
            else
            {
                // 显示普通字符串值，并自动换行
                GUILayout.Label(value, EditorStyles.wordWrappedLabel);
            }
        }
    }

    // 辅助函数：检测一个字符串是否可以解析为JSON
    private bool IsJsonString(string value)
    {
        value = value.Trim();
        if ((value.StartsWith("{") && value.EndsWith("}")) || // 判断是否为对象
            (value.StartsWith("[") && value.EndsWith("]")))   // 判断是否为数组
        {
            try
            {
                JToken.Parse(value);
                return true;
            }
            catch
            {
                // 解析失败，说明不是有效的JSON
                return false;
            }
        }
        return false;
    }
}
