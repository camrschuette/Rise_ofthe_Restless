using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HordeRoom : MonoBehaviour 
{
	private List<GameObject> liveEnemies;
	private List<EnemySpawner> roomSpawners;
	private int numSpawnersComplete = 0;
	private bool keySpawned = false;
	public GameObject keyPrefab;
	public Transform keySpawnPoint;
	public bool multiKey = false;

	void Awake()
	{
		liveEnemies = new List<GameObject>();
		roomSpawners = new List<EnemySpawner>();
	}

	void Start()
	{
		// Get references to all of the enemy spawners in the room
		GameObject[] es = GameObject.FindGameObjectsWithTag("EnemySpawn");
		foreach (GameObject go in es)
		{
			if (go.transform.root == transform)
			{
				roomSpawners.Add(go.GetComponent<EnemySpawner>());
			}
		}
	}

	void Update()
	{
		if (!keySpawned && keySpawnPoint != null)
		{
			// When all spawners are spent and all enemies are dead, open the door
			if (numSpawnersComplete >= roomSpawners.Count && liveEnemies.Count == 0)
			{
				GameObject key = Instantiate(keyPrefab, keySpawnPoint.position, Quaternion.Euler(-90.0f, 0.0f, 0.0f)) as GameObject;
				keySpawned = true;
				if (multiKey)
				{
					key.GetComponent<KeyController>().multiKey = true;
				}
			}
		}
	}

	public void startSpawning()
	{
		foreach (EnemySpawner es in roomSpawners)
		{
			es.enableSpawning();
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
