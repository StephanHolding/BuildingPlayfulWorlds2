using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityReciever : MonoBehaviour
{

	public enum Ability
	{

	}

	public delegate void AbilityRecieverDelegate();
	public static event AbilityRecieverDelegate OnAbilityRecieved;

	 

}
