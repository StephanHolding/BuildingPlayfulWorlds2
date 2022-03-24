using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : SingletonTemplateMono<InputHandler>
{

	public LayerMask clickableLayers;

	private Camera mainCam;

	protected override void Awake()
	{
		base.Awake();
		mainCam = Camera.main;
	}

	private void Update()
	{
		MouseRaycast();
		InventoryButton();
	}

	private void MouseRaycast()
	{
		if (Input.GetMouseButtonDown(0))
		{
			RaycastHit2D hit = Physics2D.Raycast(mainCam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 20, clickableLayers);

			if (hit.collider != null)
			{
				IClickable clickable = hit.collider.GetComponent<IClickable>();

				if (clickable != null)
				{
					clickable.OnClick();
				}
			}
		}
	}
	
	private void InventoryButton()
	{
		if (Input.GetKeyDown(KeyCode.I))
		{
			UIManager.instance.ToggleWindow("Inventory Window");
		}
	}
}
