using System.Collections;
using System.Collections.Generic;
using System; // I had to do this to get Math to work, I don't think this is the best way?
using UnityEngine;

public class PlayerController : MonoBehaviour {
	
	public float turningSpeed;
	public float movementSpeed;
	public float jumpStrength;
	
	// Note: facingAngle is in degrees. There's no good reason for this except for the fact that it didn't want to work in radians for whatever reason.
	private float facingAngle = 0.0f;
	
	private float posXLastRenderTick = 0.0f;
	private float posYLastRenderTick = 0.0f;
	private float posZLastRenderTick = 0.0f;
	
	private float posYLastPhysicsTick = 0.0f;
	private bool canJump = true;
	
	private Transform selfTransform;
	private Rigidbody selfRigidbody;
	
	private Transform cameraTransform;

	void Start () {
		selfTransform = this.GetComponent<Transform> ();
		selfRigidbody = this.GetComponent<Rigidbody> ();
		
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
	
	// FixedUpdate is called once per frame of physics
	void FixedUpdate () {
		if (posYLastPhysicsTick == selfTransform.position.y) {
			canJump = true;
		} else {
			posYLastPhysicsTick = selfTransform.position.y;
		}
	}
	
	// Update is called once per frame of rendering
	void Update () {
		
		float inputForward = Input.GetAxis("Vertical");
		float inputStrafe = Input.GetAxis("Horizontal");
		float inputRotate = Input.GetAxis("Mouse X");
		float inputJump = Input.GetAxis("Jump");
		
		bool updateNeeded = false;
		
		if (posXLastRenderTick != selfTransform.position.x || posYLastRenderTick != selfTransform.position.y || posZLastRenderTick != selfTransform.position.z) {
			posXLastRenderTick = selfTransform.position.x;
			posYLastRenderTick = selfTransform.position.y;
			posZLastRenderTick = selfTransform.position.z;
			updateNeeded = true;
		}
		
		if (inputRotate != 0) {
			facingAngle += turningSpeed * inputRotate;
			selfTransform.Rotate(0, turningSpeed * inputRotate, 0);
			updateNeeded = true;
		}
		
		if (inputJump != 0 && canJump) {
			canJump = false;
			selfRigidbody.AddForce(transform.up * jumpStrength, ForceMode.Impulse);
		}
		
		if (inputForward != 0 || inputStrafe != 0) {
			
			// Get the direction of the input; forward has an angle of 0, right strafing has an angle of pi / 2
			// Backward should have an angle of 1 pi, but the range of inverse sin is from -pi / 2 to pi / 2; forward and backward appear identical to asin
			float inputMagnitude = (float) Math.Sqrt(inputForward * inputForward + inputStrafe * inputStrafe);
			float inputAngle = (float) Math.Asin(inputStrafe / inputMagnitude);
			
			// Handle the limited range of inverse sin for backwards movement
			if (inputForward < 0) {
				inputAngle = (float) (Math.PI - inputAngle);
			}
			
			// The direction the player will move depends on both the direction the player's facing and the direction of the input
			float movementAngle = (float) (facingAngle / 180 * Math.PI + inputAngle);
			
			// These are negative because that's what made it work ¯\_(ツ)_/¯
			float dX = -movementSpeed * inputMagnitude * (float) Math.Sin(movementAngle);
			float dZ = -movementSpeed * inputMagnitude * (float) Math.Cos(movementAngle);
			
			// Todo: Change this into a physics engine force instead, to work with collision.
			
			// selfRigidbody.AddForce(transform.right * dX * 100, ForceMode.Impulse);
			// selfRigidbody.AddForce(transform.forward * dZ * 100, ForceMode.Impulse);
			
			float posX = selfTransform.position.x + dX;
			float posY = selfTransform.position.y;
			float posZ = selfTransform.position.z + dZ;
			selfTransform.position = new Vector3(posX, posY, posZ);
		}
		
		if (updateNeeded) {
			UpdateCameraTransform();
		}
	}
}
