using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Class must be inherited from to so the PlayerController can call the Interaction script
 */
public abstract class Interactable : MonoBehaviour {

	public abstract void Interaction ();
}
