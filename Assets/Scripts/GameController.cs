using UnityEngine;
using System.Collections;
using System;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour {

	public GameObject flappy;
	public EegInput eegInput;
	public PipeSpawning pipeSpawning;

	private FlappyController flappyController;

	void Start () {
		flappyController = flappy.GetComponent<FlappyController> ();
		EegInput.OnBlink += JumpFlappy;

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
		Autopilot ();
	}

	private void Autopilot() {
		Single currentHeight = flappy.rigidbody2D.position.y;
		Single targetHeight = 100;
		if (currentHeight - targetHeight < 0)
			JumpFlappy ();
	}

	private void JumpFlappy() {
		flappyController.JumpEvent ();
	}
}

[Serializable]
public class PipeSpawning {

	public GameObject pipe;
	public Transform spawn;
	public float delay;
	public int heightRange;
}
