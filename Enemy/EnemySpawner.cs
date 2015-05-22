using UnityEngine;
using System.Collections;

public enum spawnerType
{
	TIMER,
	TRIGGER
}

public class EnemySpawner : MonoBehaviour 
{
	public GameObject spawnController; 		// this object will be notified whenever an enemy is spawned, this can be used to determine when no enemies are left alive
	public spawnerType spawnType;			// determines if the spawner is timer based or trigger based
	public bool infiniteSpawn = false;		// if set to true, enemies will continue to spawn until spawner is shut down
	public bool ableToSpawn = true;			// set to false if the enemies should not spawn immediately
	public bool enemiesRemaining = true;	// becomes false when spawner is spent
	public float spawnTimer = 3.0f;			// enemy will spawn every spawnTimer seconds
	public float timeTilSpawn = 0.0f;		// remaining time until next spawn
	public int[] numberToSpawn;   			// array of ints that correspond with array of enemy prefabs
	public GameObject[] enemiesToSpawn;		// array of enemy prefabs that will be spawned
											// spawner will spawn numberToSpawn[i] of enemiesToSpawn[i] enemy prefabs
											// ex: numberToSpawn[0] = 4
											//	   enemiesToSpawn[0] = zombieprefab
											// 	   spawner will spawn 4 zombies
	void Start()
	{
//		if (numberToSpawn.Length != enemiesToSpawn.Length)
//		{
//			Debug.Log ("numberToSpawn and enemiesToSpawn on " + gameObject.name + "'s EnemySpawner must be the same length");
//		}
	}

	void Update()
	{
		if (enemiesToSpawn.Length > 0 && enemiesRemaining && ableToSpawn)
		{
			if (spawnType == spawnerType.TIMER)
			{
				timeTilSpawn -= Time.deltaTime;
				if (timeTilSpawn <= 0.0f)
				{
					spawnEnemy();
					timeTilSpawn += spawnTimer;
				}
			}
		}
	}

	private void spawnEnemy()
	{
		// Pick a random enemy type to spawn
		int index = Random.Range(0, enemiesToSpawn.Length-1);
		if (!infiniteSpawn)
		{
			int startIndex = index;
			while (numberToSpawn[index] == 0)
			{
				index = (index + 1) % numberToSpawn.Length;
				if (index == startIndex)
				{
					return;
				}
			}
		}
		Vector3 s = new Vector3 (Random.Range(-1.5f, 1.5f), 0.0f, Random.Range(-1.5f, 1.5f));
		GameObject newEnemy = Instantiate(enemiesToSpawn[index], transform.position + s, transform.rotation) as GameObject;
		newEnemy.transform.parent = spawnController.transform;
		EnemyLifeManager elm = newEnemy.AddComponent<EnemyLifeManager>() as EnemyLifeManager;
		elm.setManager(spawnController);

		// Check to see if we have any enemies left to spawn
		if (!infiniteSpawn)
		{
			numberToSpawn[index]--;
			bool canContinue = false;
			for (int i = 0; i < numberToSpawn.Length; i++)
			{
				if (numberToSpawn[i] != 0)
				{
					canContinue = true;
				}
			}
			if (!canContinue)
			{
				enemiesRemaining = false;
				spawnController.SendMessage("notifySpawnerFinished");
			}
		}
	}

	// Call this to instantly spawn an enemy. The only way to spawn an enemy on trigger mode
	public void triggerSpawn()
	{
		if (ableToSpawn)
		{
			spawnEnemy();
		}
	}

	public void enableSpawning()
	{
		ableToSpawn = true;
		enemiesRemaining = true;
	}

	public void disableSpawning()
	{
		ableToSpawn = false;
	}
}
