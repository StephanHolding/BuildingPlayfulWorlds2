using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Pawn
{
	public override void Init(string objectName)
	{
		base.Init(objectName);
		lastRecordedPosition = standingOnTile;
	}

	public static Vector2Int lastRecordedPosition { get; private set; }

	public Ability[] abilities;

	private List<PathfindingTile> highlightedTiles = new List<PathfindingTile>();

	protected override void Awake()
	{
		base.Awake();

		for (int i = 0; i < abilities.Length; i++)
		{
			abilities[i].SubscribeToAbility(this);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		for (int i = 0; i < abilities.Length; i++)
		{
			abilities[i].UnsubsribeFromAbility(this);
		}
	}

	public override void EndTurn()
	{
		base.EndTurn();
		TurnManager.instance.NextTurn();
	}

	public override void OnTurnEnded()
	{
		base.OnTurnEnded();
		lastRecordedPosition = standingOnTile;
		DungeonManager.instance.DisableHighlightOnTiles(highlightedTiles);
	}

	public override void OnTurnRecieved()
	{
		base.OnTurnRecieved();
		CameraManager.instance.FollowTarget(gameObject);
		highlightedTiles = DungeonManager.instance.HighlightTilesInRange(this, allowedMovement);
	}

	protected override void EndOfPathReached()
	{
		EndTurn();
	}
}
