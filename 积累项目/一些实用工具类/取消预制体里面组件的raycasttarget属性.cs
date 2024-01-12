using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;

public class NewBehaviourScript : MonoBehaviour
{
    [MenuItem("Assets/UI/CancelAllRaycastTarget")]
    public static void CancelAllRaycastTarget()
    {
        GameObject[] gos = Selection.gameObjects;
        foreach (var obj in gos)
        {
            if (obj == null)
            {
                continue;
            }
            var instance_obj = GameObject.Instantiate(obj);
            foreach (var text_comp in instance_obj.GetComponentsInChildren<Text>(true))
            {
                text_comp.raycastTarget = false;
            }
            foreach (var image_comp in instance_obj.GetComponentsInChildren<Image>(true))
            {
                if (image_comp.gameObject.GetComponent<Button>())
                {
                    continue;
                }
                else if (image_comp.gameObject.GetComponent<InputField>())
                {
                    continue;
                }
                //else if (image_comp.gameObject.GetComponent<UIButtonTrigger>())
                //{
                //    continue;
                //}
                else if (image_comp.gameObject.GetComponent<ScrollRect>())
                {
                    continue;
                }
                else if (image_comp.gameObject.GetComponent<Toggle>())
                {
                    continue;
                }
                //else if (image_comp.gameObject.GetComponent<UIDraggable>())
                //{
                //    continue;
                //}
                image_comp.raycastTarget = false;
            }
            var path = AssetDatabase.GetAssetPath(obj);
            PrefabUtility.ReplacePrefab(instance_obj, obj);
            AssetDatabase.ImportAsset(path);
            GameObject.DestroyImmediate(instance_obj);
        }
        AssetDatabase.SaveAssets();
    }
}
