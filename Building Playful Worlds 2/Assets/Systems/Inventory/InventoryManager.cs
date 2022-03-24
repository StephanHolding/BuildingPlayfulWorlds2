using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : SingletonTemplateMono<InventoryManager>
{ 
	[System.Serializable]
	public class InventorySaveFile : SerializableData
	{
		public InventorySaveFile(string instanceIdentifier) : base(instanceIdentifier)
		{
			this.instanceIdentifier = instanceIdentifier;
		}

		public InventoryItem[] currentInventory;
	}

	public Transform inventoryOwner;
	public Image draggingItemImage;

	private InventoryItem[] currentInventory;
	private InventorySlot[] inventorySlots;

	private InventoryItem draggingItem = null;
	private int draggingItemOrigin = -1;
	private Coroutine draggingCoroutine;
	private const string SAVEFILE_IDENTIFIER = "InventoryFile";

	protected override void Awake()
	{
		base.Awake();
		inventorySlots = GetComponentsInChildren<InventorySlot>();
		InitializeSlots();
		currentInventory = InitializeCurrentInventory();
		draggingItemImage.enabled = false;

		SerializationManager.WantsToSave += SaveData;
	}

	private void OnDestroy()
	{
		SerializationManager.WantsToSave -= SaveData;
	}

	private void Start()
	{
		UpdateInventoryVisuals();
	}

	public bool AddItemToInventory(InventoryItem toAdd)
	{
		int emptySpaceIndex = FindEmptyInventorySpace();
		if (emptySpaceIndex == -1)
		{
			return false;
		}

		currentInventory[emptySpaceIndex] = toAdd;
		toAdd.OnAddedToInventory(inventoryOwner);
		UpdateInventoryVisuals();
		return true;
	}

	public void RemoveItemFromInventory(InventoryItem toRemove)
	{
		for (int i = 0; i < currentInventory.Length; i++)
		{
			if (currentInventory[i] == toRemove)
			{
				toRemove.OnEjectedFromInventory();
				currentInventory[i] = null;
				UpdateInventoryVisuals();
			}
		}
	}

	public void InventorySlotWasClicked(int clickedSlotIndex)
	{
		if (draggingItem == null)
		{
			if (currentInventory[clickedSlotIndex] != null)
			{
				draggingItem = currentInventory[clickedSlotIndex];
				draggingItemOrigin = clickedSlotIndex;
				currentInventory[clickedSlotIndex] = null;

				if (draggingCoroutine != null)
					StopCoroutine(draggingCoroutine);

				draggingCoroutine = StartCoroutine(DragItem());
			}
		}
		else
		{
			InventoryItem temp = currentInventory[clickedSlotIndex];
			currentInventory[clickedSlotIndex] = draggingItem;

			if (clickedSlotIndex != draggingItemOrigin)
				currentInventory[draggingItemOrigin] = temp;


			print(currentInventory[clickedSlotIndex]);
			print(draggingItem);

			draggingItem = null;
			draggingItemOrigin = -1;
		}

		UpdateInventoryVisuals();
	}

	private int FindEmptyInventorySpace()
	{
		for (int i = 0; i < currentInventory.Length; i++)
		{
			if (currentInventory[i] == null)
			{
				return i;
			}
		}

		return -1;
	}

	private void UpdateInventoryVisuals()
	{
		for (int i = 0; i < currentInventory.Length; i++)
		{
			if (currentInventory[i] != null)
				inventorySlots[i].PlaceItemInThisSlot(currentInventory[i]);
			else
				inventorySlots[i].ClearSlot();
		}
	}

	private void InitializeSlots()
	{
		for (int i = 0; i < inventorySlots.Length; i++)
		{
			inventorySlots[i].Init(this, i);
		}
	}

	private IEnumerator DragItem()
	{
		draggingItemImage.enabled = true;
		draggingItemImage.sprite = draggingItem.inventoryIcon;

		while (draggingItem != null)
		{
			draggingItemImage.transform.position = Input.mousePosition;
			yield return null;
		}

		draggingItemImage.enabled = false;
	}

	private void SaveData()
	{
		InventorySaveFile file = new InventorySaveFile(SAVEFILE_IDENTIFIER);
		file.currentInventory = currentInventory;
		SerializationManager.AddFileToSaveSlot(file);
	}

	private InventoryItem[] InitializeCurrentInventory()
	{
		InventorySaveFile saveFile = SerializationManager.GetDataByInstanceIdentifier<InventorySaveFile>(SAVEFILE_IDENTIFIER);

		if (saveFile != null)
			return saveFile.currentInventory;
		else
			return new InventoryItem[inventorySlots.Length];
	}

}
