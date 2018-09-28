using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public float speed = 0.5f;

	private Transform selfTransform;

	void Start () {
		selfTransform = this.GetComponent<Transform> ();
	}

	// Update is called once per frame
	void Update () {
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

        if (inputX != 0 || inputY != 0)
        {
            float tempX = selfTransform.position.x + inputX * speed;
            float tempY = selfTransform.position.y;
            float tempZ = selfTransform.position.z + inputY * speed;

            selfTransform.position = new Vector3(tempX, tempY, tempZ);
        }
    }
}
