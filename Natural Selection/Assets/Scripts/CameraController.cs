using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

	public float CameraMoveSpeed = 120.0f;
	public GameObject CameraFollowObj;
	public float clampAngle = 80.0f;
	public float inputSensitivity = 150.0f;
	public float mouseX;
	public float mouseY;
	public Vector3 offset;

	private float Yaw = 0.0f;
	private float Pitch = 0.0f;




	void Start()
	{
		Vector3 rot = transform.localRotation.eulerAngles;
		Yaw = rot.y;
		Pitch = rot.x;
	}

	void Update()
	{
		mouseX = Input.GetAxis("Mouse X");
		mouseY = Input.GetAxis("Mouse Y");

		Yaw += mouseX * inputSensitivity * Time.fixedDeltaTime;
		Pitch -= mouseY * inputSensitivity * Time.fixedDeltaTime;

		Pitch = Mathf.Clamp(Pitch, -clampAngle, clampAngle);

		Quaternion localRotation = Quaternion.Euler(Pitch, Yaw, 0.0f);
		transform.rotation = localRotation;

		
	}

	void LateUpdate()
	{
		CameraUpdater();
	}

	void CameraUpdater()
	{
		Transform target = CameraFollowObj.transform;

		

		float speed = CameraMoveSpeed * Time.deltaTime;
		transform.position = Vector3.MoveTowards(transform.position, target.position, speed);
	}
}
