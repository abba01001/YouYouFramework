using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using YouYou;

public class GuildPanel : PanelBase
{
    protected override void OnAwake()
    {
        base.OnAwake();
        CurPanelName = "GuildPanel";
    }
}