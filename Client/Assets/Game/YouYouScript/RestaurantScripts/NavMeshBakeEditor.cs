#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.AI;

[ExecuteInEditMode]
public class NavMeshBakeEditor : MonoBehaviour
{
#if UNITY_EDITOR

    public BuildingBase[] building;
    private int i = 0;
    public bool activeObjects;
    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        i = 0;
        building = FindObjectsOfType<BuildingBase>();
        foreach (BuildingBase buyPoint in building)
        {
            // if (activeObjects)
            // {
            //     buyPoint.objectToUnlock.MSetActive(true);
            // }
            // else
            // {
            //     buyPoint.objectToUnlock.MSetActive(false);
            // }

            // buyPoint.gameObject.name = "BuyPoint " + i;
            i++;
            // buyPoint.srNo = i;

            PrefabUtility.RecordPrefabInstancePropertyModifications(buyPoint.gameObject);
            EditorUtility.SetDirty(buyPoint);
        }
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }
#endif

}
