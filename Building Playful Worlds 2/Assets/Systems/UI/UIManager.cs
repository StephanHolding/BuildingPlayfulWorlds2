using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : SingletonTemplateMono<UIManager>
{
	public string openWindowOnStart;

	private Dictionary<string, UI_Window> allWindows = new Dictionary<string, UI_Window>();

	private void Start()
	{
		HideAllWindows();

		if (openWindowOnStart != null && openWindowOnStart != string.Empty)
			ShowWindow(openWindowOnStart);
	}

	public void AddWindowToDictionary(UI_Window window)
	{
		allWindows.Add(window.windowName, window);
	}

	public UI_Window ShowWindow(string windowName)
	{
		allWindows[windowName].ShowThisWindow();
		return allWindows[windowName];
	}

	public void HideWindow(string windowName)
	{
		allWindows[windowName].HideThisWindow();
	}

	public void HideAllWindows()
	{
		foreach (KeyValuePair<string, UI_Window> window in allWindows)
		{
			window.Value.HideThisWindow();
		}
	}

}
