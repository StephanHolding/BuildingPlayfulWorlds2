using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class DungeonManager : SingletonTemplateMono<DungeonManager>
{
	private enum TileType
	{
		Floor,
		Wall
	}

	public class Room
	{
		private Vector2Int[] allRoomTiles;

		public Room(Vector2Int[] tiles)
		{
			this.allRoomTiles = tiles;
		}

		public Vector2Int ReturnRandomTile()
		{
			return allRoomTiles[Random.Range(0, allRoomTiles.Length)];
		}
	}

	[Header("Dungeon Generator")]
	public int roomAmount;
	public int corridorsPerRoom;
	public Vector2Int gridSize;
	public Vector2Int roomSizeMinMaxX;
	public Vector2Int roomSizeMinMaxY;
	public GameObject[] floorTiles;
	public GameObject[] wallTiles;

	[Header("Player and Enemy Spawning")]
	public GameObject playerPrefab;
	public GameObject enemyPrefab;
	public int enemyAmount;

	private Dictionary<Vector2Int, PathfindingTile> walkableTiles = new Dictionary<Vector2Int, PathfindingTile>();
	private Dictionary<Vector2Int, TileType> grid = new Dictionary<Vector2Int, TileType>();
	//private Dictionary<Vector2Int, SpriteRenderer> spawnedSprites = new Dictionary<Vector2Int, SpriteRenderer>();
	private List<Room> rooms = new List<Room>();


	private Vector2Int firstFloorTile;
	internal Vector2Int startTile;


	protected override void Awake()
	{
		base.Awake();

		SpawnDungeon();
		walkableTiles = GetWalkableTiles();
		SpawnPlayer();
		for (int i = 0; i < enemyAmount; i++)
		{
			SpawnEnemy();
		}
	}

	//Player and Enemy Spawning: --------------------------------------------------------------------------------------------------------

	private void SpawnPlayer()
	{
		Vector2Int spawnPoint = GetRandomWalkableTilePosition();
		GameObject newObject = Instantiate(playerPrefab, new Vector3(spawnPoint.x, spawnPoint.y, 0), Quaternion.identity);
		Player player = newObject.GetComponent<Player>();
		player.Init(spawnPoint);
	}

	private void SpawnEnemy()
	{
		Vector2Int spawnPoint = GetRandomWalkableTilePosition();
		GameObject newObject = Instantiate(enemyPrefab, new Vector3(spawnPoint.x, spawnPoint.y, 0), Quaternion.identity);
		Enemy enemy = newObject.GetComponent<Enemy>();
		enemy.Init(spawnPoint);
	}


	//Dungeon Generation: ---------------------------------------------------------------------------------------------------------------

	private void SpawnDungeon()
	{
		for (int i = 0; i < roomAmount; i++)
		{
			CreateClearing();
		}

		ConvertClearingsToRooms();
		CreateCorridors();
		FillRemainingTilesWithWall();
		InstantiateTiles();
	}

	private void CreateClearing()
	{
		int roomsizeX = Random.Range(roomSizeMinMaxX.x, roomSizeMinMaxX.y);
		int roomsizeY = Random.Range(roomSizeMinMaxY.x, roomSizeMinMaxY.y);

		int middlePointX = Random.Range(0 + roomsizeX, gridSize.x - roomsizeX);
		int middlePointY = Random.Range(0 + roomsizeY, gridSize.y - roomsizeY);

		int leftPointX = middlePointX - roomsizeX / 2;
		int leftPointY = middlePointY - roomsizeY / 2;

		for (int x = leftPointX; x < leftPointX + roomsizeX; x++)
		{
			for (int y = leftPointY; y < leftPointY + roomsizeY; y++)
			{
				if (!grid.ContainsKey(new Vector2Int(x, y)))
				{
					SetTileType(new Vector2Int(x, y), TileType.Floor);

					if (firstFloorTile == Vector2Int.zero)
					{
						firstFloorTile = new Vector2Int(x, y);
					}
				}

			}
		}
	}

	private void FillRemainingTilesWithWall()
	{
		for (int x = 0; x < gridSize.x; x++)
		{
			for (int y = 0; y < gridSize.y; y++)
			{
				if (!grid.ContainsKey(new Vector2Int(x, y)))
				{
					grid.Add(new Vector2Int(x, y), TileType.Wall);
				}
			}
		}
	}

	private void InstantiateTiles()
	{
		foreach (KeyValuePair<Vector2Int, TileType> kvp in grid)
		{
			GameObject newObject = Instantiate(GetRandomTile(kvp.Value), new Vector3(kvp.Key.x, kvp.Key.y, 0), Quaternion.identity);
			newObject.transform.SetParent(transform, false);
			//spawnedSprites.Add(kvp.Key, newObject.GetComponent<SpriteRenderer>());
		}
	}

	private GameObject GetRandomTile(TileType type)
	{
		switch (type)
		{
			case TileType.Floor: return floorTiles[Random.Range(0, floorTiles.Length)];
			case TileType.Wall: return wallTiles[Random.Range(0, wallTiles.Length)];
			default: return null;
		}
	}

	private void ConvertClearingsToRooms()
	{
		List<Vector2Int> tilesDone = new List<Vector2Int>();

		foreach (KeyValuePair<Vector2Int, TileType> kvp in grid)
		{
			if (kvp.Value == TileType.Floor)
			{
				if (!tilesDone.Contains(kvp.Key))
				{
					Vector2Int[] roomTiles = FindAllConnectedFloorTiles(kvp.Key);
					tilesDone.AddRange(roomTiles);

					Room newRoom = new Room(roomTiles);
					rooms.Add(newRoom);
				}
			}
		}
	}

	private Vector2Int[] FindAllConnectedFloorTiles(Vector2Int firstFloorTile)
	{
		List<Vector2Int> openList = new List<Vector2Int>();
		List<Vector2Int> toReturn = new List<Vector2Int>();
		toReturn.Add(firstFloorTile);
		openList.Add(firstFloorTile);

		while (openList.Count > 0)
		{
			for (int i = 0; i < openList.Count; i++)
			{
				Vector2Int[] neighbors = GetNeighbouringFloors(openList[i]);

				openList.RemoveAt(i);

				for (int j = 0; j < neighbors.Length; j++)
				{
					if (!toReturn.Contains(neighbors[j]))
					{
						openList.Add(neighbors[j]);
						toReturn.Add(neighbors[j]);
					}
				}
			}
		}

		return toReturn.ToArray();
	}

	private Vector2Int[] GetNeighbouringFloors(Vector2Int floor)
	{
		List<Vector2Int> toReturn = new List<Vector2Int>();
		Vector2Int[] offsets = new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };

		for (int i = 0; i < offsets.Length; i++)
		{
			if (grid.ContainsKey(floor + offsets[i]) && grid[floor + offsets[i]] == TileType.Floor)
			{
				toReturn.Add(floor + offsets[i]);
			}
		}

		return toReturn.ToArray();
	}

	private void CreateCorridors()
	{
		for (int i = 0; i < rooms.Count; i++)
		{
			Room currentRoom = rooms[i];
			List<Room> otherRooms = new List<Room>(rooms);
			otherRooms.RemoveAt(i);

			for (int c = 0; c < corridorsPerRoom; c++)
			{
				int otherRoomIndex = Random.Range(0, otherRooms.Count);
				Room otherRoom = otherRooms[otherRoomIndex];
				otherRooms.RemoveAt(otherRoomIndex);

				Vector2Int startTile = currentRoom.ReturnRandomTile();
				Vector2Int targetTile = otherRoom.ReturnRandomTile();

				int directionX = Mathf.Clamp(targetTile.x - startTile.x, -1, 1);
				int directionY = Mathf.Clamp(targetTile.y - startTile.y, -1, 1);

				Vector2Int corridorHead = startTile;

				for (int x = 0; x < Mathf.Abs(startTile.x - targetTile.x); x++)
				{
					corridorHead.x += directionX;
					SetTileType(corridorHead, TileType.Floor);
				}

				for (int y = 0; y < Mathf.Abs(startTile.y - targetTile.y); y++)
				{
					corridorHead.y += directionY;
					SetTileType(corridorHead, TileType.Floor);
				}
			}
		}
	}

	private void SetTileType(Vector2Int tileKey, TileType tileType)
	{
		if (grid.ContainsKey(tileKey))
		{
			grid[tileKey] = tileType;
		}
		else
		{
			grid.Add(tileKey, tileType);
		}
	}

	//Pathfinding: ---------------------------------------------------------------------------------------------------------------

	public List<PathfindingTile> FindPath(Vector2Int startPosition, Vector2Int endPosition)
	{
		List<PathfindingTile> openList = new List<PathfindingTile>();
		List<PathfindingTile> closedList = new List<PathfindingTile>();

		openList.Add(walkableTiles[startPosition]);

		while (openList.Count > 0)
		{
			PathfindingTile current = GetTileWithLowestTotalCost(openList);

			openList.Remove(current);
			closedList.Add(current);

			if (current.placeInDictionary == endPosition)
			{
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

					//neighbor.SetColor(Color.magenta);
				}
			}
		}

		Debug.LogWarning("No path was found");
		return null;
	}

	internal void StartCalculatingPath(Vector2Int placeInDictionary)
	{
		FindPath(startTile, placeInDictionary);
	}

	public Vector2Int GetRandomWalkableTilePosition()
	{
		Vector2Int[] allWalkableTilePositions = walkableTiles.Keys.ToArray();
		Vector2Int toReturn = allWalkableTilePositions[Random.Range(0, allWalkableTilePositions.Length)];
		return toReturn;
	}

	private Dictionary<Vector2Int, PathfindingTile> GetWalkableTiles()
	{
		Dictionary<Vector3Int, PathfindingTile> allTiles = new Dictionary<Vector3Int, PathfindingTile>();
		Dictionary<Vector2Int, PathfindingTile> unorderedWalkableTiles = new Dictionary<Vector2Int, PathfindingTile>();
		PathfindingTile[] allPathfindingTiles = FindObjectsOfType<PathfindingTile>();

		foreach (PathfindingTile tile in allPathfindingTiles)
		{
			Vector3Int key = new Vector3Int(Mathf.RoundToInt(tile.transform.position.x), Mathf.RoundToInt(tile.transform.position.y), tile.markedWalkable ? 1 : 0);
			if (!allTiles.ContainsKey(key))
				allTiles.Add(key, tile);
		}

		foreach (KeyValuePair<Vector3Int, PathfindingTile> tileWithKey in allTiles)
		{
			Vector3Int potentialSiblingTileKey = new Vector3Int(tileWithKey.Key.x, tileWithKey.Key.y, tileWithKey.Key.z == 0 ? 1 : 0);

			if (!allTiles.ContainsKey(potentialSiblingTileKey) && tileWithKey.Key.z == 1)
			{
				unorderedWalkableTiles.Add(new Vector2Int(tileWithKey.Key.x, tileWithKey.Key.y), tileWithKey.Value);
				tileWithKey.Value.SetPlaceInDictionary(new Vector2Int(tileWithKey.Key.x, tileWithKey.Key.y));
			}
		}

		return unorderedWalkableTiles;
	}

	private List<PathfindingTile> RetracePath(PathfindingTile startTile, PathfindingTile endTile)
	{
		List<PathfindingTile> toReturn = new List<PathfindingTile>();
		PathfindingTile current = endTile;
		while (current != startTile)
		{
			toReturn.Add(current);
			//current.SetColor(Color.cyan);
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

	private PathfindingTile[] GetNeighborTiles(Vector2Int currentKey)
	{
		List<PathfindingTile> toReturn = new List<PathfindingTile>();

		if (walkableTiles.ContainsKey(currentKey + new Vector2Int(1, 0)))
			toReturn.Add(walkableTiles[currentKey + new Vector2Int(1, 0)]);

		if (walkableTiles.ContainsKey(currentKey + new Vector2Int(-1, 0)))
			toReturn.Add(walkableTiles[currentKey + new Vector2Int(-1, 0)]);

		if (walkableTiles.ContainsKey(currentKey + new Vector2Int(0, 1)))
			toReturn.Add(walkableTiles[currentKey + new Vector2Int(0, 1)]);

		if (walkableTiles.ContainsKey(currentKey + new Vector2Int(0, -1)))
			toReturn.Add(walkableTiles[currentKey + new Vector2Int(0, -1)]);

		return toReturn.ToArray();
	}

	private float GetDistance(Vector2Int tile1, Vector2Int tile2)
	{
		float distanceX = Mathf.Abs(tile1.x - tile2.x);
		float distanceY = Mathf.Abs(tile1.y - tile2.y);
		return distanceX + distanceY;
	}
}
