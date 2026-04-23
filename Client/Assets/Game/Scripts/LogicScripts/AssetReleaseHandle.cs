using System.Collections.Generic;
using FrameWork;
using Main;
using UnityEngine;
using YooAsset;

/// <summary>
/// Asset池, 对象被销毁时, 自动让引用计数-1
/// </summary>
public class AssetReleaseHandle : MonoBehaviour
{
    private List<AssetHandle> releaseList = new();

    public static void Add(AssetHandle assetHandle, GameObject target)
    {
        if (target == null)
        {
            GameObject SceneRoot = GameEntry.Pool.GameObjectPool.ObjectPool;
            target = SceneRoot;
            Debugger.Log(string.Format("因为{0}没有可绑定的target， 所以绑定到了{1}上， 随当前场景销毁而减少引用计数", assetHandle.AssetObject, SceneRoot));
        }

        if (target != null)
        {
            AssetReleaseHandle handle = target.GetComponent<AssetReleaseHandle>();
            if (handle == null) handle = target.AddComponent<AssetReleaseHandle>();
            handle.releaseList.Add(assetHandle);
        }
    }

    private void OnDestroy()
    {
        foreach (var assetHandle in releaseList)
        {
            assetHandle.Release();
        }
    }
}
