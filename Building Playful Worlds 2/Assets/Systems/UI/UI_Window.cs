using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Window : MonoBehaviour
{
	public string windowName;
	public bool isActive { get; private set; }

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
		isActive = true;
		OnWindowShown();
	}

	protected virtual void OnWindowShown() { }

	public void HideThisWindow()
	{
		gameObject.SetActive(false);
		isActive = false;
		OnWindowHidden();
	}

	protected virtual void OnWindowHidden() { }
}
