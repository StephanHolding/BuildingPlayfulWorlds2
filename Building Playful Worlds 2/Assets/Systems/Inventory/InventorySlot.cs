using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
	public Image slotImage;

	private InventoryManager inventoryManager;
	protected InventoryItem holdingItem;
	private Button slotButton;
	private int slotIndex;

	public void Init(InventoryManager inventoryManager, int slotIndex)
	{
		this.slotIndex = slotIndex;
		this.inventoryManager = inventoryManager;
		slotButton = GetComponent<Button>();

		slotButton.onClick.AddListener(delegate { InventorySlotWasClicked(); });
	}

	public void PlaceItemInThisSlot(InventoryItem item)
	{
		holdingItem = item;
		UpdateSlot();
	}

	public void ClearSlot()
	{
		holdingItem = null;
		UpdateSlot();
	}

	private void UpdateSlot()
	{
		if (holdingItem != null)
		{
			slotImage.sprite = holdingItem.inventoryIcon;
			slotImage.color = new Color(1, 1, 1, 1);
		}
		else
		{
			slotImage.sprite = null;
			slotImage.color = new Color(0, 0, 0, 0);
		}
	}

	private void InventorySlotWasClicked()
	{
		inventoryManager.InventorySlotWasClicked(slotIndex);
	}

}
