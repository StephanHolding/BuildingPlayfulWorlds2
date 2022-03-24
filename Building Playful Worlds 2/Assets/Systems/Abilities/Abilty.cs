using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu()]
public class Abilty : ScriptableObject
{

	public UnityEvent abilityEffect;

	private GameObject calledOn;

	public void InvokeAbility(GameObject callOn)
	{
		calledOn = callOn;
		if (abilityEffect != null)
		{
			abilityEffect.Invoke();
		}
		calledOn = null;
	}
	
	public void ChangePawnMovementRange(int changeBy)
	{
		Pawn pawn = calledOn.GetComponent<Pawn>();
		pawn.ChangeAllowedMovement(changeBy);
	}

}
