using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Pawn, ITurnReciever
{
	public override void Init(string objectName)
	{
		base.Init(objectName);
		lastRecordedPosition = standingOnTile;
	}

	public static Vector2Int lastRecordedPosition { get; private set; }

	private List<PathfindingTile> highlightedTiles = new List<PathfindingTile>();

	public void EndTurn()
	{
		TurnManager.instance.NextTurn();
	}

	public void OnTurnEnded()
	{
		lastRecordedPosition = standingOnTile;
		DungeonManager.instance.DisableHighlightOnTiles(highlightedTiles);
	}

	public void OnTurnRecieved()
	{
		CameraManager.instance.FollowTarget(gameObject);
		highlightedTiles = DungeonManager.instance.HighlightTilesInRange(this, allowedMovement);
	}

	protected override void EndOfPathReached()
	{
		EndTurn();
	}
}
