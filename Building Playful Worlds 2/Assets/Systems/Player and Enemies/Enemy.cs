using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Pawn, IPlayerTouchable
{

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!onTurn) return;

		IEnemyTouchable enemyTouchable = collision.gameObject.GetComponent<IEnemyTouchable>();

		if (enemyTouchable != null)
		{
			enemyTouchable.OnTouchedByEnemy(this);
		}
	}

	public void OnTouchedByPlayer(Player player)
	{
		Destroy(gameObject);
	}
}
