using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargingEnemy : Enemy
{

	private void MoveTowardsPlayer()
	{
		MoveToTile(Player.lastRecordedPosition);
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
		MoveTowardsPlayer();
	}
}
