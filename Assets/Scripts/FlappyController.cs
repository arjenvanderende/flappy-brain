using UnityEngine;
using System.Collections;

public class FlappyController : MonoBehaviour {

	public float impulse;
	public ForceMode2D mode;

	void Start () {
	}
	
	void FixedUpdate () {
		if (Input.GetButtonDown ("Fire1")) {

			Vector2 force = -gameObject.rigidbody2D.velocity + new Vector2(0, impulse);

			gameObject.rigidbody2D.AddForce(force, mode);
		}
	}
}
