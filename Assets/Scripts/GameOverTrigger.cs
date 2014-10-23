using UnityEngine;
using System.Collections;

public class GameOverTrigger : MonoBehaviour {

	public delegate void GameOverEvent();
	public static event GameOverEvent OnGameOver;
	
	void OnTriggerExit2D (Collider2D other) {
		if (other.tag == "Player" && OnGameOver != null) {
			OnGameOver.Invoke();
		}
	}
}
