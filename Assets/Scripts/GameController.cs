using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	public PipeSpawning pipeSpawning;

	void Start () {
		StartCoroutine (SpawnPipes());
	}

	IEnumerator SpawnPipes () {
		yield return new WaitForSeconds (pipeSpawning.delay);

		while (true) {
			Vector3 pipeSpawnPosition = pipeSpawning.spawn.position + new Vector3(0, Random.Range(-pipeSpawning.heightRange, pipeSpawning.heightRange), 0);
			Instantiate(pipeSpawning.pipe, pipeSpawnPosition, Quaternion.identity);
			yield return new WaitForSeconds (pipeSpawning.delay);
		}
	}
}

[System.Serializable]
public class PipeSpawning {

	public GameObject pipe;
	public Transform spawn;
	public float delay;
	public int heightRange;
}
