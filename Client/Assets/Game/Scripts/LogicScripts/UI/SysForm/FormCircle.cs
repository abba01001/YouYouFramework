
using UnityEngine;
using System;

namespace GameScripts
{
    public class FormCircle : UIFormBase
    {
    	[SerializeField] private Transform circleTrans;
    	private void Update()
    	{
    		circleTrans.Rotate(0, 0, -5, Space.Self);
    	}
    }
}