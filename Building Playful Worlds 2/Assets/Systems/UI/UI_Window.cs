using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Window : MonoBehaviour
{
	public string windowName;

	protected virtual void Awake()
	{
		if (windowName == null || windowName == string.Empty)
		{
			windowName = gameObject.name;
		}

		UIManager.instance.AddWindowToDictionary(this);
	}

	public void ShowThisWindow()
	{
		gameObject.SetActive(true);
		OnWindowShown();
	}

	protected virtual void OnWindowShown() { }

	public void HideThisWindow()
	{
		gameObject.SetActive(false);
		OnWindowHidden();
	}

	protected virtual void OnWindowHidden() { }
}
