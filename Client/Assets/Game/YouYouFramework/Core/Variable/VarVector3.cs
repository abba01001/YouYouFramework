using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VarVector3 : Variable<Vector3>
{
    /// <summary>
    /// ����һ������
    /// </summary>
    /// <returns></returns>
    public static VarVector3 Alloc()
    {
        VarVector3 var = GameEntry.Pool.DequeueVarObject<VarVector3>();
        var.Value = Vector3.zero;
        ;
        var.Retain();
        return var;
    }

    /// <summary>
    /// ����һ������
    /// </summary>
    /// <param name="value">��ʼֵ</param>
    /// <returns></returns>
    public static VarVector3 Alloc(VarVector3 value)
    {
        VarVector3 var = Alloc();
        var.Value = value;
        return var;
    }

    /// <summary>
    /// VarString -> string
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Vector3(VarVector3 value)
    {
        return value.Value;
    }
}