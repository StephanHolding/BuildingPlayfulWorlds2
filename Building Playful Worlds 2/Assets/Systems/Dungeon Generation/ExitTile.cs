using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitTile : MonoBehaviour, IPlayerTouchable
{
	public void OnTouchedByPlayer(Player player)
	{
		UIManager.instance.ShowWindow("Game Win");
		TurnManager.instance.Terminate();
	}
}
