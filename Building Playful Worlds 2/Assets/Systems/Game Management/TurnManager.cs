using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TurnManager : SingletonTemplateMono<TurnManager>
{

	private ITurnReciever[] allTurnRecievers;
	private int currentTurn = -1;

	public void Begin()
	{
		NextTurn();
	}

	public void NextTurn()
	{
		if (currentTurn != -1)
			allTurnRecievers[currentTurn].OnTurnEnded();

		allTurnRecievers = GetAllTurnRecieversFromScene();

		if (currentTurn + 1 < allTurnRecievers.Length)
		{
			currentTurn++;
		}
		else
		{
			currentTurn = 0;
		}

		allTurnRecievers[currentTurn].OnTurnRecieved();
	}

	private ITurnReciever[] GetAllTurnRecieversFromScene()
	{
		return FindObjectsOfType<Pawn>().OfType<ITurnReciever>().ToArray();
	}

}
