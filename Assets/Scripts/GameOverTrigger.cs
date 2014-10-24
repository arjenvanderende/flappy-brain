using UnityEngine;
using System.Collections;

public class GameOverTrigger : MonoBehaviour {

	public delegate void GameOverEvent();
	public static event GameOverEvent OnGameOver;
	
	void OnTriggerExit2D (Collider2D other) {
		bool collidedWithPlayer = other.tag == "Player";
		bool exitedLeftOfCollider = other.transform.position.x < transform.position.x;
		if (collidedWithPlayer && exitedLeftOfCollider && OnGameOver != null) {
			OnGameOver.Invoke();
		}
	}
}
