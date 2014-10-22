using UnityEngine;
using System.Collections;

public class FlappyController : MonoBehaviour {

	public float impulse;
	public bool jump;

	void Update () {
		if (Input.GetButtonDown ("Fire1")) {
			jump = true;
		}
	}
	
	void FixedUpdate () {
		if (jump) {
			Jump ();
			jump = false;
		}
	}

	void Jump() {
		Vector2 force = -gameObject.rigidbody2D.velocity + new Vector2(0, impulse);
		gameObject.rigidbody2D.AddForce(force, ForceMode2D.Impulse);
	}

	public void Flap() {
		jump = true;
	}
}
