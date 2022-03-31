using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TurnManager : SingletonTemplateMono<TurnManager>
{

	private ITurnReciever[] allTurnRecievers;
	private int currentTurn = -1;
	private bool terminated;

	public void Begin()
	{
		NextTurn();
	}

	public void NextTurn()
	{
		if (terminated) return;

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
		List<ITurnReciever> turnRecievers = FindObjectsOfType<Pawn>().OfType<ITurnReciever>().ToList();

		ITurnReciever at0 = turnRecievers[0];

		for (int i = 0; i < turnRecievers.Count; i++)
		{
			if (turnRecievers[i].GetType() == typeof(Player))
			{
				turnRecievers[0] = turnRecievers[i];
				turnRecievers[i] = at0;
			}
		}

		return turnRecievers.ToArray();
	}

	public void Terminate()
	{
		foreach (var turn in allTurnRecievers)
		{
			turn.Terminate();
		}

		terminated = true;
		InputHandler.instance.StopRecordingInput();
	}

}
