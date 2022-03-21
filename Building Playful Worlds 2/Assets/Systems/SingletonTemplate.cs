using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if PHOTON_UNITY_NETWORKING
using Photon.Pun;
#endif

[DefaultExecutionOrder(-5)]
public class SingletonTemplateMono<T> : MonoBehaviour where T : Component
{

	public static T instance;

	public bool dontDestroyOnLoad;

	protected virtual void Awake()
	{
		if (instance == null)
		{
			instance = this as T;

			if (dontDestroyOnLoad)
				DontDestroyOnLoad(gameObject);
		}
		else
			Destroy(gameObject);
	}

}

#if PHOTON_UNITY_NETWORKING
[DefaultExecutionOrder(-5)]
public abstract class SingletonTemplateMonoPunCallbacks<T> : MonoBehaviourPunCallbacks where T : Component
{
	public static T instance;

	public bool dontDestroyOnLoad;

	protected virtual void Awake()
	{
		if (instance == null)
		{
			instance = this as T;

			if (dontDestroyOnLoad)
				DontDestroyOnLoad(gameObject);
		}
		else
			Destroy(gameObject);
	}
}
#endif