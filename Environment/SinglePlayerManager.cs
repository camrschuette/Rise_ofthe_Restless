using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SinglePlayerManager : MonoBehaviour 
{
	private int waveNum = -1;
	private int maxWave = 4;
	private bool waveInProgress = false;
	private int numSpawnersComplete = 0;

	private List<GameObject> liveEnemies;
	private List<EnemySpawner> enemySpawners;

	private int[] spawnersPerWave;
	private int[] enemiesPerWave;

	public Text newWaveText;

	void Awake()
	{
		liveEnemies = new List<GameObject>();
		enemySpawners = new List<EnemySpawner>();

		//spawnersPerWave = new int[8]{2, 2, 4, 4, 6, 6, 8, 1};
		//enemiesPerWave = new int[8]{6, 8, 10, 16, 24, 32, 40, 1};
		spawnersPerWave = new int[5]{2, 3, 4, 6, 1};
		enemiesPerWave = new int[5]{6, 8, 10, 12, 1};
	}

	void Start()
	{
		// Set up the player's spawn
		GameObject[] spawns = GameObject.FindGameObjectsWithTag("Respawn");
		PlayerManager.current.assignNewSpawnPoints(spawns);
		// Set up the enemies' spawns
		enemySpawners = new List<EnemySpawner>();
		GameObject[] es = GameObject.FindGameObjectsWithTag("EnemySpawn");
		foreach (GameObject go in es)
		{
			enemySpawners.Add(go.GetComponent<EnemySpawner>());
		}
		PlayerManager.current.setup();
		PlayerManager.current.doneLoading = true;
		StartCoroutine(waitForNextWave());
	}

	void Update()
	{
		if (waveInProgress)
		{
			if (numSpawnersComplete >= spawnersPerWave[waveNum] && liveEnemies.Count == 0)
			{
				waveInProgress = false;
				numSpawnersComplete = 0;
				StartCoroutine(waitForNextWave());
			}
		}
	}

	private IEnumerator waitForNextWave()
	{
		yield return new WaitForSeconds(2.5f);
		startNewWave();
	}

	private void startNewWave()
	{
		waveNum++;
		if (waveNum > maxWave)
		{
			newWaveText.text = "YOU WIN!!";
			return; // game over man
		}
		newWaveText.text = "WAVE " + (waveNum + 1);
		StartCoroutine(delayedSpawning());
		waveInProgress = true;
	}

	private IEnumerator delayedSpawning()
	{
		int numSpawned = 0;
		List<int> spawnersSelected = new List<int>();
		int enemyToSpawn = 0;	// spawn normal zombie
		if (waveNum == maxWave)
		{
			enemyToSpawn = 1;	// spawn king restless
		}
		for (int i = 0; i < spawnersPerWave[waveNum]; i++)
		{
			int rand = Random.Range(0, enemySpawners.Count);
			int start = rand;
			do
			{
				if (!spawnersSelected.Contains(rand))
				{
					int num = Mathf.CeilToInt((float)enemiesPerWave[waveNum] / (float)spawnersPerWave[waveNum]);
					if (numSpawned + num > enemiesPerWave[waveNum])
					{
						num = enemiesPerWave[waveNum] - numSpawned; // makes sure that we don't spawn too many enemies
					}
					numSpawned += num;
					enemySpawners[rand].numberToSpawn[enemyToSpawn] = num;
					enemySpawners[rand].enableSpawning();
					enemySpawners[rand].timeTilSpawn = 0.0f;
					spawnersSelected.Add(rand);
					break;
				}
				rand = (rand + 1) % enemySpawners.Count;
			} while (rand != start);

			yield return new WaitForSeconds(Random.Range(0.8f, 1.5f));
		}
	}

	public void notifySpawnerFinished()
	{
		numSpawnersComplete++;
	}

	public void notifySpawn(GameObject enemy)
	{
		liveEnemies.Add(enemy);
	}

	public void notifyDeath(GameObject enemy)
	{
		liveEnemies.Remove(enemy);
	}
}
