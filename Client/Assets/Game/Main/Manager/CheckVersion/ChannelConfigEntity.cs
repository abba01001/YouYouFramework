using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 渠道配置
/// </summary>
public class ChannelConfigEntity
{
    /// <summary>
    /// 渠道号
    /// </summary>
    public short ChannelId = 146;

    /// <summary>
    /// 服务器时间
    /// </summary>
    public long ServerTime;

    /// <summary>
    /// 资源版本号
    /// </summary>
    public string SourceVersion = "";

    public string DefaultVersion = "";
    /// <summary>
    /// APK版本号
    /// </summary>
    public string APKVersion = "";
    /// <summary>
    /// 资源地址
    /// </summary>
    public string SourceUrl = "https://abab01001-1318826377.cos.ap-guangzhou.myqcloud.com/Unity/AssetBundle/";
    public string GameDataUrl = "https://abab01001-1318826377.cos.ap-guangzhou.myqcloud.com/Unity/GameData/";
    public string APKVersionUrl = "https://abab01001-1318826377.cos.ap-guangzhou.myqcloud.com/APK/APKVersion.txt";
    public string ApkUrl = "https://abab01001-1318826377.cos.ap-guangzhou.myqcloud.com/APK/{0}.apk";
    #region RealSourceUrl 真正的资源地址
    private string m_RealSourceUrl;
    /// <summary>
    /// 真正的资源地址
    /// </summary>
    public string RealSourceUrl
    {
        get
        {
            if (string.IsNullOrEmpty(m_RealSourceUrl))
            {
                string buildTarget = string.Empty;

#if UNITY_STANDALONE_WIN
                buildTarget = "Windows";
#elif UNITY_ANDROID
				buildTarget = "Android";
#elif UNITY_IPHONE
                buildTarget = "iOS";
#endif
                m_RealSourceUrl = string.Format("{0}{1}/{2}/", SourceUrl, SourceVersion, buildTarget);
            }
            return m_RealSourceUrl;
        }
    }

    #endregion
}
