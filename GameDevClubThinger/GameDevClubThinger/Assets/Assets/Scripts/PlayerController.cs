
#define DEBUG_CONSTANTS
#define DEBUG_MOVEMENT

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	
	// movementSpeed: In units per second
	// groundControl: Factor on how easily player can change their velocity while on the ground
	public float movementSpeed = 15f;
	public float groundControl = 15f;
	
	// In degrees per second:
	public float turningSpeed = 300f;
	
	// timeToApex: Time a maximum-height, maximum-speed jump takes to reach its apex
	public float minJumpHeight = 3f;
	public float maxJumpHeight = 11f;
	public float timeToApex = 1f;
	
	// airControl: Factor on how easily player can change their velocity while in the air
	// glideDownSpeed: Downwards velocity target when gliding above glideMinSpeed
	// glideMinSpeed: A percent (range 0-1) of movementSpeed, at which speed gliding will result in 0 gravity and a tendency towards glideDownSpeed
	// glideDrag: A percent (range 0-1) of gravityOnFalling, applied to reduction of gravity even when falling straight down
	public float airControl = 1.5f;
	public float glideDownSpeed = -1f;
	public float glideMinSpeed = 0.9f;
	public float glideDrag = 0.7f;
    public bool canGlide = true;

	// Gravity used whenever player is falling (units per second per second)
	public float gravityOnFalling = -10f;
	
	// Calculated based on min/max jump height and time to apex
	private float jumpVelocity;
	private float gravityOnJumpHeld;
	private float gravityOnJumpRelease;
	
	private float inputVertical = 0f;
	private float inputHorizontal = 0f;
	private float inputMouseX = 0f;
	private float inputMouseY = 0f;
	
	private bool inputQ = false;
	private bool inputE = false;
	private bool inputJumpPress = false;
	private bool inputJumpRelease = false;
	
	private bool isJumpHeld = false;
	private bool isGliding = false;
	
	private bool doCameraUpdate = true;
	
	private Vector3 velocity;
	private Vector3 checkpoint;
	
	private CharacterController controller;
	private MainCameraController cameraController;
	
	void Start() {
		
		LoadValues();
		Cursor.lockState = CursorLockMode.Locked;
	}
	
	void LoadValues() {
		
		controller = this.GetComponent<CharacterController>();
		
		GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
		if (camera == null) {
			Debug.Log("Camera not found!");
		} else {
			cameraController = camera.GetComponent<MainCameraController>();
		}
		
		jumpVelocity = 2 * maxJumpHeight / timeToApex;
		gravityOnJumpHeld = -jumpVelocity / timeToApex;
		gravityOnJumpRelease = jumpVelocity / minJumpHeight / 2 - jumpVelocity * jumpVelocity / minJumpHeight;
		
		#if DEBUG_CONSTANTS
		Debug.Log("Jump velocity: " + jumpVelocity);
		Debug.Log("Jump held gravity: " + gravityOnJumpHeld);
		Debug.Log("Jump released gravity: " + gravityOnJumpRelease);
		#endif
		
		velocity = controller.velocity;
		checkpoint = transform.position;
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
	
	bool GetQ() {
		bool isQ = inputQ;
		inputQ = false;
		return isQ;
	}
	
	bool GetE() {
		bool isE = inputE;
		inputE = false;
		return isE;
	}
	
	void InputUpdate() {
		
		inputVertical = Input.GetAxis("Vertical");
		inputHorizontal = Input.GetAxis("Horizontal");
		inputMouseX = Input.GetAxis("Mouse X");
		inputMouseY = Input.GetAxis("Mouse Y");
		
		inputQ = inputQ || Input.GetKeyDown(KeyCode.Q);
		inputE = inputE || Input.GetKeyDown(KeyCode.E);
		
		inputJumpPress = inputJumpPress || Input.GetKeyDown(KeyCode.Space);
		inputJumpRelease = inputJumpRelease || Input.GetKeyUp(KeyCode.Space);
		
		if (!Input.GetKey(KeyCode.Space) && isJumpHeld) {
			isJumpHeld = false;
			
			#if DEBUG_MOVEMENT
				Debug.Log("Jump released.");
			#endif
		}
	}
	
	float GetPlanarMagnitude() {
		return (float) Math.Sqrt(velocity.x * velocity.x + velocity.z * velocity.z);
	}
	
	void FixedAcceleration(Vector3 force) {
		
		velocity += force * Time.fixedDeltaTime;
	}
	
    void setCanGlide(bool newCanGlide) {
        canGlide = newCanGlide;

        // If the player was gliding, and it is set that they can no longer glide, stop their gliding.
        isGliding = isGliding && newCanGlide;
    }

	void FixedPlayerControl() {
		
		float dX = 0;
		float dZ = 0;
		bool input = false;
		
		if (inputVertical != 0 || inputHorizontal != 0) {
			
			input = true;
			
			// Get the direction of the input; forward has an angle of 0, right strafing has an angle of pi / 2
			// Backward should have an angle of 1 pi, but the range of inverse sin is from -pi / 2 to pi / 2; forward and backward appear identical to asin
			double inputMagnitude = Math.Sqrt(inputVertical * inputVertical + inputHorizontal * inputHorizontal);
			double inputAngle = Math.Asin(inputHorizontal / inputMagnitude);
			
			// Handle the limited range of inverse sin for backwards movement
			if (inputVertical < 0) {
				inputAngle = Math.PI - inputAngle;
			}
			
			// The direction the player will move depends on both the direction the player's facing and the direction of the input
			double movementAngle = transform.eulerAngles.y / 180 * Math.PI + inputAngle;
			
			// This line is trigonometric magic.
			// Dividing inputMagnitude by this number prevents diagonal movement from being ~1.414 times faster than strictly orthogonal movement.
			inputMagnitude /= Math.Sqrt(Math.Pow(Math.Tan(Math.Abs(inputAngle - Math.Round(inputAngle / Math.PI * 2) * Math.PI / 2)), 2) + 1);
			
			dX = (float) (-movementSpeed * inputMagnitude * Math.Sin(movementAngle));
			dZ = (float) (-movementSpeed * inputMagnitude * Math.Cos(movementAngle));
		}
		
		float control;
		
		if (controller.isGrounded) {
			control = groundControl;
		} else {
			control = airControl;
		}
		
		if (!(isGliding && !input)) {
			dX = (dX - velocity.x) * control;
			dZ = (dZ - velocity.z) * control;
			
			FixedAcceleration(new Vector3(dX, 0, dZ));
		}
		
		bool jumpPress = GetJumpPress();

        // Not currently used:
        // bool jumpRelease = GetJumpRelease();

        // Gliding:
        // Note: It may appear that canGlide == false means the player can't untoggle glide,
        // which is why setting canGlide to false should be accompanied by setting isGliding to false.
        // This is handled with setCanGlide().
		if (jumpPress && canGlide && !isJumpHeld && !controller.isGrounded) {
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
		
		bool doCameraUpdate2 = false;
		
		if (inputMouseX != 0) {
			transform.Rotate(0, turningSpeed * inputMouseX * Time.deltaTime, 0);
			doCameraUpdate2 = true;
		}
		
		if (cameraController.Move(inputMouseY, doCameraUpdate2)) {
			doCameraUpdate = false;
		}
		
		if (GetQ() && controller.isGrounded) {
			checkpoint = transform.position;
		} else if (GetE()) {
			transform.position = checkpoint;
			doCameraUpdate = true;
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
		
		if (glide) {
			
			float glidePercent = GetPlanarMagnitude() / movementSpeed;
			
			if (glidePercent >= glideMinSpeed) {
				gravity = (glideDownSpeed - velocity.y) * airControl;
			} else {
				gravity *= (glideDrag / (glideMinSpeed * glideMinSpeed)) * (glidePercent - glideMinSpeed) * (glidePercent - glideMinSpeed);
			}
		}
		
		FixedAcceleration(transform.up * gravity);
	}
	
	void FixedUpdate() {
		
		FixedPlayerControl();
		Gravity();
		
		controller.Move(velocity * Time.fixedDeltaTime);
		velocity = controller.velocity;
		
		if (velocity.x != 0 || velocity.y != 0 || velocity.z != 0) {
			doCameraUpdate = true;
		}
		
		if (controller.isGrounded) {
			isGliding = false;
		}
	}
	
	void Update() {
		
		InputUpdate();
		PlayerControl();
		
		if (doCameraUpdate) {
			cameraController.CameraUpdate();
		}

		if (Input.GetKeyDown ("left shift")) {
			if (canInteract) {
				CallInteraction ();
			}
		}
	}



	private int interactNear = 0;
	private bool canInteract = false;
	public List<Interactable> interactables = new List<Interactable>();
	public List<Transform> interTransform = new List<Transform>();


	private void CallInteraction() {
		float closestDistance;
		Vector3 selfPosition = transform.position;
		int interCount = interactables.Count;
		int index = 0;

		closestDistance = Vector3.Distance(selfPosition, interTransform[0].position);
		if (interCount != 0) {
			for (int i = 1; i < interCount; i++) {
				float tempDistance = Vector3.Distance(selfPosition, interTransform[0].position);
				if (tempDistance < closestDistance) {
					closestDistance = tempDistance;
					index = i;
				}
			}
		}
		interactables [index].Interaction ();
	}

	public void EnterInteraction(GameObject inter) {
		interactNear++;
		canInteract = true;
		interactables.Add (inter.GetComponent<Interactable>());
		interTransform.Add (inter.GetComponent<Transform> ());
	}

	public void ExitInteraction(GameObject inter) {
		interactNear--;
		if (interactNear == 0) {
			canInteract = false;
		} else if (interactNear < 0) {
			Debug.Log ("interactNear == " + interactNear + " ????");
		}
		interactables.Remove (inter.GetComponent<Interactable>());
		interTransform.Remove (inter.GetComponent<Transform> ());
	}
	void onCollisionEnter(Collision col){
		if(col.gameObject.name == "Vennntttttt")
        {
            Destroy(col.gameObject);
        }
		
	}
}
