
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IPlayerTouchable
{
	[Header("Inventory Item")]
	public Sprite inventoryIcon;
	public Ability ability;

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

	public void OnTouchedByPlayer(Player player)
	{
		if (player != null)
		{
			AddMyselfToInventory();
		}
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

	public void InvokeAbility()
	{
		if (ability != null)
		{
			ability.InvokeAbility();
			DeleteItem();
		}
	}

	private void DeleteItem()
	{
		InventoryManager.instance.RemoveItemFromInventory(this);
		Destroy(gameObject);
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
