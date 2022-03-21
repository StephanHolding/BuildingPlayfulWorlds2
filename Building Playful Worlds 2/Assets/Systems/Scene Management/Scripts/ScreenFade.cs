using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFade : MonoBehaviour
{

	internal delegate void FadeEvent();
	internal static event FadeEvent OnFadeDone;

	//private Material sprite;
	private SpriteRenderer sprite;
	private Image image;
	private Color currentColor;
	private static ScreenFade instance;

	private void Awake()
	{
		sprite = GetComponent<SpriteRenderer>();
		image = GetComponentInChildren<Image>();
		currentColor = sprite.color;
	}

	private void Start()
	{
		if (instance == null)
			instance = this;

		if (SceneHandler.instance == null)
		{
			FadeIn();
		}
	}

	/// <summary>
	/// Fade the screen in over time.
	/// </summary>
	/// <param name="fadeTime">Time in seconds it will take the color to reach 0% opacity.</param>
	public static void FadeIn(float fadeTime = 2)
	{
		ScreenFade sf = GetScreenfadeInstance();
		FadeIn(sf.currentColor, fadeTime);
	}

	/// <summary>
	///  Fade the screen in over time.
	/// </summary>
	/// <param name="fadeColor">The color that will apear on the screen.</param>
	/// <param name="fadeTime">Time in seconds it will take the color to reach 0% opacity.</param>
	public static void FadeIn(Color fadeColor, float fadeTime = 2)
	{
		ScreenFade sf = GetScreenfadeInstance();
		sf.StopAllCoroutines();

		sf.StartCoroutine(sf.FadeInCoroutine(fadeTime, fadeColor));
	}

	/// <summary>
	///  Fade the screen in over time.
	/// </summary>
	/// <param name="fadeColor">The color that will apear on the screen.</param>
	/// <param name="fadeTime">Time in seconds it will take the color to reach 0% opacity.</param>
	/// <param name="alpha">Alpha to go to.</param>
	public static void FadeIn(Color fadeColor, float fadeTime = 2, float alpha = 0)
	{
		ScreenFade sf = GetScreenfadeInstance();
		sf.StopAllCoroutines();

		sf.StartCoroutine(sf.FadeInCoroutine(fadeTime, fadeColor, alpha));
	}

	/// <summary>
	/// Fade the screen out over time.
	/// </summary>
	/// <param name="fadeTime">Time in seconds it will take the color to reach 100% opacity.</param>
	public static void FadeOut(float fadeTime = 2)
	{
		ScreenFade sf = GetScreenfadeInstance();
		FadeOut(sf.currentColor, fadeTime);
	}

	/// <summary>
	/// Fade the screen out over time.
	/// </summary>
	/// <param name="fadeColor">Color that will appear on screen.</param>
	/// <param name="fadeTime">Time in seconds it will take the color to reach 100% opacity.</param>
	public static void FadeOut(Color fadeColor, float fadeTime = 2)
	{
		ScreenFade sf = GetScreenfadeInstance();
		sf.StopAllCoroutines();
		sf.StartCoroutine(sf.FadeOutCoroutine(fadeTime, fadeColor));
	}
	/// <summary>
	/// Fade the screen out over time.
	/// </summary>
	/// <param name="fadeColor">Color that will appear on screen.</param>
	/// <param name="fadeTime">Time in seconds it will take the color to reach 100% opacity.</param>
	/// <param name="alpha">Alpha to go to.</param>
	public static void FadeOut(Color fadeColor, float fadeTime = 2, float alpha = 1)
	{
		ScreenFade sf = GetScreenfadeInstance();
		sf.StopAllCoroutines();
		sf.StartCoroutine(sf.FadeOutCoroutine(fadeTime, fadeColor, alpha));
	}

	/// <summary>
	/// Set the opacity of the screen fade directly and instantly.
	/// </summary>
	/// <param name="fadeAmount">New opacity of the screenfade. Clamped between 0 and 1.</param>
	/// <param name="fadeColor">The color of the screen.</param>
	public static void SetFadeDirectly(float fadeAmount, Color fadeColor)
	{
		ScreenFade sf = GetScreenfadeInstance();
		fadeAmount = Mathf.Clamp(fadeAmount, 0, 1);
		sf.sprite.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, fadeAmount);
		sf.image.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, fadeAmount);
	}

	private IEnumerator FadeInCoroutine(float fadeTime, Color fadeColor, float alpha = 0)
	{
		currentColor = fadeColor;
		while (sprite.color.a > alpha)
		{
			sprite.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, sprite.color.a - Time.deltaTime / fadeTime);
			image.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, image.color.a - Time.deltaTime / fadeTime);
			yield return new WaitForEndOfFrame();
		}

		sprite.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
		image.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);

		OnFadeDone?.Invoke();
	}

	private IEnumerator FadeOutCoroutine(float fadeTime, Color fadeColor, float alpha = 1)
	{
		currentColor = fadeColor;
		while (sprite.color.a < alpha)
		{
			sprite.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, sprite.color.a + Time.deltaTime / fadeTime);
			image.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, image.color.a + Time.deltaTime / fadeTime);
			yield return new WaitForEndOfFrame();
		}

		sprite.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
		image.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);

		OnFadeDone?.Invoke();
	}

	private static ScreenFade GetScreenfadeInstance()
	{
		if (instance == null)
			instance = CreateScreenfadeInstance(Camera.main);

		return instance;
	}

	private static ScreenFade CreateScreenfadeInstance(Camera cam)
	{
		if (!Application.isPlaying) return null;

		GameObject screenfadePrefab = (GameObject)Resources.Load("Screenfade");
		GameObject newObject = Instantiate(screenfadePrefab, cam.transform.position, Quaternion.identity);
		newObject.transform.SetParent(cam.transform);
		newObject.transform.localPosition = new Vector3(0, 0, cam.nearClipPlane + 0.05f);
		newObject.transform.localRotation = new Quaternion();

		return newObject.GetComponent<ScreenFade>();
	}
}
