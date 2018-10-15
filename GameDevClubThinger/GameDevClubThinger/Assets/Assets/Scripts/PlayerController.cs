
#define DEBUG_MOVEMENT
#define DEBUG_CONSTANTS

using System.Collections;
using System.Collections.Generic;
using System; // I had to do this to get Math to work, I don't think this is the best way?
using UnityEngine;

public class PlayerController : MonoBehaviour {
	
	public float turningSpeed = 7f;
	
	// Maximum orthogonal movement speed.
	public float movementSpeed = 15f;
	
	// Time a maximum-height, maximum-speed jump takes to reach its apex.
	public float timeToApex = 1f;
	
	public float minJumpHeight = 3f;
	public float maxJumpHeight = 11f;
	
	// Calculated based on movementSpeed, distJumpToLedge, and min/max jump height
	private float jumpVelocity;
	private float gravityOnJumpHeld;
	private float gravityOnJumpRelease;
	
	// Gravity used whenever player is falling.
	public float gravityOnFalling = -20f;
	
	// Gravity used when player is on a surface, to stop player from being pressed into the ground and slowed from friction.
	public float gravityOnSurface = -10f;
	
	// Velocity minimum when gliding.
	public float glideVelocity = -1f;
	
	// Velocity below which it is detected the player has fallen off a ledge.
	public float ledgeSensitivity = -0.1f;
	
	// Note: facingAngle is in degrees. There's no good reason for this except for the fact that it didn't want to work in radians for whatever reason.
	private float facingAngle = 0f;
	
	private float posXLastRenderTick = 0f;
	private float posYLastRenderTick = 0f;
	private float posZLastRenderTick = 0f;
	
	private bool onSurface = true;
	private bool jumpHeld = false;
	private bool glide = false;
	
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
		
		jumpVelocity = 2 * maxJumpHeight / timeToApex;
		gravityOnJumpHeld = -jumpVelocity / timeToApex;
		gravityOnJumpRelease = jumpVelocity / minJumpHeight / 2 - jumpVelocity * jumpVelocity / minJumpHeight;
		
		#if DEBUG_CONSTANTS
		Debug.Log("Jump velocity: " + jumpVelocity);
		Debug.Log("Jump held gravity: " + gravityOnJumpHeld);
		Debug.Log("Jump released gravity: " + gravityOnJumpRelease);
		#endif
		
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
		
		float gravity;
		bool gliding = false;
		if (selfRigidbody.velocity.y >= 0) {
			if (jumpHeld) {
				gravity = gravityOnJumpHeld;
			} else if (!onSurface) {
				gravity = gravityOnJumpRelease;
			} else {
				gravity = gravityOnSurface;
			}
		} else {
			gravity = gravityOnFalling;
			gliding = glide;
		}
		
		// Handle gravity here instead of letting the physics engine do it, so that we can customize its strength.
		selfRigidbody.AddForce(transform.up * gravity, ForceMode.Acceleration);
		
		// If gliding and the player would now be falling faster than the glider allows, set the velocity properly.
		if (gliding && selfRigidbody.velocity.y < glideVelocity) {
			selfRigidbody.velocity = new Vector3(selfRigidbody.velocity.x, glideVelocity, selfRigidbody.velocity.z);
		}
		
		if (selfRigidbody.velocity.y == 0 && !onSurface && !jumpHeld) {
			// This is supposed to detect when the player has set themself onto the ground (their vertical velocity will be zero)
			// However, I suppose it's theoretically possible for the velocity to precisely equal 0 at the apex of a jump?
			onSurface = true;
			glide = false;
			
			#if DEBUG_MOVEMENT
			Debug.Log("Regained jump.");
			#endif
		} else if (selfRigidbody.velocity.y < ledgeSensitivity && onSurface) {
			// If the player has fallen off an edge, they lose their ability to jump.
			onSurface = false;
			
			#if DEBUG_MOVEMENT
			Debug.Log("Fell off ledge.");
			#endif
		}
	}
	
	// Update is called once per frame of rendering
	void Update () {
		
		float inputForward = Input.GetAxis("Vertical");
		float inputStrafe = Input.GetAxis("Horizontal");
		float inputRotate = Input.GetAxis("Mouse X");
		
		bool inputJumpPress = Input.GetKeyDown(KeyCode.Space);
		bool inputJumpHeld = Input.GetKey(KeyCode.Space);
		
		bool updateNeeded = false;
		
		// If the player's position has changed, the camera must be updated.
		if (posXLastRenderTick != selfTransform.position.x || posYLastRenderTick != selfTransform.position.y || posZLastRenderTick != selfTransform.position.z) {
			posXLastRenderTick = selfTransform.position.x;
			posYLastRenderTick = selfTransform.position.y;
			posZLastRenderTick = selfTransform.position.z;
			updateNeeded = true;
		}
		
		// If the direction the player intends to be facing is changing, the camera must be updated.
		if (inputRotate != 0) {
			facingAngle += turningSpeed * inputRotate;
			selfTransform.Rotate(0, turningSpeed * inputRotate, 0);
			updateNeeded = true;
		}
		
		// Jumping:
		if (inputJumpPress) {
			if (onSurface) {
				onSurface = false;
				jumpHeld = true;
				selfRigidbody.AddForce(transform.up * jumpVelocity, ForceMode.VelocityChange);
				
				#if DEBUG_MOVEMENT
				Debug.Log("Jumped.");
				#endif
			} else {
				glide = !glide;
				
				#if DEBUG_MOVEMENT
				if (glide) {
					Debug.Log("Started gliding.");
				} else {
					Debug.Log("Stopped gliding.");
				}
				#endif
			}
		} else if (!inputJumpHeld && jumpHeld) {
			jumpHeld = false;
			
			#if DEBUG_MOVEMENT
			Debug.Log("Jump released.");
			#endif
		}
		
		// Orthogonal movement:
		float dX = 0.0f;
		float dZ = 0.0f;
		if (inputForward != 0 || inputStrafe != 0) {
			
			// Get the direction of the input; forward has an angle of 0, right strafing has an angle of pi / 2
			// Backward should have an angle of 1 pi, but the range of inverse sin is from -pi / 2 to pi / 2; forward and backward appear identical to asin
			double inputMagnitude = Math.Sqrt(inputForward * inputForward + inputStrafe * inputStrafe);
			
			double inputAngle = Math.Asin(inputStrafe / inputMagnitude);
			
			// Handle the limited range of inverse sin for backwards movement
			if (inputForward < 0) {
				inputAngle = Math.PI - inputAngle;
			}
			
			// The direction the player will move depends on both the direction the player's facing and the direction of the input
			double movementAngle = facingAngle / 180 * Math.PI + inputAngle;
			
			// This line is trigonometric magic. Don't touch.
			// Dividing inputMagnitude by this number prevents diagonal movement from being ~1.414 times faster than strictly orthogonal movement.
			inputMagnitude /= Math.Sqrt(Math.Pow(Math.Tan(Math.Abs(inputAngle - Math.Round(inputAngle / Math.PI * 2) * Math.PI / 2)), 2) + 1);
			
			// These are negative because that's what made it work ¯\_(ツ)_/¯
			dX = (float) (-movementSpeed * inputMagnitude * Math.Sin(movementAngle));
			dZ = (float) (-movementSpeed * inputMagnitude * Math.Cos(movementAngle));
		}
		selfRigidbody.velocity = new Vector3(dX, selfRigidbody.velocity.y, dZ);
		
		if (updateNeeded) {
			UpdateCameraTransform();
		}
	}
}
