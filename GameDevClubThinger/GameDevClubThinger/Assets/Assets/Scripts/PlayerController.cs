using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public float speed = 0.5f;

	private Transform selfTransform;
	private int interactNear = 0;
	private bool canInteract = false;
	public List<Interactable> interactables = new List<Interactable>();
	public List<Transform> interTransform = new List<Transform>();

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

		if (Input.GetKeyDown ("left shift")) {
			if (canInteract) {
				CallInteraction ();
			}
		}
    }

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


}
