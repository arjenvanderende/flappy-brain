using UnityEngine;
using System.Collections;

public class Scorer : MonoBehaviour {

	public delegate void ScoreEvent();
	public static event ScoreEvent OnScore;

	void OnTriggerExit2D () {
		if (OnScore != null) {
			OnScore.Invoke();
		}
	}
	
}
