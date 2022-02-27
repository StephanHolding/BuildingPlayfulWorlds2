using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
	[Header("Inventory Item")]
	public Sprite inventoryIcon;

	private Renderer[] renderers;

	protected virtual void Awake()
	{
		renderers = GetRelevantRenderers();
	}

	[ContextMenu("Add Myself To Inventory")]
	public void AddMyselfToInventory()
	{
		InventoryManager.instance.AddItemToInventory(this);
	}

	public void OnAddedToInventory(Transform inventoryOwner)
	{
		ToggleWorldRepresentation(false);
		transform.SetParent(inventoryOwner, false);
	}

	public void OnEjectedFromInventory()
	{
		ToggleWorldRepresentation(false);
		transform.SetParent(null, false);
	}

	protected virtual Renderer[] GetRelevantRenderers()
	{
		return GetComponentsInChildren<Renderer>();
	}

	private void ToggleWorldRepresentation(bool toggle)
	{
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i].enabled = toggle;
		}
	}

}
