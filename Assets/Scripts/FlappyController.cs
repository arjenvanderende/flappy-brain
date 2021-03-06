﻿using UnityEngine;
using System.Collections;

public class FlappyController : MonoBehaviour {

	public float forwardVelocity;
	public float correctionForce;
	public float impulse;

	private Vector3 spawnPosition;
	private bool jump = false;

	void Start() {
		spawnPosition = transform.position;
	}

	void Update () {
		Vector2 forward = new Vector2 (forwardVelocity, 0);
		Vector2 direction = forward + gameObject.rigidbody2D.velocity;
		float angle = Vector2.Angle (forward, direction);
		if (direction.y < 0)
			angle = -angle;

		Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		gameObject.transform.rotation = rotation;
	}

	void FixedUpdate () {
		if (jump) {
			Jump();
			jump = false;
		}

		if (transform.position.x < spawnPosition.x) {
			gameObject.rigidbody2D.AddForce (new Vector2 (correctionForce, 0), ForceMode2D.Force);
		}
	}

	void Jump () {
		// Jumping should always look/work the same, regardless of the current velocity.
		// So when calculating the force to apply to the object, we need to factor that in.
		Vector2 force = new Vector2 (0, impulse) - gameObject.rigidbody2D.velocity;
		Vector2 amplifiedForce = force * gameObject.rigidbody2D.mass;
		gameObject.rigidbody2D.AddForce(amplifiedForce, ForceMode2D.Impulse);
	}

	public void Respawn() {
		jump = false;
		transform.position = spawnPosition;
	}

	public void JumpEvent() {
		jump = true;
	}
}
