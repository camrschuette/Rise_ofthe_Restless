using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Rewired;

public class PlayerManager : MonoBehaviour 
{
	public static PlayerManager current = null;

	public bool normalMode = false;

	// init variables
	public int numPlayers = 0;
	private GameObject[] spawns;
	public List<PlayerBase> players;

	public bool haveKey = false;
	public bool multiKey = false;	// used for rooms that need two doors to open at once

	public GameObject crown;
	private int highScorePlayer = -1;

	public Vector3 playersCenter;
	public bool playersSpawned = false;
	
	public bool[] selectedCharacters;
	public PlayerController[] playerControllers;
	public bool gameStarted = false;

	private AsyncOperation op = null;
	public bool act;
	public bool doneLoading = false;
		
	void Awake()
	{
		if(current == null)
		{
			current = this;
			selectedCharacters = new bool[4];
			playerControllers = new PlayerController[4];
		}
		else
		{
			Destroy(this);
		}
	}

	void Start()
	{
		// instantiate the crown
		crown = Instantiate(crown) as GameObject;
		crown.SetActive(false);
	}

	// NOTE: Had to move this stuff from Start() to a function called by the map manager after switching to scene-loaded rooms; the player manager now has to wait for
	// the map manager to finish setting up the dungeon before it can grab spawn points and spawn the players
	public void setup()
	{
		foreach(PlayerBase pb in players)
		{
			if (MapManager.mapManager.map.rooms.Count > 0)
			{
				pb.roomIn = MapManager.mapManager.map.rooms[0];
			}
		}
		respawnAllPlayers();
		playersSpawned = true;
	}

	public PlayerBase ChangeCharacter(int index)
	{
		int numClasses = System.Enum.GetValues(typeof(playerClass)).Length;
		return players[(index + numClasses) % numClasses];
	}

	public bool SelectCharacter(playerClass pClass)
	{
		if(!selectedCharacters[(int)pClass])
		{
			selectedCharacters[(int)pClass] = true;
			PlayerBase player = players[(int)pClass];
			player.GetComponentInChildren<Light>().enabled = false;
			return true;
		}
		return false;
	}

	public void Update() 
	{
		foreach(PlayerController pc in playerControllers)
		{
			if(pc == null)
				return;
		}
		if (playersSpawned)
		{
			crownUpdate();
			updateCenterLocation();
		}

		bool begin = false;
		if(!gameStarted && !begin)
		{
			foreach(PlayerController pc in playerControllers)
			{
				if(pc.joinState == PlayerController.JoinState.SELECTING)
				{
					begin = false;
					break;
				}
				else if(pc.joinState == PlayerController.JoinState.WAITING)
				{
					begin = true;
				}
			}
		}
		if(begin)
		{
			foreach(PlayerBase pc in players)
			{
				if(!selectedCharacters[(int)pc.classType])
				{
					pc.gameObject.SetActive(false);
				}
			}
			StartCoroutine(LoadMainScene());
		}
		if (gameStarted && doneLoading)
		{
			GameObject ss = GameObject.Find("StartScreen");
			foreach (Transform child in ss.transform)
			{
				if (child.name != "LoadScreenGroup")
				{
					child.gameObject.SetActive(false);
				}
			}
			StartCoroutine(fadeOutLoadingScreen());
			doneLoading = false;
		}
		if(op != null)
		{
			GameObject can = GameObject.Find("LoadScreenGroup");
			if(can != null)
			{
				if(op.progress >= 0.9f)
				{
					DontDestroyOnLoad(can.transform.parent.gameObject);
					op.allowSceneActivation = true;
					Destroy(GameObject.Find("PlayerCameras"));
					Destroy(GameObject.Find("Room"));
					foreach(PlayerController pc in playerControllers)
					{
						pc.Prepare("MainScene");
					}
					//CanvasGroup cg = can.GetComponent<CanvasGroup>();
					//cg.alpha = Mathf.Clamp01(cg.alpha - Time.deltaTime);
				}
				else
				{
					CanvasGroup cg = can.GetComponent<CanvasGroup>();
					cg.alpha = 1.0f;
					//cg.alpha = Mathf.Clamp01(cg.alpha + Time.deltaTime);
				}
			}
		}
	}

	public Vector3 getRespawnPoint()
	{
		int randSpawn = Random.Range (0,4);
		return spawns[randSpawn].transform.position;
	}

	public void assignNewSpawnPoints(GameObject[] newSpawns)
	{
		spawns = newSpawns;
	}

	public void respawnAllPlayers()
	{
		for (int i = 0; i < players.Count; i++)
		{
			if(selectedCharacters[i])
			{
				players[i].transform.position = spawns[i].transform.position;
				players[i].controllable = true;
			}
		}
	}

	public void updateCenterLocation()
	{
		Vector3 center = Vector3.zero;
		for(int i = 0; i < players.Count; i++)
		{
			if(selectedCharacters[i])
			{
				center += players[i].transform.position;
			}
		}
		center = center / numPlayers;

		playersCenter = center;
	}

	public void crownUpdate()
	{
		if (players.Count < 2)
		{
			return;
		}
		for (int i = 0; i < players.Count; i++)
		{
			if (highScorePlayer == -1 && players[i].score > 0 ||
			    highScorePlayer != -1 && players[i].score > players[highScorePlayer].score)
			{
				highScorePlayer = i;
			} 
		}
		if (highScorePlayer != -1)
		{
			crown.transform.SetParent(players[highScorePlayer].GetComponent<PlayerBase>().headTransform);
			//crown.transform.SetParent(players[highScorePlayer].transform);
			crown.transform.localPosition = Vector3.zero;
			//crown.transform.localPosition = new Vector3(0.0f, 2.0f, 0.0f);
			crown.SetActive (true);
		}
	}

	private IEnumerator LoadMainScene()
	{
		foreach(PlayerBase pb in players)
			pb.gameObject.SetActive(false);
		HawkAI2.current.gameObject.SetActive(false);

		if(normalMode)
			op = Application.LoadLevelAdditiveAsync("MainScene");
		else
			op = Application.LoadLevelAdditiveAsync("SinglePlayerRoom");

		op.allowSceneActivation = false;
		gameStarted = true;
		yield return op;
	}

	private IEnumerator fadeOutLoadingScreen()
	{
		yield return new WaitForSeconds(1.75f);
		GameObject can = GameObject.Find("LoadScreenGroup");
		float start = Time.time;
		CanvasGroup cg = can.GetComponent<CanvasGroup>();
		float duration = 2.0f;

		while (Time.time <= start + duration)
		{
			cg.alpha = Mathf.Lerp(1.0f, 0.0f, (Time.time - start) / (start + duration - Time.time));
			yield return null;
		}
		GameObject ss = GameObject.Find("StartScreen");
		Destroy(ss);
		foreach(PlayerController pc in playerControllers)
		{
			if(pc.joinState == PlayerController.JoinState.WAITING)
				pc.joinState = PlayerController.JoinState.PLAYING;
		}
	}

	public void end_screen(){
		GameObject ending = GameObject.Find ("EndScreen");
		List<int> scores = new List<int>();
		List<Sprite> pics = new List<Sprite> ();

		for (int i=0; i < selectedCharacters.Length; i++) {
			if (selectedCharacters[i]){
				players[i].moveSpeed = 0.0f;
				scores.Add(players[i].score);
				pics.Add(players[i].cStat.profile.sprite);
			}
		}

		StartCoroutine ("fade_in", ending);
		ending.GetComponent<coinCountEnd>().num_players = scores.Count;
		ending.SendMessage ("load_scores", scores);
		ending.SendMessage ("load_pics", pics);
	}

	private IEnumerator fade_in(GameObject end){
		float start = Time.time;
		CanvasGroup cg = end.GetComponent<CanvasGroup>();
		float duration = 3.0f;
		
		while (Time.time <= start + duration)
		{
			cg.alpha = Mathf.Lerp(0.0f, 1.0f, (Time.time - start) / (start + duration - Time.time));
			yield return null;
		}
		end.GetComponent<coinCountEnd> ().enabled = true;
	}
}
