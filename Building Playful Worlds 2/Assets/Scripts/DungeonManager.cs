using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : SingletonTemplateMono<DungeonManager>
{
	private Dictionary<Vector2, PathfindingTile> walkableTiles = new Dictionary<Vector2, PathfindingTile>();

	protected override void Awake()
	{
		base.Awake();

		walkableTiles = GetWalkableTiles();
	}

	public List<PathfindingTile> FindPath(Vector2 startPosition, Vector2 endPosition)
	{
		List<PathfindingTile> openList = new List<PathfindingTile>();
		List<PathfindingTile> closedList = new List<PathfindingTile>();

		openList.Add(walkableTiles[startPosition]);

		while(openList.Count > 0)
		{
			PathfindingTile current = GetTileWithLowestTotalCost(openList);

			openList.Remove(current);
			closedList.Add(current);

			if (current.placeInDictionary == endPosition)
			{
				print("we did it");
				return RetracePath(walkableTiles[startPosition], current);
			}

			PathfindingTile[] neighbors = GetNeighborTiles(current.placeInDictionary);

			foreach (PathfindingTile neighbor in neighbors)
			{
				if (closedList.Contains(neighbor)) continue;

				float costToThisNeighbor = current.distanceSinceStart + GetDistance(current.placeInDictionary, neighbor.placeInDictionary);
				if (costToThisNeighbor < neighbor.distanceSinceStart || !openList.Contains(neighbor))
				{
					neighbor.distanceSinceStart = costToThisNeighbor;
					neighbor.distanceToEnd = GetDistance(neighbor.placeInDictionary, endPosition);
					neighbor.previousTileInPath = current;

					openList.Add(neighbor);

					neighbor.SetColor(Color.magenta);
				}
			}
		}

		Debug.LogWarning("No path was found");
		return null;
	}

	internal void StartCalculatingPath(Vector2 placeInDictionary)
	{
		FindPath(new Vector2(-2.5f, -3.5f), placeInDictionary);
	}

	private Dictionary<Vector2, PathfindingTile> GetWalkableTiles()
	{
		Dictionary<Vector3, PathfindingTile> allTiles = new Dictionary<Vector3, PathfindingTile>();
		Dictionary<Vector2, PathfindingTile> unorderedWalkableTiles = new Dictionary<Vector2, PathfindingTile>();
		PathfindingTile[] allPathfindingTiles = FindObjectsOfType<PathfindingTile>();

		foreach (PathfindingTile tile in allPathfindingTiles)
		{
			Vector3 key = new Vector3(tile.transform.position.x, tile.transform.position.y, tile.markedWalkable ? 1 : 0);
			if (!allTiles.ContainsKey(key))
				allTiles.Add(key, tile);
		}

		foreach (KeyValuePair<Vector3, PathfindingTile> tileWithKey in allTiles)
		{
			Vector3 potentialSiblingTileKey = new Vector3(tileWithKey.Key.x, tileWithKey.Key.y, tileWithKey.Key.z == 0 ? 1 : 0);

			if (!allTiles.ContainsKey(potentialSiblingTileKey) && tileWithKey.Key.z == 1)
			{
				unorderedWalkableTiles.Add(tileWithKey.Key, tileWithKey.Value);
				tileWithKey.Value.SetPlaceInDictionary(tileWithKey.Key);
			}
		}

		return unorderedWalkableTiles;
	}

	private List<PathfindingTile> RetracePath(PathfindingTile startTile, PathfindingTile endTile)
	{
		List<PathfindingTile> toReturn = new List<PathfindingTile>();
		PathfindingTile current = endTile;
		while(current != startTile)
		{
			toReturn.Add(current);
			current.SetColor(Color.cyan);
			current = current.previousTileInPath;
		}

		toReturn.Reverse();
		return toReturn;
	}

	private PathfindingTile GetTileWithLowestTotalCost(List<PathfindingTile> openList)
	{
		float currentLowestCost = Mathf.Infinity;
		PathfindingTile toReturn = null;

		for (int i = 0; i < openList.Count; i++)
		{
			float totalCost = openList[i].CalculateTotalCost();

			if (totalCost < currentLowestCost)
			{
				currentLowestCost = totalCost;
				toReturn = openList[i];
			}
		}

		return toReturn;
	}

	private PathfindingTile[] GetNeighborTiles(Vector2 currentKey)
	{
		List<PathfindingTile> toReturn = new List<PathfindingTile>();

		if (walkableTiles.ContainsKey(currentKey + new Vector2(1, 0)))
			toReturn.Add(walkableTiles[currentKey + new Vector2(1, 0)]);

		if (walkableTiles.ContainsKey(currentKey + new Vector2(-1, 0)))
			toReturn.Add(walkableTiles[currentKey + new Vector2(-1, 0)]);

		if (walkableTiles.ContainsKey(currentKey + new Vector2(0, 1)))
			toReturn.Add(walkableTiles[currentKey + new Vector2(0, 1)]);

		if (walkableTiles.ContainsKey(currentKey + new Vector2(0, -1)))
			toReturn.Add(walkableTiles[currentKey + new Vector2(0, -1)]);

		return toReturn.ToArray();
	}

	private float GetDistance(Vector2 tile1, Vector2 tile2)
	{
		float distanceX = Mathf.Abs(tile1.x - tile2.x);
		float distanceY = Mathf.Abs(tile1.y - tile2.y);
		return distanceX + distanceY;
	}
}
