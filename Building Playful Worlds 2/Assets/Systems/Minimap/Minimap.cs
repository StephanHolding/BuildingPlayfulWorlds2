using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{

	public Vector2 minMaxZoom;
	public int zoomStep;

	private float currentZoom;
	private Camera cam;

	private void Awake()
	{
		cam = GetComponent<Camera>();
		currentZoom = cam.orthographicSize;
	}

	private void Update()
	{
		float mouseScroll = Input.mouseScrollDelta.y;

	

		if (mouseScroll != 0)
		{
			if (mouseScroll > 0)
			{
				currentZoom = Mathf.Clamp(currentZoom - zoomStep, minMaxZoom.x, minMaxZoom.y);
			}
			else if (mouseScroll < 0)
			{
				currentZoom = Mathf.Clamp(currentZoom + zoomStep, minMaxZoom.x, minMaxZoom.y);
			}

			print(currentZoom);
			cam.orthographicSize = currentZoom;
		}
	}
}
