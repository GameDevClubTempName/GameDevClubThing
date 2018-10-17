
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MainCameraController : MonoBehaviour {
	
	// cameraDistance: Distance from player to camera, irrespective of camera angle
	// min/maxCameraAngle: In degrees; negative is looking up above player, positive is looking down at player
	// lookUpBuffer: How many degrees upwards a player needs to try to look before the camera will actually look up
	public float cameraDistance = 10f;
	public float minCameraAngle = -30f;
	public float maxCameraAngle = 89.9f;
	public float lookUpBuffer = 20f;
	
	// yaw = playerTransform.eulerAngles.y
	// pitch is in degrees:
	private float pitch = 45f;
	
	private Transform playerTransform;
	private PlayerController playerController;
	
	public void CameraUpdate() {
		
		// Compute the location of the camera based on which direction it should be looking
		
		float yaw = playerTransform.eulerAngles.y;
		
		float pseudoPitch = pitch;
		if (pitch < 0) {
			pseudoPitch = Math.Min(0, pitch + lookUpBuffer);
		}
		
		double cameraPlanarDistance = cameraDistance * Math.Cos(pseudoPitch / 180 * Math.PI);
		double cameraVerticalDistance = cameraDistance * Math.Sin(Math.Max(0, pitch) / 180 * Math.PI);
		
		float posX = (float) (playerTransform.position.x + cameraPlanarDistance * Math.Sin(yaw / 180 * Math.PI));
		float posY = (float) (playerTransform.position.y + cameraVerticalDistance);
		float posZ = (float) (playerTransform.position.z + cameraPlanarDistance * Math.Cos(yaw / 180 * Math.PI));
		
		transform.position = new Vector3(posX, posY, posZ);
		
		// Change the camera's rotation to be aimed precisely at the player
		transform.LookAt(playerTransform);
		
		if (pitch < 0) {
			transform.Rotate(pseudoPitch, 0, 0);
		}
	}
	
	public void Move(float inPitch, bool doCameraUpdate) {
		
		if (inPitch != 0) {
			
			pitch -= playerController.turningSpeed * inPitch * Time.deltaTime;
			if (pitch < minCameraAngle - lookUpBuffer) {
				pitch = minCameraAngle - lookUpBuffer;
			} else if (pitch > maxCameraAngle) {
				pitch = maxCameraAngle;
			}
			doCameraUpdate = true;
			
		} else if (pitch < 0 && pitch > -lookUpBuffer) {
			pitch = 0;
		}
		
		if (doCameraUpdate) {
			CameraUpdate();
		}
	}
	
	void Start() {
		
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		if (player == null) {
			Debug.Log("Player not found!");
		} else {
			playerTransform = player.GetComponent<Transform>();
			playerController = player.GetComponent<PlayerController>();
		}
	}
}
