using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEditor.Animations;

public class ModelTools {
    [MenuItem("Assets/Model/GeneratePrefab")]
    public static void GeneratePrefab() {
        GameObject go = Selection.activeGameObject;
        if (go == null) {
            return;
        }
        Animation anim = go.GetComponent<Animation>();
        if (anim != null && anim.clip == null)
        {
            Debug.LogError("模型没有设置默认动作");
        }
        string prefab_name = go.name + "_pb";
        GameObject pb_go = new GameObject();
        pb_go.name = prefab_name;
        UnityEngine.AI.NavMeshAgent nav = pb_go.AddComponent<UnityEngine.AI.NavMeshAgent>();
        nav.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.NoObstacleAvoidance;
        nav.avoidancePriority = 0;
        GameObject mesh_go = GameObject.Instantiate(go);
        mesh_go.name = "model";
        mesh_go.transform.SetParent(pb_go.transform);

        pb_go.AddComponent<CapsuleCollider>();
        pb_go.AddComponent<UnitTrigger>();

        string path = AssetDatabase.GetAssetPath(go);
        int i = path.LastIndexOf("/");
        path = path.Substring(0, i + 1) + prefab_name +".prefab";

        PrefabUtility.CreatePrefab(path, pb_go);
        GameObject.DestroyImmediate(pb_go);
        AssetDatabase.SaveAssets();
    }
   
}
