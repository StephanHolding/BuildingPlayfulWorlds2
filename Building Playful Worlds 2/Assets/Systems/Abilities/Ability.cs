using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu()]
public class Ability : ScriptableObject
{

	public UnityEvent abilityEffect;

	private List<Pawn> subscribedPawns = new List<Pawn>();

	public void InvokeAbility()
	{
		if (abilityEffect != null)
		{
			abilityEffect.Invoke();
		}
	}

	public void SubscribeToAbility(Pawn pawn)
	{
		if (!subscribedPawns.Contains(pawn))
		{
			subscribedPawns.Add(pawn);
		}
	}

	public void UnsubsribeFromAbility(Pawn pawn)
	{
		if (subscribedPawns.Contains(pawn))
		{
			subscribedPawns.Remove(pawn);
		}
	}
	
	public void ChangePawnMovementRange(int changeBy)
	{
		for (int i = 0; i < subscribedPawns.Count; i++)
		{
			subscribedPawns[i].ChangeAllowedMovement(changeBy);
		}
	}

}
