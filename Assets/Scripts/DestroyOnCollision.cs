using UnityEngine;
using System.Collections;

public class DestroyOnCollision : MonoBehaviour {

	void OnTriggerEnter2D(Collider2D other) {
		Destroy (other.gameObject);
	}
}
