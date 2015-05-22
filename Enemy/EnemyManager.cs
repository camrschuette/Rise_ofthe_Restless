using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class EnemyManager : MonoBehaviour{
	// init variables
	public int numEnemies;

	private GameObject[] spawns;
	public List<GameObject> enemies;
	public List<enemyArchtype> enemyList;

	void Start() 
	{
		// instantiate enemy list
		enemies = new List<GameObject>();

		// instantiate selectedClasses and populate it
		enemyList = new List<enemyArchtype>();
		
		enemyList.Add(enemyArchtype.MELEE);
		enemyList.Add(enemyArchtype.RANGED);
		enemyList.Add(enemyArchtype.HORDE);
		enemyList.Add(enemyArchtype.INTELLIGENT);
		enemyList.Add(enemyArchtype.MINDLESS);
		// set players to 4 at start
		//numPlayers = 4;
		numEnemies = 4;
		getNewSpawnPoints();
	

		for (int i=0; i<numEnemies; i++)
		{
			switch (enemyList[i])
			{
				case(enemyArchtype.MELEE):
					// load zombie model here
					//GameObject go = Instantiate (Resources.Load("Prefabs/Enemies/Restless_Melee"),spawns[i].transform.position,Quaternion.identity) as GameObject;
					//EnemyArchtypeMelee mel = go.GetComponent<EnemyArchtypeMelee>();
					break;

			}
		}
	}
	public Vector3 getRespawnPoint()
	{
		int randSpawn = Random.Range (0,4);
		return spawns[randSpawn].transform.position;
	}
	
	public void getNewSpawnPoints()
	{
		spawns = GameObject.FindGameObjectsWithTag("Respawn");

	}
	public void respawnAllEnemies()
	{
		for (int i = 0; i < enemies.Count; i++)
		{
			enemies[i].transform.position = spawns[i].transform.position;
			enemies[i].GetComponent<PlayerBase>().controllable = true;
		}
	}
}


