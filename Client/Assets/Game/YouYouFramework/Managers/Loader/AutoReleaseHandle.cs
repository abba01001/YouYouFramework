using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class AutoReleaseHandle : MonoBehaviour
{
    private List<AssetReferenceEntity> releaseList = new List<AssetReferenceEntity>();

    public static void Add(AssetReferenceEntity referenceEntity, GameObject target)
    {
        if (target == null)
        {
            GameObject SceneRoot = GameEntry.Pool.GameObjectPool.YouYouObjPool;
            target = SceneRoot;
            GameEntry.Log(LogCategory.Loader, "因为{0}没有可绑定的target， 所以绑定到了{1}上， 随当前场景销毁而减少引用计数", referenceEntity.AssetFullPath, SceneRoot);
        }

        if (target != null)
        {
            AutoReleaseHandle handle = target.GetComponent<AutoReleaseHandle>();
            if (handle == null)
            {
                handle = target.AddComponent<AutoReleaseHandle>();
            }
            handle.releaseList.Add(referenceEntity);
            referenceEntity.ReferenceAdd();
        }
    }

    private void OnDestroy()
    {
        foreach (AssetReferenceEntity referenceEntity in releaseList)
        {
            referenceEntity.ReferenceRemove();
        }
    }
}
