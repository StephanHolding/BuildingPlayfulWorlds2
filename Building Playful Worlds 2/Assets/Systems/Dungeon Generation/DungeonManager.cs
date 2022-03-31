using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class DungeonManager : SingletonTemplateMono<DungeonManager>
{
	[System.Serializable]
	public class DungeonSaveFile : SerializableData
	{
		public DungeonSaveFile(string instanceIdentifier) : base(instanceIdentifier)
		{
			this.instanceIdentifier = instanceIdentifier;
		}

		public int test = 5;
		//public Dictionary<Vector2Int, TileType> savedGrid = new Dictionary<Vector2Int, TileType>();
		public List<Vector2Int> savedGridPositions = new List<Vector2Int>();
		public List<TileType> savedTilesTypes = new List<TileType>();
		public List<Room> savedRooms = new List<Room>();
	}

	public enum TileType
	{
		Floor,
		Wall
	}

	[System.Serializable]
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
	public GameObject exitTilePrefab;
	public static bool useSaveFile = false;

	[Header("Player and Enemy")]
	public GameObject playerPrefab;
	public GameObject[] enemyPrefabs;
	public int enemyAmount;

	[Header("Item Spawning")]
	public GameObject[] allSpawnableItems;
	public int itemAmount;

	private Dictionary<Vector2Int, PathfindingTile> walkableTiles = new Dictionary<Vector2Int, PathfindingTile>();
	private Dictionary<Vector2Int, TileType> grid = new Dictionary<Vector2Int, TileType>();
	private List<Room> rooms = new List<Room>();
	private Player player;
	private List<Enemy> enemies = new List<Enemy>();
	private List<InventoryItem> inventoryItemsInScene = new List<InventoryItem>();

	private Vector2Int firstFloorTile;
	internal Vector2Int startTile;
	private const string DUNGEONFILE_IDENTIFIER = "DungeonFile";


	protected override void Awake()
	{
		base.Awake();

		SerializationManager.WantsToSave += SaveDungeon;

		DungeonSaveFile saveFile = useSaveFile ? GetDungeonSaveFile() : null;

		if (saveFile == null)
			SpawnDungeon();
		else
			LoadDungeon(saveFile);


		walkableTiles = GetWalkableTiles();

		if (saveFile == null)
		{
			SpawnPlayer();

			for (int i = 0; i < enemyAmount; i++)
			{
				SpawnEnemy();
			}
		}
		else
		{
			SpawnPlayer();

			for (int i = 0; i < enemyAmount; i++)
			{
				SpawnEnemy();
			}

		}

		for (int i = 0; i < itemAmount; i++)
		{
			SpawnItem();
		}

		SpawnExitTile();
	}

	private void Start()
	{
		TurnManager.instance.Begin();
	}

	private void OnDestroy()
	{
		SerializationManager.WantsToSave -= SaveDungeon;
	}

	//Player and Enemy: --------------------------------------------------------------------------------------------------------

	private void SpawnPlayer()
	{
		GameObject newObject = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
		Player player = newObject.GetComponent<Player>();
		player.Init("Player");
		this.player = player;
	}

	private void SpawnEnemy()
	{
		GameObject newObject = Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Length)], Vector3.zero, Quaternion.identity);
		Enemy enemy = newObject.GetComponent<Enemy>();
		enemy.Init("Enemy " + enemies.Count);
		enemies.Add(enemy);
	}

	public List<PathfindingTile> GetAllTilesInRange(Pawn pawn, int range)
	{
		List<PathfindingTile> tilesInRange = new List<PathfindingTile>();
		Vector2Int[] directions = new Vector2Int[] { new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1) };

		for (int i = 0; i < directions.Length; i++)
		{
			for (int xOffset = 0; xOffset <= range; xOffset++)
			{
				for (int yOffset = 0; yOffset <= range - xOffset; yOffset++)
				{
					Vector2Int key = new Vector2Int(pawn.standingOnTile.x + xOffset * directions[i].x, pawn.standingOnTile.y + yOffset * directions[i].y);

					if (walkableTiles.ContainsKey(key))
					{
						if (FindPath(pawn.standingOnTile, key, out List<PathfindingTile> path))
						{
							if (path.Count <= range)
							{
								tilesInRange.Add(walkableTiles[key]);
							}
						}
					}
				}
			}
		}

		return tilesInRange;
	}

	public List<PathfindingTile> HighlightTilesInRange(Pawn pawn, int range, bool makeClickable = true)
	{
		List<PathfindingTile> tilesInRange = GetAllTilesInRange(pawn, range);
		foreach (PathfindingTile tile in tilesInRange)
		{
			tile.SetColor(Color.green);

			if (makeClickable)
			{
				tile.AddToOnClick(delegate { print("clicked"); });
				tile.AddToOnClick(delegate { pawn.MoveToTile(tile.placeInDictionary); });
			}

		}

		return tilesInRange;
	}

	public void DisableHighlightOnTiles(List<PathfindingTile> tiles)
	{
		foreach (PathfindingTile tile in tiles)
		{
			tile.SetColor(new Color(0, 0, 0, 0));
			tile.RemoveListenersFromOnClick();
		}
	}

	//Item Spawning: --------------------------------------------------------------------------------------------------------------------

	private void SpawnItem()
	{
		GameObject toSpawn = allSpawnableItems[Random.Range(0, allSpawnableItems.Length)];
		Vector2Int randomTile = GetRandomWalkableTilePosition();
		GameObject newObject = Instantiate(toSpawn, new Vector3(randomTile.x, randomTile.y, 0), Quaternion.identity);
		InventoryItem ii = newObject.GetComponent<InventoryItem>();
		inventoryItemsInScene.Add(ii);
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

	private void LoadDungeon(DungeonSaveFile saveFile)
	{
		grid = ConvertSavedListsToDictionary(saveFile.savedGridPositions, saveFile.savedTilesTypes);
		rooms = saveFile.savedRooms;
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

	private void SpawnExitTile()
	{
		Vector2Int randomPos = GetRandomWalkableTilePosition();
		GameObject newObject = Instantiate(exitTilePrefab, new Vector3(randomPos.x, randomPos.y, 0), Quaternion.identity);
		newObject.transform.SetParent(transform, false);
	}

	//Pathfinding: ---------------------------------------------------------------------------------------------------------------

	public bool FindPath(Vector2Int startPosition, Vector2Int endPosition, out List<PathfindingTile> path)
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
				path = RetracePath(walkableTiles[startPosition], current);
				return true;
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
		path = null;
		return false;
	}

	internal void StartCalculatingPath(Vector2Int placeInDictionary)
	{
		FindPath(startTile, placeInDictionary, out List<PathfindingTile> path);
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

	private void SaveDungeon()
	{
		DungeonSaveFile saveFile = new DungeonSaveFile(DUNGEONFILE_IDENTIFIER);
		saveFile.savedRooms = rooms;
		//saveFile.savedGrid = grid;

		foreach (var gridTile in grid)
		{
			saveFile.savedGridPositions.Add(gridTile.Key);
			saveFile.savedTilesTypes.Add(gridTile.Value);
		}

		SerializationManager.AddFileToSaveSlot(saveFile);
	}

	private Dictionary<Vector2Int, TileType> ConvertSavedListsToDictionary(List<Vector2Int> positions, List<TileType> types)
	{
		Dictionary<Vector2Int, TileType> toReturn = new Dictionary<Vector2Int, TileType>();

		for (int i = 0; i < positions.Count; i++)
		{
			toReturn.Add(positions[i], types[i]);
		}

		return toReturn;
	}

	private DungeonSaveFile GetDungeonSaveFile()
	{
		return SerializationManager.GetDataByInstanceIdentifier<DungeonSaveFile>(DUNGEONFILE_IDENTIFIER);
	}
}
