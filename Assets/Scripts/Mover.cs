using UnityEngine;
using System.Collections;
using System;

public class Mover : MonoBehaviour {

	public float velocity;
	public MoverRange range;

	private float width;

	void Start() {
		gameObject.rigidbody2D.velocity = new Vector2 (velocity, 0f);
		width = gameObject.GetComponent<BoxCollider2D> ().size.x;
	}	

	void Update() {
		if (range.enabled && transform.position.x < range.minX) {
			transform.position += new Vector3(2 * width, 0, 0);
		}
	}
}

[Serializable]
public class MoverRange {

	public bool enabled;
	public float minX;
}
