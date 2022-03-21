using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PathfindingTile : MonoBehaviour
{

	public bool markedWalkable;
	public Vector2Int placeInDictionary;
	public PathfindingTile previousTileInPath;

	internal float distanceToEnd;
	internal float distanceSinceStart;
	internal float totalCost;

	internal bool canBeWalkedOn;

	private Color originalColor;
	private SpriteRenderer spriteRenderer;
	private UnityAction onClick;

	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		originalColor = spriteRenderer.color;
	}

	private void OnMouseDown()
	{
		if (onClick != null)
		{
			onClick.Invoke();
		}
	}

	public void AddToOnClick(UnityAction call)
	{
		onClick += call;
	}

	public void RemoveListenersFromOnClick()
	{
		onClick = null;
	}

	public void SetPlaceInDictionary(Vector2Int newPlace)
	{
		placeInDictionary = newPlace;
	}

	public void SetColor(Color newColor)
	{
		spriteRenderer.color = newColor;
	}

	public float CalculateTotalCost()
	{
		totalCost = distanceToEnd + distanceSinceStart;
		return totalCost;
	}

	[ContextMenu("Calculate path to me")]
	public void CalculatePathToMe()
	{
		DungeonManager.instance.StartCalculatingPath(placeInDictionary);
	}

	[ContextMenu("Set Starttile")]
	public void SetStartTileToMe()
	{
		DungeonManager.instance.startTile = placeInDictionary;
	}

}
