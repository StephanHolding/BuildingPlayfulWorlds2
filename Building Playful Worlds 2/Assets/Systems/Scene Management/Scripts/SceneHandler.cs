using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : SingletonTemplateMono<SceneHandler>
{
	public bool useScreenfade;

	private bool isFading;
	private string sceneToLoad;

	protected override void Awake()
	{
		base.Awake();

		if (useScreenfade)
		{
			ScreenFade.OnFadeDone += FadeIsDone;
			SceneManager.sceneLoaded += OnSceneLoaded;
			StartCoroutine(WaitForMainCamera());
		}
	}

	private void OnDestroy()
	{
		if (useScreenfade)
		{
			ScreenFade.OnFadeDone -= FadeIsDone;
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}
	}

	public void LoadScene(string sceneName)
	{
		sceneToLoad = sceneName;

		if (useScreenfade)
		{
			ScreenFade.FadeOut();
			isFading = true;
			StartCoroutine(WaitForFade());
		}
		else
		{
			SceneLoading();
		}
	}

	public void LoadScene(int sceneIndex)
	{
		LoadScene(GetSceneIndexByBuildName(sceneIndex));
	}

	[ContextMenu("Reload Current Scene")]
	public void ReloadCurrentScene()
	{
		LoadScene(SceneManager.GetActiveScene().name);
	}

	[ContextMenu("Load Next Scene")]
	public void LoadNextScene()
	{
		int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
		LoadScene(nextSceneIndex);
	}

	[ContextMenu("Load First Scene")]
	public void LoadFirstScene()
	{
		LoadScene(0);
	}

	[ContextMenu("Load Last Scene")]
	public void LoadLastScene()
	{
		LoadScene(SceneManager.sceneCountInBuildSettings - 1);
	}

	public void QuitGame()
	{
		Application.Quit();
	}

	public bool NextSceneIsPresent()
	{
		int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
		return nextSceneIndex < SceneManager.sceneCountInBuildSettings;
	}

	private void FadeIsDone()
	{
		isFading = false;
	}

	private void OnSceneLoaded(Scene loadedScene, LoadSceneMode mode)
	{
		StartCoroutine(WaitForMainCamera());
	}

	private string GetSceneIndexByBuildName(int sceneIndex)
	{
		string scenePath = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
		string toReturn = System.IO.Path.GetFileNameWithoutExtension(scenePath);
		return toReturn;
	}

	private IEnumerator WaitForFade()
	{
		yield return new WaitUntil(() => isFading == false);
		yield return new WaitForSeconds(0.5f);
		SceneLoading();
	}

	private IEnumerator WaitForMainCamera()
	{
		yield return new WaitUntil(() => Camera.main != null);
		ScreenFade.FadeIn();
	}

	private void SceneLoading()
	{
		SceneManager.LoadScene(sceneToLoad);
	}
}
