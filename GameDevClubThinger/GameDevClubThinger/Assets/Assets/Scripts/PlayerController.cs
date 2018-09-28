using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public float speed = 2f;

	private Transform selfTransform;

	void Start () {
		selfTransform = this.GetComponent<Transform> ();
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetAxis("Horizontal") != 0) {
			float tempx = selfTransform.position.x + (Input.GetAxis ("Horizontal") * speed);
			float tempy = selfTransform.position.y;
			float tempz = selfTransform.position.z;
			Vector3 tempPos = new Vector3 (tempx, tempy, tempz);

			selfTransform.position = tempPos;
		}

		if (Input.GetAxis("Vertical") != 0) {
			float tempx = selfTransform.position.x;
			float tempy = selfTransform.position.y;
			float tempz = selfTransform.position.z + (Input.GetAxis ("Vertical") * speed);
			Vector3 tempPos = new Vector3 (tempx, tempy, tempz);

			selfTransform.position = tempPos;
		}
	}
}
