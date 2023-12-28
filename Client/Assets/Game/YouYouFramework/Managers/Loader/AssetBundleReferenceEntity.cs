using Main;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// AssetBundle引用计数实体
    /// </summary>
    public class AssetBundleReferenceEntity
    {
        /// <summary>
        /// 资源包路径
        /// </summary>
        public string AssetBundlePath;

        /// <summary>
        /// 关联目标
        /// </summary>
        public AssetBundle Target;

        /// <summary>
        /// 上次使用时间
        /// </summary>
        public float LastUseTime { get; private set; }

        /// <summary>
        /// 引用计数
        /// </summary>
        public int ReferenceCount { get; private set; }


        /// <summary>
        /// 刷新最后使用时间
        /// </summary>
        public void RefeshLastUseTime()
        {
            LastUseTime = Time.time;
        }

        /// <summary>
        /// 引用计数+1
        /// </summary>
        public void ReferenceAdd()
        {
            ReferenceCount++;
        }
        /// <summary>
        /// 引用计数-1
        /// </summary>
        public void ReferenceRemove()
        {
            RefeshLastUseTime();
            ReferenceCount--;
            if (ReferenceCount < 0)
            {
                GameEntry.LogError(LogCategory.Loader, "AB引用计数出错， ReferenceCount==" + ReferenceCount);
            }
        }

        /// <summary>
        /// 对象是否可以释放
        /// </summary>
        /// <returns></returns>
        public bool GetCanRelease()
        {
            return ReferenceCount == 0 && Time.time - LastUseTime > GameEntry.Pool.ReleaseAssetBundleInterval;
        }

        public static AssetBundleReferenceEntity Create(string path, AssetBundle target)
        {
            if (target == null)
            {
                return null;
            }

            AssetBundleReferenceEntity assetBundleEntity = MainEntry.ClassObjectPool.Dequeue<AssetBundleReferenceEntity>();
            assetBundleEntity.AssetBundlePath = path;
            assetBundleEntity.Target = target;
            GameEntry.Pool.AssetBundlePool.Register(assetBundleEntity);

            //如果是锁定资源包, 默认引用计数就是1
            if (GameEntry.Pool.CheckAssetBundleIsLock(path))
            {
                assetBundleEntity.ReferenceAdd();
            }

            return assetBundleEntity;
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Release()
        {
            AssetBundle bundle = Target;
            bundle.Unload(true);

            AssetBundlePath = null;
            Target = null;

            MainEntry.ClassObjectPool.Enqueue(this); //把这个资源实体回池
        }
    }
}