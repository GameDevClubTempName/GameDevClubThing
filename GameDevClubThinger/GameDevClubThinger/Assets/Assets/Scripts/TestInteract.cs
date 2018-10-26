using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Class is a test to check if the interaction system works
 * When Interaction() is called, block will do a jump!
 */

public class TestInteract : Interactable {

	//holds a referenece to the rigidbody
	public Rigidbody rb;

	//holds the height of the jump 
	public float height;


	void Start() {
		rb = GetComponent<Rigidbody> ();
	}
		
	/**
	 * Override the Interaction() method so PlayerController can use it
	 */
	public override void Interaction() {
		rb.AddForce (transform.up * height);
		Debug.Log ("Interaction Method called");
	}




}
