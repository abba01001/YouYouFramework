using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Main
{
    /// <summary>
    /// AssetBundle版本文件信息
    /// </summary>
    public class VersionFileEntity
    {
        /// <summary>
        /// 资源包名称
        /// </summary>
        public string AssetBundleName;

        /// <summary>
        /// MD5码
        /// </summary>
        public string MD5;

        /// <summary>
        /// 文件大小(字节)
        /// </summary>
        public ulong Size;

        /// <summary>
        /// 是否初始数据
        /// </summary>
        public bool IsFirstData;

        /// <summary>
        /// 是否已经加密
        /// </summary>
        public bool IsEncrypt;
        
        public override string ToString()
                {
                    return $"AssetBundleName: {AssetBundleName}, MD5: {MD5}, Size: {Size}, IsFirstData: {IsFirstData}, IsEncrypt: {IsEncrypt}";
                }

    }
}