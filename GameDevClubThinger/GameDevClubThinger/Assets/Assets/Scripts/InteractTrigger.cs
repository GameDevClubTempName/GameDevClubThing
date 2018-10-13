using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractTrigger : MonoBehaviour {

	private PlayerController playerController;
	private GameObject parentGameObject;

	void Awake() {
		GameObject player = GameObject.FindGameObjectWithTag ("Player");
		if (player == null) {
			Debug.Log ("Player not found");
		} else {
			playerController = player.GetComponent<PlayerController> ();
		}

		parentGameObject = transform.parent.gameObject;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag ("Player")) {
			Debug.Log ("Player entered");
			playerController.EnterInteraction (parentGameObject);
		}
	}

	private void OnTriggerExit(Collider other) {
		if (other.CompareTag ("Player")) {
			Debug.Log ("Player exited");
			playerController.ExitInteraction (parentGameObject);
		}
	}
}
