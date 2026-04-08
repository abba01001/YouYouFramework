using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 日志分类
/// </summary>
public enum LogCategory
{
    Framework,
    Procedure,
    Loader,
    NetWork,
    Guide,
    UI,
    Audio,
    Scene,
    Pool,
}

/// <summary>
/// UI窗口的显示类型
/// </summary>
public enum UIFormShowMode
{
    Normal = 0,

    /// <summary>
    /// 反切
    /// </summary>
    ReverseChange = 1,
}