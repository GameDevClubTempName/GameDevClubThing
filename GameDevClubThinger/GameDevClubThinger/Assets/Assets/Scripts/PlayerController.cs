using System.Collections;
using System.Collections.Generic;
using System; // I had to do this to get Math to work, I don't think this is the best way?
using UnityEngine;

public class PlayerController : MonoBehaviour {
	
	public float turningSpeed = 2.5f;
	public float movementSpeed = 0.5f;
	
	// Note: facingAngle is in degrees. There's no good reason for this except for the fact that it didn't want to work in radians for whatever reason.
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
		
		// Compute the location of the camera based on which direction it should be looking
		float posX = selfTransform.position.x + 10 * (float) Math.Sin(facingAngle / 180 * Math.PI);
		float posY = selfTransform.position.y + 10;
		float posZ = selfTransform.position.z + 10 * (float) Math.Cos(facingAngle / 180 * Math.PI);
		
		cameraTransform.position = new Vector3(posX, posY, posZ);
		
		// Change the camera's rotation to be aimed precisely at the player
		cameraTransform.LookAt(selfTransform);
	}
	
	// Update is called once per frame
	void Update () {
		
		float inputForward = Input.GetAxis("Vertical");
		float inputStrafe = Input.GetAxis("Horizontal");
		float inputRotate = Input.GetAxis("Mouse X");
		
		bool updateNeeded = false;
		
		if (inputRotate != 0) {
			facingAngle += turningSpeed * inputRotate;
			updateNeeded = true;
		}
		
		if (inputForward != 0 || inputStrafe != 0) {
			
			// Get the direction of the input; forward has an angle of 0, right strafing has an angle of pi / 2
			// Backward should have an angle of 1 pi, but the range of inverse sin is from -pi / 2 to pi / 2; forward and backward appear identical to sinh
			float inputMagnitude = (float) Math.Sqrt(inputForward * inputForward + inputStrafe * inputStrafe);
			float inputAngle = (float) Math.Sinh(inputStrafe / inputMagnitude);
			
			// Handle the limited range of inverse sin for backwards movement
			if (inputForward < 0) {
				inputAngle = (float) (Math.PI - inputAngle);
			}
			
			// The direction the player will move depends on both the direction the player's facing and the direction of the input
			float movementAngle = (float) (facingAngle / 180 * Math.PI + inputAngle);
			
			// These are negative because that's what made it work ¯\_(ツ)_/¯
			float dX = -movementSpeed * inputMagnitude * (float) Math.Sin(movementAngle);
			float dY = -movementSpeed * inputMagnitude * (float) Math.Cos(movementAngle);
			
			// Todo: Change this into a physics engine force instead, to work with collision.
			float posX = selfTransform.position.x + dX;
			float posY = selfTransform.position.y;
			float posZ = selfTransform.position.z + dY;
			selfTransform.position = new Vector3(posX, posY, posZ);
			
			updateNeeded = true;
		}
		
		if (updateNeeded) {
			UpdateCameraTransform();
		}
	}
}
