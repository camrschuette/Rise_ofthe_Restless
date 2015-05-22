using UnityEngine;
using System.Collections;
using Rewired;
using UnityEngine.UI;
using System;

public class PlayerController: MonoBehaviour
{
	public enum JoinState
	{
		INACTIVE,
		SELECTING,
		WAITING,
		PLAYING
	}

	public JoinState joinState = JoinState.INACTIVE;
	public int playerId = 0; // The Rewired player id of this character
	
	private Player player; // The Rewired Player
	
	public PlayerBase character;
	public StartScreenCamera playerCamera;
	public Text playerText;

	private bool isTest = true;

	private float moveX;
	private int moveXPrev;
	private float moveZ;

	private bool attackDown;
	private bool attackUp;
	private bool jumpDown;
	//private bool jumpUp;
	private bool classAbilityDown;
	private bool classAbilityUp;
	private bool itemDown;
	//private bool itemUp;
	private bool startDown;
	//private bool startUp;

	/*private bool start;
	private bool confirm;
	private bool cancel;*/
	
	public AudioClip selectClip;

	[System.NonSerialized] // Don't serialize this so the value is lost on an editor script recompile.
	private bool initialized;

	private void Awake()
	{
		DontDestroyOnLoad(this);
	}

	private void Start()
	{
		//PlayerManager.current.playerControllers[playerId] = this;
	}

	private void Initialize()
	{
		// Get the Rewired Player object for this player.
		player = ReInput.players.GetPlayer(playerId);
		
		
		#if UNITY_WEBPLAYER
		
		string categoryName = "WebBuild";
		foreach (ControllerMap map in player.controllers.maps.GetAllMapsInCategory(categoryName)) {
			map.enabled = true; // set the enabled state on the map
		}
		
		categoryName = "Default";
		foreach (ControllerMap map in player.controllers.maps.GetAllMapsInCategory(categoryName)) {
			map.enabled = false; // set the enabled state on the map
		}
		#endif
		
		if (isTest) 
		{
			string categoryName2 = "TEST";
			foreach (ControllerMap map in player.controllers.maps.GetAllMapsInCategory(categoryName2)) 
			{
				map.enabled = true; // set the enabled state on the map
			}
			
			categoryName2 = "Default";
			foreach (ControllerMap map in player.controllers.maps.GetAllMapsInCategory(categoryName2)) 
			{
				map.enabled = false; // set the enabled state on the map
			}
		}
		
		initialized = true;
	}
	
	private void FixedUpdate () {
		if(!ReInput.isReady) return; // Exit if Rewired isn't ready. This would only happen during a script recompile in the editor.
		if(!initialized) Initialize(); // Reinitialize after a recompile in the editor

		GetInput();
		ProcessInput();
	}
	
	private void GetInput()
	{
		// Get the input from the Rewired Player. All controllers that the Player owns will contribute, so it doesn't matter
		// whether the input is coming from a joystick, the keyboard, mouse, or a custom controller.

		switch(joinState)
		{
		case JoinState.SELECTING:
			moveXPrev = (int)player.GetAxisRawPrev("Move Horizontal"); // get input by name or action id
			moveX = player.GetAxisRaw("Move Horizontal"); // get input by name or action id
			break;
		case JoinState.PLAYING:
			moveX = player.GetAxis("Move Horizontal"); // get input by name or action id
			break;
		}
 
		moveZ = player.GetAxis("Move Vertical");
		attackDown = player.GetButtonDown("X");
		attackUp = player.GetButtonUp ("X");
		classAbilityDown = player.GetButtonDown ("B");
		classAbilityUp = player.GetButtonUp ("B");
		jumpDown = player.GetButtonDown ("A");
		//jumpUp = player.GetButtonUp ("A");
		itemDown = player.GetButtonDown ("Y");
		//itemUp = player.GetButtonUp ("Y");
		startDown = player.GetButtonDown ("Start Button");
		//startUp = player.GetButtonUp ("Start Button");


//		switch(joinState)
//		{
//		case JoinState.INACTIVE:
//			//start
//			break;
//		case JoinState.SELECTING:
//			//raw horiz, start
//			goto case JoinState.INACTIVE;
//		case JoinState.PLAYING:
//			//precise horiz
//			goto default;
//		default:
//			//all other inputs
//			goto case JoinState.INACTIVE;
//		}
	}
	
	private void ProcessInput()
	{
		switch(joinState)
		{
		case JoinState.INACTIVE:
			if(startDown)
			{
				if(playerText != null)
					playerText.text = "Choose a Character!";
				playerText.color = new Color(255f, 0f, 0f, 255f);
				PlayerManager.current.numPlayers++;
				joinState = JoinState.SELECTING;
			}
			break;
		case JoinState.SELECTING:
			bool changeCharacter = true;
			if((int)moveX != moveXPrev && (int)moveX != 0)
			{
				if(playerCamera != null)
				{
					changeCharacter = playerCamera.CameraRotate(moveX);
				}
				if(changeCharacter)
				{
					PlayerBase pBase = PlayerManager.current.ChangeCharacter((int)character.classType - (int)moveX);
					if(pBase)
						character = pBase;
				}
			}
			if(changeCharacter)
			{
				if(attackDown)
				{
					if(PlayerManager.current.SelectCharacter(character.classType))
					{
						GameObject soundObj = new GameObject("playerselect");
						soundObj.transform.position = transform.position;
						DontDestroyOnLoad(soundObj);
						AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
						src.clip = selectClip;
						src.Play();
						Destroy(soundObj, src.clip.length);

						joinState = JoinState.WAITING;
						if(playerText != null)
							playerText.text = "READY!!!";
							playerText.color = new Color(0f, 255f, 0f, 255f);
					}
				}
			}
			break;
		case JoinState.WAITING:
			if(startDown)
			{
				PlayerManager.current.normalMode = !PlayerManager.current.normalMode;
				if(playerText != null)
				{
					Color c = playerText.color;
					c.r = ((int)c.r) ^ 1;
					playerText.color = c;
				}
			}
			break;
		case JoinState.PLAYING:
			// Process fire button down
			if (attackDown) {
				character.basicAttack ("down");	
			}
			
			//process fire button up
			if(attackUp) {
				character.basicAttack("up");
			}
			
			//process class ability button down
			if (classAbilityDown) {
				character.classAbility("down");
			}
			
			//process class ability button up
			if (classAbilityUp) {
				character.classAbility("up");
			}

			//process item button down
			if (itemDown) {
				character.itemAbility();
			}

			character.Jump(jumpDown);

			character.Movement(moveX, moveZ);
			break;
		}
	}

	private void OnLevelWasLoaded(int level)
	{
		if(Application.loadedLevelName == "StartScreen")
		{
			playerCamera = GameObject.Find("PlayerCameras/p" + (playerId+1) + "cam").GetComponent<StartScreenCamera>();
			playerText = GameObject.Find("StartScreen/p" + (playerId+1)).GetComponent<Text>();
			character = PlayerManager.current.players[playerId];
			PlayerManager.current.playerControllers[playerId] = this;
		}
	}

	public void Prepare(string lName)
	{
		if(joinState == JoinState.WAITING)
		{
//			if(Application.loadedLevelName == "MainScene" || Application.loadedLevelName == "SinglePlayerRoom")
//			{
				character.attackStarted = Mathf.Infinity;
				
				moveX = 0f;
				moveXPrev = 0;
				moveZ = 0f;
				
				attackDown = false;
				attackUp = false;
				jumpDown = false;
				classAbilityDown = false;
				classAbilityUp = false;
				itemDown = false;
				startDown = false;
				
				InvokeRepeating("DelayInput", 1f, 1f);
//			}
		}
	}

	public void DelayInput()
	{
		if(MapManager.mapManager.allRoomsLoaded)
		{
			character.gameObject.SetActive(true);
			character.GetComponentInChildren<Light>().enabled = true;
			Camera.main.camera.GetComponent<cameraControl>().targets.Add(character.gameObject);
			character.cStat = GameObject.Find("Player" + (playerId+1)).GetComponent<CharacterStats>();
			character.cStat.ChangeCharacter(character.classType);
			CancelInvoke("DelayInput");
		}
	}
}
































