using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInteract : Interactable {

	public Rigidbody rb;
	public float height;

	void Start() {
		rb = GetComponent<Rigidbody> ();
	}

	public override void Interaction() {
		rb.AddForce (transform.up * height);
		Debug.Log ("Interaction Method called");
	}




}
