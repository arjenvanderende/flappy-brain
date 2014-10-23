using UnityEngine;
using System.Collections;
using System;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour {

	public GameObject flappy;
	public EegInput eegInput;
	public GUIText scoreText;
	public GameObject titleScreen;
	public GameObject gameOverScreen;
	public PipeSpawning pipeSpawning;
	public AutoPilot autoPilot;

	private FlappyController flappyController;
	private Vector3 flappySize;
	private Vector3 pipeSize;
	private int score;

	private GameState gameState;

	void Start () {
		flappyController = flappy.GetComponent<FlappyController> ();
		flappySize = flappy.GetComponent<SpriteRenderer> ().bounds.size;
		pipeSize = pipeSpawning.pipe.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer> ().bounds.size;

		StartTitleScreen ();
	}

	private void StartTitleScreen() {
		titleScreen.SetActive (true);
		flappy.SetActive (false);
		gameOverScreen.SetActive (false);
		scoreText.gameObject.SetActive (false);
		gameState = GameState.TitleScreen;
	}

	private void StartGame() {
		titleScreen.SetActive (false);
		flappy.SetActive (true);
		scoreText.gameObject.SetActive (true);
		scoreText.text = "0";
		gameState = GameState.Playing;

		EegInput.OnBlink += JumpFlappy;
		Scorer.OnScore += IncreaseScore;
		GameOverTrigger.OnGameOver += StartGameOver;

		StartCoroutine (SpawnPipes());
	}

	private void StartGameOver() {
		gameOverScreen.SetActive (true);
		flappy.SetActive (false);
		gameState = GameState.TitleScreen;

		EegInput.OnBlink -= JumpFlappy;
		Scorer.OnScore -= IncreaseScore;
		GameOverTrigger.OnGameOver -= StartGameOver;
	}

	private IEnumerator SpawnPipes () {
		float delayBetweenPipes = pipeSpawning.delayBetweenPipes;

		yield return new WaitForSeconds (pipeSpawning.delayBeforeStart);
		while (gameState == GameState.Playing) {
			// Spawn pipes in wave
			for (int i = 0; i < pipeSpawning.pipesPerWave; i++)
			{
				Vector3 pipeSpawnPosition = pipeSpawning.spawn.position + new Vector3(0, Random.Range(-pipeSpawning.heightRange, pipeSpawning.heightRange), 0);
				Instantiate(pipeSpawning.pipe, pipeSpawnPosition, Quaternion.identity);
				yield return new WaitForSeconds (delayBetweenPipes);
			}

			// Increase difficulty and wait for next wave
			delayBetweenPipes -= pipeSpawning.delayDecreaseAfterWave;
			delayBetweenPipes = Mathf.Max(delayBetweenPipes, pipeSpawning.minimumDelayBetweenPipes);
			yield return new WaitForSeconds (pipeSpawning.delayBetweenWaves);
		}
	}

	void Update() {
		switch (gameState) {
			case GameState.TitleScreen:
				if (Input.GetButton ("Fire1")) {
					StartGame();
				}
				break;
			case GameState.Playing:
				if (autoPilot.Enabled) {
					GameObject pipe = GetNextPipe ();
					float targetHeight = pipe != null 
						? pipe.transform.position.y - (pipeSpawning.heightBetweenPipes / 2) + (flappySize.y)
						: autoPilot.defaultTargetHeight;
					Autopilot (targetHeight);
				}

				if (Input.GetButton ("Fire1")) {
					flappyController.JumpEvent ();
				}
				break;
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
	public float heightBetweenPipes;
	public int heightRange;

	public float delayBeforeStart;

	public int pipesPerWave;
	public float delayBetweenWaves;

	public float delayBetweenPipes;
	public float delayDecreaseAfterWave;
	public float minimumDelayBetweenPipes;
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

public enum GameState {
	TitleScreen,
	Playing
}
