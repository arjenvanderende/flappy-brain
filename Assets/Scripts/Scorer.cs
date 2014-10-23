using UnityEngine;
using System.Collections;

public class Scorer : MonoBehaviour {

	public delegate void ScoreEvent();
	public static event ScoreEvent OnScore;

	void OnTriggerExit2D (Collider2D other) {
		if (other.tag == "Player" && OnScore != null) {
			OnScore.Invoke();
		}
	}
	
}
