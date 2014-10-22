using UnityEngine;
using System.Collections;

public class Mover : MonoBehaviour {

	public float velocity;

	void Start() {
		gameObject.rigidbody2D.velocity = new Vector2 (velocity, 0f);
	}



	 
}
