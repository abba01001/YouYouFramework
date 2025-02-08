using UnityEngine;
using UnityEditor;

public class ObjectPathPrinter : MonoBehaviour
{
    [MenuItem("Tools/打印窗口物体路径")]
    public static void PrintSelectedObjectPath()
    {
        // 获取选中的物体
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject != null)
        {
            // 打印路径
            string path = GetObjectPath(selectedObject);
            Debug.Log("Selected Object Path: " + path);
        }
        else
        {
            Debug.LogWarning("No object selected!");
        }
    }

    // 递归获取物体路径
    private static string GetObjectPath(GameObject obj)
    {
        string path = obj.name;

        // 如果物体有父物体，则递归获取父物体的路径
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = obj.name + "/" + path;
        }

        return path;
    }
}