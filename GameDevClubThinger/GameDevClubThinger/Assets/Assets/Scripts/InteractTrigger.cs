using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Script to be placed on a child GameObject to the interactable
 * Child GameObject must have a trigger collider 
 * (A GameObject cannot have a trigger and a regular collider, so a child is needed)
 */
public class InteractTrigger : MonoBehaviour {

	private PlayerController playerController;
	private GameObject parentGameObject;

	/**
	 * Sets a reference to the PlayerController and the interactable this is a parent of
	 */
	void Awake() {
		GameObject player = GameObject.FindGameObjectWithTag ("Player");
		if (player == null) {
			Debug.Log ("Player not found");
		} else {
			playerController = player.GetComponent<PlayerController> ();
		}

		parentGameObject = transform.parent.gameObject;
	}
		
	/**
	 * When the trigger is activated by the player, send the information of the parent to the PlayerController
	 */
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag ("Player")) {
			Debug.Log ("Player entered");
			playerController.EnterInteraction (parentGameObject);
		}
	}

	/**
	 * When the player leaves the trigger, instruct PlayerController to remove references to the parent
	 */
	private void OnTriggerExit(Collider other) {
		if (other.CompareTag ("Player")) {
			Debug.Log ("Player exited");
			playerController.ExitInteraction (parentGameObject);
		}
	}
}
