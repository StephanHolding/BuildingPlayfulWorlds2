using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Pawn, IEnemyTouchable
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

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!onTurn) return;

		IPlayerTouchable playerTouchable = collision.gameObject.GetComponent<IPlayerTouchable>();

		if (playerTouchable != null)
		{
			playerTouchable.OnTouchedByPlayer(this);
		}
	}


	public void OnTouchedByEnemy(Enemy enemy)
	{
		UIManager.instance.ShowWindow("Game Over");
		TurnManager.instance.Terminate();
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

	public override void ChangeAllowedMovement(int changeBy)
	{
		base.ChangeAllowedMovement(changeBy);

		if (onTurn)
		{
			highlightedTiles = DungeonManager.instance.HighlightTilesInRange(this, allowedMovement);
		}
	}

	protected override void EndOfPathReached()
	{
		EndTurn();
	}
}
