using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySlot_ActiveItem : InventorySlot
{
	public string activationButton;

	private KeyCode activationKeyCode;

	private void Awake()
	{
		activationKeyCode = (KeyCode)System.Enum.Parse(typeof(KeyCode), activationButton);
	}

	private void Update()
	{
		if (Input.GetKeyDown(activationKeyCode))
		{

		}
	}

}
