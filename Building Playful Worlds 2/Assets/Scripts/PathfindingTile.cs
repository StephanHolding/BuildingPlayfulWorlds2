using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		originalColor = spriteRenderer.color;
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
