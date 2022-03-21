using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargingEnemy : Enemy, ITurnReciever
{

	private void MoveTowardsPlayer()
	{
		MoveToTile(Player.lastRecordedPosition);
	}

	protected override void EndOfPathReached()
	{
		EndTurn();
	}

	public void EndTurn()
	{
		TurnManager.instance.NextTurn();
	}

	public void OnTurnEnded()
	{
		//throw new System.NotImplementedException();
	}

	public void OnTurnRecieved()
	{
		CameraManager.instance.FollowTarget(gameObject);
		MoveTowardsPlayer();
	}
}
