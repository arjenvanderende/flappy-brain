using UnityEngine;
using System.Collections;
using System;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour {

	public GameObject flappy;
	public EegInput eegInput;
	public GUIText scoreText;
	public PipeSpawning pipeSpawning;
	public AutoPilot autoPilot;

	private FlappyController flappyController;
	private Vector3 flappySize;
	private Vector3 pipeSize;
	private int score;

	void Start () {
		flappyController = flappy.GetComponent<FlappyController> ();
		flappySize = flappy.GetComponent<SpriteRenderer> ().bounds.size;
		pipeSize = pipeSpawning.pipe.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer> ().bounds.size;
		EegInput.OnBlink += JumpFlappy;
		Scorer.OnScore += IncreaseScore;
		scoreText.text = "0";

		StartCoroutine (SpawnPipes());
	}

	private IEnumerator SpawnPipes () {
		yield return new WaitForSeconds (pipeSpawning.delay);

		while (true) {
			Vector3 pipeSpawnPosition = pipeSpawning.spawn.position + new Vector3(0, Random.Range(-pipeSpawning.heightRange, pipeSpawning.heightRange), 0);
			Instantiate(pipeSpawning.pipe, pipeSpawnPosition, Quaternion.identity);
			yield return new WaitForSeconds (pipeSpawning.delay);
		}
	}

	void Update() {
		if (autoPilot.Enabled) {
			GameObject pipe = GetNextPipe ();
			float targetHeight = pipe != null 
				? pipe.transform.position.y - (pipeSpawning.heightBetweenPipes / 2) + (flappySize.y)
				: autoPilot.defaultTargetHeight;
			Autopilot (targetHeight);
		}
	}

	private void Autopilot(float targetHeight) {
		bool shouldJump = flappy.rigidbody2D.position.y < targetHeight;
		bool isConcentrated = autoPilot.mode == AutoPilotMode.EegConcentration && eegInput.IsConcentrated ();
		bool autoPilotAllowed = autoPilot.mode == AutoPilotMode.Automatic || isConcentrated;
		if (shouldJump && autoPilotAllowed) {
			JumpFlappy ();
		}
	}

	private void JumpFlappy() {
		flappyController.JumpEvent ();
	}

	private GameObject GetNextPipe() {
		GameObject[] pipes = GameObject.FindGameObjectsWithTag ("Pipe");
		GameObject nearestPipe = null;
		for (int i = 0; i < pipes.Length; i ++) {
			GameObject pipe = pipes [i];
			if (flappy.transform.position.x - (flappySize.x / 2) < pipe.transform.position.x + (pipeSize.x / 2)) {
				if (nearestPipe == null)
					nearestPipe = pipe;
				else if (nearestPipe.transform.position.x > pipe.transform.position.x)
					nearestPipe = pipe;
			}
		}
		return nearestPipe;
	}

	private void IncreaseScore() {
		score++;
		scoreText.text = score.ToString ();
	}
}

[Serializable]
public class PipeSpawning {

	public GameObject pipe;
	public Transform spawn;
	public float delay;
	public float heightBetweenPipes;
	public int heightRange;
}

[Serializable]
public class AutoPilot {

	public float defaultTargetHeight;
	public AutoPilotMode mode;

	public bool Enabled { 
		get { return mode != AutoPilotMode.Disabled; }
	}
}

public enum AutoPilotMode { 
	Automatic,
	EegConcentration,
	Disabled
}
