
#define DEBUG_CONSTANTS
#define DEBUG_MOVEMENT

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class PlayerController : MonoBehaviour {
	
	// In degrees/units per second:
	public float turningSpeed = 360f;
	public float movementSpeed = 15f;
	
	// timeToApex: Time a maximum-height, maximum-speed jump takes to reach its apex
	public float minJumpHeight = 3f;
	public float maxJumpHeight = 11f;
	public float timeToApex = 1f;
	
	// airControl: Factor on how easily player can change their velocity while in the air
	// glideVelocity: Velocity minimum when gliding
	public float airControl = 1.5f;
	public float glideVelocity = -1f;
	
	// Gravity used whenever player is falling (units per second per second)
	public float gravityOnFalling = -10f;
	
	// cameraDistance: Distance from player to camera, irrespective of camera angle
	// min/maxCameraAngle: In degrees; negative is looking up above player, positive is looking down at player
	// lookUpBuffer: How many degrees upwards a player needs to try to look before the camera will actually look up
	public float cameraDistance = 10f;
	public float minCameraAngle = -30f;
	public float maxCameraAngle = 89.9f;
	public float lookUpBuffer = 20f;
	
	// Calculated based on min/max jump height and time to apex
	private float jumpVelocity;
	private float gravityOnJumpHeld;
	private float gravityOnJumpRelease;
	
	// In degrees:
	private float yaw = 0f;
	private float pitch = 45f;
	
	private float inputVertical = 0f;
	private float inputHorizontal = 0f;
	private float inputMouseX = 0f;
	private float inputMouseY = 0f;
	
	private bool inputJumpPress = false;
	private bool inputJumpRelease = false;
	
	private bool isJumpHeld = false;
	private bool isGliding = false;
	
	private bool doCameraUpdate = false;
	
	private Vector3 velocity;
	
	private Transform transform;
	private CharacterController controller;
	private Transform cameraTransform;
	
	void Start() {
		
		LoadValues();
		Cursor.lockState = CursorLockMode.Locked;
	}
	
	void LoadValues() {
		
		transform = this.GetComponent<Transform>();
		controller = this.GetComponent<CharacterController>();
		
		GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
		if (camera == null) {
			Debug.Log("Camera not found!");
		} else {
			cameraTransform = camera.GetComponent<Transform>();
			UpdateCamera();
		}
		
		jumpVelocity = 2 * maxJumpHeight / timeToApex;
		gravityOnJumpHeld = -jumpVelocity / timeToApex;
		gravityOnJumpRelease = jumpVelocity / minJumpHeight / 2 - jumpVelocity * jumpVelocity / minJumpHeight;
		
		#if DEBUG_CONSTANTS
		Debug.Log("Jump velocity: " + jumpVelocity);
		Debug.Log("Jump held gravity: " + gravityOnJumpHeld);
		Debug.Log("Jump released gravity: " + gravityOnJumpRelease);
		#endif
	}
	
	bool GetJumpPress() {
		
		bool isJumpPressed = inputJumpPress;
		inputJumpPress = false;
		return isJumpPressed;
	}
	
	bool GetJumpRelease() {
		
		bool isJumpReleased = inputJumpRelease;
		inputJumpRelease = false;
		return isJumpReleased;
	}
	
	void UpdateCamera() {
		
		// Compute the location of the camera based on which direction it should be looking
		float pseudoPitch = pitch;
		if (pitch < 0) {
			pseudoPitch = Math.Min(0, pitch + lookUpBuffer);
		}
		
		double cameraPlanarDistance = cameraDistance * Math.Cos(pseudoPitch / 180 * Math.PI);
		double cameraVerticalDistance = cameraDistance * Math.Sin(Math.Max(0, pitch) / 180 * Math.PI);
		
		float posX = (float) (transform.position.x + cameraPlanarDistance * Math.Sin(yaw / 180 * Math.PI));
		float posY = (float) (transform.position.y + cameraVerticalDistance);
		float posZ = (float) (transform.position.z + cameraPlanarDistance * Math.Cos(yaw / 180 * Math.PI));
		
		cameraTransform.position = new Vector3(posX, posY, posZ);
		
		// Change the camera's rotation to be aimed precisely at the player
		cameraTransform.LookAt(transform);
		
		if (pitch < 0) {
			cameraTransform.Rotate(pseudoPitch, 0, 0);
		}
	}
	
	void UpdateInput() {
		
		inputVertical = Input.GetAxis("Vertical");
		inputHorizontal = Input.GetAxis("Horizontal");
		inputMouseX = Input.GetAxis("Mouse X");
		inputMouseY = Input.GetAxis("Mouse Y");
		
		inputJumpPress = inputJumpPress || Input.GetKeyDown(KeyCode.Space);
		inputJumpRelease = inputJumpRelease || Input.GetKeyUp(KeyCode.Space);
		
		if (!Input.GetKey(KeyCode.Space) && isJumpHeld) {
			isJumpHeld = false;
			
			#if DEBUG_MOVEMENT
				Debug.Log("Jump released.");
			#endif
		}
	}
	
	void FixedAcceleration(Vector3 force) {
		
		velocity += force * Time.fixedDeltaTime;
	}
	
	void FixedPlayerControl() {
		
		float dX = 0;
		float dZ = 0;
		if (inputVertical != 0 || inputHorizontal != 0) {
			
			// Get the direction of the input; forward has an angle of 0, right strafing has an angle of pi / 2
			// Backward should have an angle of 1 pi, but the range of inverse sin is from -pi / 2 to pi / 2; forward and backward appear identical to asin
			double inputMagnitude = Math.Sqrt(inputVertical * inputVertical + inputHorizontal * inputHorizontal);
			double inputAngle = Math.Asin(inputHorizontal / inputMagnitude);
			
			// Handle the limited range of inverse sin for backwards movement
			if (inputVertical < 0) {
				inputAngle = Math.PI - inputAngle;
			}
			
			// The direction the player will move depends on both the direction the player's facing and the direction of the input
			double movementAngle = yaw / 180 * Math.PI + inputAngle;
			
			// This line is trigonometric magic.
			// Dividing inputMagnitude by this number prevents diagonal movement from being ~1.414 times faster than strictly orthogonal movement.
			inputMagnitude /= Math.Sqrt(Math.Pow(Math.Tan(Math.Abs(inputAngle - Math.Round(inputAngle / Math.PI * 2) * Math.PI / 2)), 2) + 1);
			
			dX = (float) (-movementSpeed * inputMagnitude * Math.Sin(movementAngle));
			dZ = (float) (-movementSpeed * inputMagnitude * Math.Cos(movementAngle));
		}
		
		if (controller.isGrounded) {
			velocity = new Vector3(dX, velocity.y, dZ);
			
		} else {
			dX = (dX - velocity.x) * airControl;
			dZ = (dZ - velocity.z) * airControl;
			
			FixedAcceleration(new Vector3(dX, 0, dZ));
		}
		
		bool jumpPress = GetJumpPress();
		bool jumpRelease = GetJumpRelease();
		
		// Gliding:
		if (jumpPress && !isJumpHeld && !controller.isGrounded) {
			isGliding = !isGliding;
			
			#if DEBUG_MOVEMENT
			if (isGliding) {
				Debug.Log("Started gliding.");
			} else {
				Debug.Log("Stopped gliding.");
			}
			#endif
		}
		
		// Jumping:
		if (jumpPress) {
			if (controller.isGrounded) {
				isJumpHeld = true;
				velocity = new Vector3(velocity.x, jumpVelocity, velocity.z);
				
				#if DEBUG_MOVEMENT
				Debug.Log("Jumped.");
				#endif
			}
		}
	}
	
	void PlayerControl() {
		
		if (inputMouseX != 0) {
			
			yaw += turningSpeed * inputMouseX * Time.deltaTime;
			transform.Rotate(0, turningSpeed * inputMouseX * Time.deltaTime, 0);
			doCameraUpdate = true;
		}
		
		if (inputMouseY != 0) {
			
			pitch -= turningSpeed * inputMouseY * Time.deltaTime;
			if (pitch < minCameraAngle - lookUpBuffer) {
				pitch = minCameraAngle - lookUpBuffer;
			} else if (pitch > maxCameraAngle) {
				pitch = maxCameraAngle;
			}
			doCameraUpdate = true;
			
		} else if (pitch < 0 && pitch > -lookUpBuffer) {
			pitch = 0;
		}
	}
	
	void Gravity() {
		
		float gravity = 0f;
		bool glide = false;
		if (velocity.y > 0) {
			if (isJumpHeld) {
				gravity = gravityOnJumpHeld;
			} else {
				gravity = gravityOnJumpRelease;
			}
		} else {
			gravity = gravityOnFalling;
			glide = isGliding;
		}
		
		FixedAcceleration(transform.up * gravity);
		
		if (glide && velocity.y < glideVelocity) {
			velocity = new Vector3(velocity.x, glideVelocity, velocity.z);
		}
		
		if (controller.isGrounded) {
			isGliding = false;
		}
	}
	
	void FixedUpdate() {
		
		velocity = controller.velocity;
		
		FixedPlayerControl();
		Gravity();
		
		controller.Move(velocity * Time.fixedDeltaTime);
		doCameraUpdate = true;
	}
	
	void Update() {
		
		UpdateInput();
		PlayerControl();
		
		if (doCameraUpdate) {
			UpdateCamera();
		}
	}
}
