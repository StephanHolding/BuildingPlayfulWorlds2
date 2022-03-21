using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : SingletonTemplateMono<CameraManager>
{

	public float cameraSpeed;

	private GameObject targetToFollow;
	private Camera cam;

	private void Start()
	{
		cam = Camera.main;
	}

	private void Update()
	{
		if (cam == null) return;
		if (targetToFollow == null) return;

		Follow();
	}

	public void FollowTarget(GameObject target)
	{
		targetToFollow = target;
	}

	private void Follow()
	{
		Vector3 moveTowardsPosition = Vector3.MoveTowards(cam.transform.position, targetToFollow.transform.position, cameraSpeed * Time.deltaTime);
		cam.transform.position = new Vector3(moveTowardsPosition.x, moveTowardsPosition.y, cam.transform.position.z);
	}

}
