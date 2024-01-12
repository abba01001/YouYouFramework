using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;

public class NewBehaviourScript : MonoBehaviour
{
    [MenuItem("Assets/UI/ReplaceAllFontsToNormal")]
    public static void ReplaceAllFontsToNormal()
    {
        var normal_font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Res/Fonts/FZY4JW.ttf");
        if (normal_font == null)
        {
            Debug.LogWarning("Can't Find normal font in path: Assets/Res/Fonts/FZY4JW.otf");
            return;
        }
        GameObject[] gos = Selection.gameObjects;
        foreach (var obj in gos)
        {
            if (obj == null)
            {
                break;
            }
            var instance_obj = GameObject.Instantiate(obj);
            foreach (var text_comp in instance_obj.GetComponentsInChildren<Text>(true))
            {
                if (!text_comp.font || (text_comp.font.dynamic && text_comp.font.name != "FZY4JW"))
                {
                    text_comp.font = normal_font;
                }
            }
            var path = AssetDatabase.GetAssetPath(obj);
            PrefabUtility.ReplacePrefab(instance_obj, obj);
            AssetDatabase.ImportAsset(path);
            GameObject.DestroyImmediate(instance_obj);
        }
        AssetDatabase.SaveAssets();
    }
}
