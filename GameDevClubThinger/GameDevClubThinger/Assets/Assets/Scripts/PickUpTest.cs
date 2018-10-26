using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpTest : Interactable {

	public PlayerController playerController;

	void Awake() {
		GameObject player = GameObject.FindGameObjectWithTag ("Player");
		if (player == null) {
			Debug.Log ("Player not found");
		} else {
			playerController = player.GetComponent<PlayerController> ();
		}
	}

	public override void Interaction() {
		playerController.PickUpItem (gameObject);
	}

}
