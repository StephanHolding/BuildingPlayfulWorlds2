using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrollingEnemy : Enemy
{
	//public override void Init(string pawnName)
	//{
	//	base.Init(pawnName);
	//	MoveToRandomTileWithinRange();
	//}

	private void MoveToRandomTileWithinRange()
	{
		Vector2Int moveTo = GetRandomTileInRange();
		MoveToTile(moveTo);
	}

	private Vector2Int GetRandomTileInRange()
	{
		List<PathfindingTile> tilesInRange = DungeonManager.instance.GetAllTilesInRange(this, allowedMovement);
		return tilesInRange[Random.Range(0, tilesInRange.Count)].placeInDictionary;
	}

	protected override void EndOfPathReached()
	{
		EndTurn();
	}

	public override void EndTurn()
	{
		base.EndTurn();
		TurnManager.instance.NextTurn();
	}

	public override void OnTurnRecieved()
	{
		base.OnTurnRecieved();
		CameraManager.instance.FollowTarget(gameObject);
		MoveToRandomTileWithinRange();
	}
}
