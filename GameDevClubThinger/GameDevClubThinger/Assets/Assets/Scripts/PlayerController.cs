using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class PlayerController : MonoBehaviour {
	
	public float turningSpeed = 1.0f;
	public float movementSpeed = 0.5f;
	private float facingAngle = 0.0f;
	
	private Transform selfTransform;
	private Transform cameraTransform;

	void Start () {
		selfTransform = this.GetComponent<Transform> ();
		GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
		if (camera == null) {
			Debug.Log("Camera not found!");
		} else {
			cameraTransform = camera.GetComponent<Transform>();
			UpdateCameraTransform();
		}
		// Cursor.visible = false;
	}
	
	// Called whenever the player's position has moved, or whenever the player's field of view has changed (i.e. turning with the mouse).
	void UpdateCameraTransform () {
		
		float posX = 10 * (float) Math.Sin(facingAngle / 180 * Math.PI);
		float posY = 10;
		float posZ = 10 * (float) Math.Cos(facingAngle / 180 * Math.PI);
		
		// The Y rotation is the only value changed by the player controller; leave other values unchanged.
		float rotX = cameraTransform.rotation.x;
		float rotZ = cameraTransform.rotation.z;
		float rotW = cameraTransform.rotation.w;
		
		cameraTransform.position = new Vector3(posX, posY, posZ);
		cameraTransform.rotation = new Quaternion(rotX, facingAngle, rotZ, rotW);
	}
	
	// Update is called once per frame
	void Update () {
		
		float inputForward = Input.GetAxis("Vertical");
		float inputStrafe = Input.GetAxis("Horizontal");
		float inputRotate = Input.GetAxis("Mouse X");
		
		if (inputRotate != 0) {
			facingAngle += turningSpeed * inputRotate;
			UpdateCameraTransform();
		}
		
		if (inputForward != 0 || inputStrafe != 0) {
			
			float inputMagnitude = (float) Math.Sqrt(inputForward * inputForward + inputStrafe * inputStrafe);
			float inputAngle = (float) Math.Sinh(inputStrafe / inputMagnitude);
			
			float movementAngle = facingAngle + inputAngle;
			
			float dX = movementSpeed * inputMagnitude * (float) Math.Sin(movementAngle / 180 * Math.PI);
			float dY = movementSpeed * inputMagnitude * (float) Math.Cos(movementAngle / 180 * Math.PI);
			
			// Todo: Change this into a physics engine force instead, to work with collision.
			float posX = selfTransform.position.x + dX;
			float posY = selfTransform.position.y;
			float posZ = selfTransform.position.z + dY;
			selfTransform.position = new Vector3(posX, posY, posZ);
		}
	}
}
