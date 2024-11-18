#if !No_Reporter
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ReporterManager : MonoBehaviour
{
	private bool showLogPanel = false;
	ReporterPanel _reporterPanel;
	Image ImageMask;
	public void Awake()
	{
		ImageMask = transform.Find("Canvas").GetComponent<Image>();
		_reporterPanel = gameObject.AddComponent<ReporterPanel>();
	}

	public void ShowLogPanel(bool state)
	{
		showLogPanel = state;
		ImageMask.enabled = state;
		if (state)
		{
			_reporterPanel.Show();
		}
		else
		{
			_reporterPanel.Close();
		}
	}

	public void WriteLogsToFile()
	{
		_reporterPanel.WriteLogsToFile();
	}
	
	void OnGUI()
	{
		if (showLogPanel)
		{
			_reporterPanel.OnGUIDraw();
		}
	}
}
#endif