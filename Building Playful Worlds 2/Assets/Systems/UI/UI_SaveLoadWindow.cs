using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SaveLoadWindow : UI_Window
{

	public void Save()
	{
		SerializationManager.SaveToSaveSlot(0);
	}


	public void Load()
	{
		DungeonManager.useSaveFile = true;

		if (SerializationManager.LoadSaveSlot(0))
		{
			SceneHandler.instance.ReloadCurrentScene();
		}
	}

	public void ReloadScene()
	{
		DungeonManager.useSaveFile = false;

		SceneHandler.instance.ReloadCurrentScene();
	}

}
