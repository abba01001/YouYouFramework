using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TestProcedure : MonoBehaviour
{
    void Update()
    {
		if (Input.GetKeyUp(KeyCode.A))
		{
			GameEntry.Procedure.ChangeState(ProcedureState.None);
		}
	}
}