using UnityEngine;
using System.Collections;

public class DestroyPipeOnCollision : MonoBehaviour {

	void OnTriggerEnter2D(Collider2D other) {
		if (other.tag == "Pipe") {
			Destroy (other.gameObject);
		}
	}
}
