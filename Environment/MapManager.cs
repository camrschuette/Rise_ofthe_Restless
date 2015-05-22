using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

// Used to label the main room types
public enum RoomType
{
	STARTER,
	HORDE,
	PUZZLE,
	BOSS,
	TREASURE,
	REACTION
}

// Used to label the four main cardinal directions
public enum Direction
{
	NORTH,
	SOUTH,
	EAST,
	WEST,
	NONE
}

// This graph makes up all of the connected rooms in a dungeon
public class RoomGraph
{
	public List<RoomNode> rooms;
	public List<RoomNode> roomsActive;

	public RoomGraph()
	{
		rooms = new List<RoomNode>();
		roomsActive = new List<RoomNode>();
	}

	public void addRoom(string name, Direction exit, Direction blocked)
	{
		RoomNode room = new RoomNode(name, exit, blocked);
		rooms.Add(room);
	}

	public RoomNode getNodeByRoomName(string roomName)
	{
		for (int i = 0; i < rooms.Count; i++)
		{
			if (rooms[i].name == roomName)
			{
				return rooms[i];
			}
		}
		return null;
	}

	public int getIdxOfBranchRoom()
	{
		for (int i = 0; i < rooms.Count; i++)
		{
			if (rooms[i].getNumNeighbors() > 2)
			{
				return i;
			}
		}
		return -1;
	}

	// Creates a neighboring connection in the graph structure between the two given rooms
	public void connectRooms(RoomNode roomOne, RoomNode roomTwo, Direction roomOneExit)
	{
		switch (roomOneExit)
		{
		case Direction.NORTH:
			roomOne.north = roomTwo;
			roomTwo.south = roomOne;
			break;
		case Direction.SOUTH:
			roomOne.south = roomTwo;
			roomTwo.north = roomOne;
			break;
		case Direction.EAST:
			roomOne.east = roomTwo;
			roomTwo.west = roomOne;
			break;
		case Direction.WEST:
			roomOne.west = roomTwo;
			roomTwo.east = roomOne;
			break;
		default:
			//Debug.Log("Error connecting rooms " + roomOne.name + " and " + roomTwo.name + ": no direction");
			break;
		}
	}
}

// These nodes make up the contents of the RoomGraph. Each RoomNode represents a room in the graph
public class RoomNode
{
	public string name;
	public GameObject obj;
	public RoomType type;
	public bool isBeingLoaded = false;
	public bool hasBeenLoaded = false; // used to keep track of the first time a room is loaded for item management
	public List<GameObject> doors;
	public List<GameObject> items;
	public List<GameObject> playerRespawns;
	public List<EnemySpawner> enemySpawners;
	public RoomNode[] neighbors;
	public List<Direction> mainExits;
	public Direction exitBlocked = Direction.NONE;
	public RoomNode north
	{
		get
		{
			return neighbors[0];
		}
		set
		{
			neighbors[0] = value;
		}
	}
	public RoomNode east
	{
		get
		{
			return neighbors[1];
		}
		set
		{
			neighbors[1] = value;
		}
	}
	public RoomNode south
	{
		get
		{
			return neighbors[2];
		}
		set
		{
			neighbors[2] = value;
		}
	}
	public RoomNode west
	{
		get
		{
			return neighbors[3];
		}
		set
		{
			neighbors[3] = value;
		}
	}

	public RoomNode(string n, Direction exit, Direction block)
	{
		name = n;
		mainExits = new List<Direction>(){exit};
		exitBlocked = block;
		neighbors = new RoomNode[4] {null, null, null, null};
		doors = new List<GameObject>();
		items = new List<GameObject>();
		playerRespawns = new List<GameObject>();
		enemySpawners = new List<EnemySpawner>();
		// set up room type
		if (n.Contains("_H_"))
		{
			type = RoomType.HORDE;
		}
		else if (n.Contains("_P_"))
		{
			type = RoomType.PUZZLE;
		}
		else if (n.Contains("_B_"))
		{
			type = RoomType.BOSS;
		}
		else if (n.Contains("_T_"))
		{
			type = RoomType.TREASURE;
		}
		else if (n.Contains("_R_"))
		{
			type = RoomType.REACTION;
		}
		else
		{
			type = RoomType.STARTER;
		}
	}

	public int getNumNeighbors()
	{
		int count = 0;
		if (north != null) {count++;}
		if (south != null) {count++;}
		if (east != null) {count++;}
		if (west != null) {count++;}
		return count;
	}
}

public class MapManager : MonoBehaviour 
{
	public class RoomPrefab
	{
		public string prefabName;
		public int[] position;
		public Direction exit;
		public Direction exitToBlock = Direction.NONE;
		public string type;

		public RoomPrefab(string name, int[] pos, Direction ex, string rt)
		{
			prefabName = name;
			position = pos;
			exit = ex;
			type = rt;
		}

		public void setName(string name)
		{
			prefabName = name;
		}
	};

	public class SidePath
	{
		public List<RoomPrefab> rooms;
		public int startingRoomIdx;
		public Direction branchDir;

		public SidePath(List<RoomPrefab> r, int idx, Direction dir)
		{
			rooms = r;
			startingRoomIdx = idx;
			branchDir = dir;
		}
	}

	public class ConnectGroup
	{
		public RoomNode roomNode;
		public Direction dir;

		public ConnectGroup(RoomNode r, Direction d)
		{
			roomNode = r;
			dir = d;
		}
	}

	public static MapManager mapManager;
	public ReadSceneNames sceneNames;

	// set to true for procedural map generation, false to use a preset order of rooms
	// note: preset order of rooms has to use the public roomsToLoad array and manually have the rooms
	// connected with map.connectRooms() after the map has been created
	public bool proceduralGeneration = false;	
	// (for procedural map generation) set to true to balance the types of rooms that are being loaded
	public bool roomBalance = true;
	// (for procedural map generation) set to true for every room in the dungeon to be unique
	public bool noRoomRepeats = false;
	// (for procedural map generation) set to true if the dungeon should have branching paths
	public bool branchingPaths = false;

	public RoomGraph map;
	public int numRooms;							// the number of rooms used along the main path, not counting branches
	public int numBranches;							// the number of branches that will be made if procedural generation and branching paths are set
	public string[] roomsToLoad = new string[3];	// temp array used for loading premade dungeons

	public bool allRoomsLoaded = false;

	private int goalHordeRooms;
	private int goalPuzzleRooms;
	private int goalReactionRooms;

	//private string RoomPrefabFilePath = "Assets/Resources/Prefabs/Environment/Resources/";
	//private string roomSceneFilePath = "Assets/Scenes/RoomScenes/";
	//DirectoryInfo dir;
	//FileInfo[] info;

	private int playerSpawnRoom = 0;

	private Queue<ConnectGroup> roomsToConnect; 	// used for asynchronus room loading

	public GameObject doorPrefab;
	public GameObject blockPrefab;
	
	void Awake()
	{
		mapManager = this;
		roomsToConnect = new Queue<ConnectGroup>();
		// Set up the map and the rooms that will be used in the dungeon
		map = new RoomGraph();
		generateDungeon();
	}

	void Update()
	{
		if (roomsToConnect.Count > 0)
		{
			ConnectGroup cg = roomsToConnect.Dequeue();
			if (cg.dir == Direction.NORTH)
			{
				// align positions of doorways properly
				GameObject rNorthObj = cg.roomNode.north.obj;
				if (rNorthObj == null)
				{
					roomsToConnect.Enqueue(cg);
					return;
				}
				rNorthObj.transform.position = cg.roomNode.obj.transform.Find("N_transition").position;
				rNorthObj.transform.position -= rNorthObj.transform.Find("S_transition").position - rNorthObj.transform.position;
				rNorthObj.SetActive(true);
				// set up doorway triggers
				GameObject trigger = cg.roomNode.obj.transform.Find("N_transition").FindChild("trigger").gameObject;
				trigger.SetActive(true);
				trigger.GetComponent<Doorway>().sideA = cg.roomNode;
				trigger.GetComponent<Doorway>().sideB = cg.roomNode.north;
			}
			else if (cg.dir == Direction.SOUTH)
			{
				// align positions of doorways properly
				GameObject rSouthObj = cg.roomNode.south.obj;
				if (rSouthObj == null)
				{
					roomsToConnect.Enqueue(cg);
					return;
				}
				rSouthObj.transform.position = cg.roomNode.obj.transform.Find("S_transition").position;
				rSouthObj.transform.position -= rSouthObj.transform.Find("N_transition").position - rSouthObj.transform.position;
				rSouthObj.SetActive(true);
				// set up doorway triggers
				GameObject trigger = cg.roomNode.obj.transform.Find("S_transition").FindChild("trigger").gameObject;
				trigger.SetActive(true);
				trigger.GetComponent<Doorway>().sideA = cg.roomNode;
				trigger.GetComponent<Doorway>().sideB = cg.roomNode.south;
			}
			else if (cg.dir == Direction.EAST)
			{
				// align positions of doorways properly
				GameObject rEastObj = cg.roomNode.east.obj;
				if (rEastObj == null)
				{
					roomsToConnect.Enqueue(cg);
					return;
				}
				rEastObj.transform.position = cg.roomNode.obj.transform.Find("E_transition").position;
				rEastObj.transform.position -= rEastObj.transform.Find("W_transition").position - rEastObj.transform.position;
				rEastObj.SetActive(true);
				// set up doorway triggers
				GameObject trigger = cg.roomNode.obj.transform.Find("E_transition").FindChild("trigger").gameObject;
				trigger.SetActive(true);
				trigger.GetComponent<Doorway>().sideA = cg.roomNode;
				trigger.GetComponent<Doorway>().sideB = cg.roomNode.east;
			}
			else if (cg.dir == Direction.WEST)
			{
				// align positions of doorways properly
				GameObject rWestObj = cg.roomNode.west.obj;
				if (rWestObj == null)
				{
					roomsToConnect.Enqueue(cg);
					return;
				}
				rWestObj.transform.position = cg.roomNode.obj.transform.Find("W_transition").position;
				rWestObj.transform.position -= rWestObj.transform.Find("E_transition").position - rWestObj.transform.position;
				rWestObj.SetActive(true);
				// set up doorway triggers
				GameObject trigger = cg.roomNode.obj.transform.Find("W_transition").FindChild("trigger").gameObject;
				trigger.SetActive(true);
				trigger.GetComponent<Doorway>().sideA = cg.roomNode;
				trigger.GetComponent<Doorway>().sideB = cg.roomNode.west;
			}

			// check to see if all rooms have been loaded now
			if (roomsToConnect.Count == 0)
			{
				bool done = true;
				foreach (RoomNode room in map.rooms)
				{
					if (!room.hasBeenLoaded)
					{
						done = false;
						break;
					}
				}
				if (done)
				{
					allRoomsLoaded = true;
					PlayerManager.current.doneLoading = true;
				}
			}
		}
	}

	// Called on awake. Generates the dungeon and sets the players in the first room
	private void generateDungeon()
	{
		if (proceduralGeneration)
		{
			// Don't even try to generate the dungeon if we have less than 2 rooms
			if (numRooms < 2)
			{
				//Debug.Log ("Error generating dungeon, too few numRooms specified");
				return;
			}
		
			// Figure out how many of each room we want for a balanced dungeon
			// (Half horde rooms, quarter puzzle rooms, quarter reaction rooms, favor puzzle rooms if odd number)
			if (roomBalance)
			{
				goalHordeRooms = Mathf.CeilToInt((float)(numRooms - 2) / 2.0f);
				goalPuzzleRooms = Mathf.CeilToInt((float)(numRooms - 2 - goalHordeRooms) / 2.0f);
				goalReactionRooms = numRooms - 2 - goalHordeRooms - goalPuzzleRooms;
			}
			// Make sure that numBranches isn't too high
			if (roomBalance && branchingPaths && numBranches > goalHordeRooms)
			{
				//Debug.Log ("Error generating dungeon, too many numBranches specified. Must be less than half the roomNum");
				return;
			}

			// Get references to all of the room prefab files
			//dir = new DirectoryInfo(RoomPrefabFilePath);
			//dir = new DirectoryInfo(roomSceneFilePath);
			//info = dir.GetFiles("*.prefab");
			//info = dir.GetFiles("*.unity");

			// Generate the dungeon with a recursive function that works one room at a time
			List<RoomPrefab> roomsToUse = new List<RoomPrefab>();
			Dictionary<string, int> numRoomTypes = new Dictionary<string, int>(){{"_H_", 0}, {"_P_", 0}, {"_R_", 0}, {"_T_", 0}, {"_S_", 0}, {"_B_", 0}};
			generateRoom(roomsToUse, numRoomTypes);

			// Log an error message if the generation failed
			if (roomsToUse.Count == 0)
			{
				//Debug.Log ("Error generating dungeon, could not generate dungeon from given settings");
				return;
			}

			// Once the main path through the dungeon has been generated, replace a few of the rooms with rooms that can branch
			List<SidePath> sidePaths = new List<SidePath>();
			if (branchingPaths)
			{
				sidePaths = setupSidePaths(roomsToUse);
			}

			// Once all of the rooms have been selected, set up the map and connect the rooms
			foreach (RoomPrefab rp in roomsToUse)
			{
				string rName = rp.prefabName.Substring(0, rp.prefabName.Length - 6);
				map.addRoom(rName, rp.exit, rp.exitToBlock);
			}
			for (int i = 0; i < numRooms - 1; i++)
			{
				map.connectRooms(map.rooms[i], map.rooms[i+1], roomsToUse[i].exit);
			}
			// Now add and connect the side paths to the main dungeon
			if (branchingPaths)
			{
				foreach (SidePath sp in sidePaths)
				{
					int branchRoomIdx = map.rooms.Count;
					map.rooms[sp.startingRoomIdx].mainExits.Add(sp.branchDir);
					
					foreach (RoomPrefab room in sp.rooms)
					{
						map.addRoom(room.prefabName, room.exit, room.exitToBlock);
					}

					map.connectRooms(map.rooms[sp.startingRoomIdx], map.rooms[branchRoomIdx], sp.branchDir);	// connect branching room to first room in branch (horde)
					map.connectRooms(map.rooms[branchRoomIdx], map.rooms[branchRoomIdx+1], sp.rooms[0].exit);	// connect first room to second room (treasure)
				}
			}

			// Load the first room and its neighbors
			StartCoroutine(loadRoomAsync(map.rooms[0]));
			//loadNeighbors(map.rooms[0]);
			for (int i = 0; i < map.rooms.Count; i++)
			{
				loadNeighbors(map.rooms[i]);
			}
		}
		else
		{
			if (roomsToLoad.Length == 0)
			{
				return;
			}
			for (int i = 0; i < roomsToLoad.Length; i++)
			{
				map.addRoom(roomsToLoad[i], Direction.NONE, Direction.NONE);
			}
			// Set up the first room of the dungeon
			StartCoroutine(loadRoomAsync(map.rooms[0]));
			// hardcoded room neighbors, this must be done by hand to get rooms to connect for this method 
			map.connectRooms(map.rooms[0], map.rooms[1], Direction.SOUTH);
			map.connectRooms(map.rooms[1], map.rooms[2], Direction.SOUTH);
			map.connectRooms(map.rooms[2], map.rooms[3], Direction.EAST);
			// Look at the first room's neighbors and set them up
			loadNeighbors(map.rooms[0]);
			/*for (int i = 0; i < map.rooms.Count; i++)
			{
				loadNeighbors(map.rooms[i]);
			}*/
		}
	}

	private bool generateRoom(List<RoomPrefab> rooms, Dictionary<string, int> numRoomTypes)
	{
		// Figure out which entrance the next room has to have to connect with the previous room
		string lookingFor = "";
		int[] nextRoomPos = new int[2]{0,0};

		if (rooms.Count > 0)
		{
			nextRoomPos[0] = rooms[rooms.Count-1].position[0];
			nextRoomPos[1] = rooms[rooms.Count-1].position[1];

			switch (rooms[rooms.Count-1].exit)
			{
			case Direction.NORTH:
				lookingFor = "S";
				nextRoomPos[1]++;
				break;
			case Direction.WEST:
				lookingFor = "E";
				nextRoomPos[0]--;
				break;
			case Direction.EAST:
				lookingFor = "W";
				nextRoomPos[0]++;
				break;
			case Direction.SOUTH:
				lookingFor = "N";
				nextRoomPos[1]--;
				break;
			}
		}

		bool roomSet = false; 								// becomes true when the room is safe to use
		List<string> roomsNotToUse = new List<string>(); 	// a list of all the rooms that we have tried that did not work

		// Continue trying to pick a room until either a suitable one is found or all options are exhausted, upon which we scrap this room and back out to the previous one
		while (!roomSet)
		{
			// Look through all room prefabs and select all of the rooms that can connect with the previous one that haven't been used yet
			List<string> potentialRooms = new List<string>();

			foreach (string f in sceneNames.scenes)
			{
				if (roomsNotToUse.Contains(f))
				{
					continue;
				}

				//string fileName = f.Name.Substring(0, f.Name.Length - 6); // remove ".unity"
				string e = f.Substring(f.LastIndexOf("_") + 1); // get exit directions string

				// Make sure that this room won't overlap with any existing rooms
				if (rooms.Count > 0)
				{
					bool willOverlap = false;
					foreach (RoomPrefab rp in rooms)
					{
						if (rp.position[0] == nextRoomPos[0] && rp.position[1] == nextRoomPos[1])
						{
							willOverlap = true;
							break;
						}
					}
					if (willOverlap)
					{
						roomsNotToUse.Add(f);
						continue;
					}
				}

				// First room
				if (rooms.Count == 0)
				{
					// first room must be starter type
					if (f.Contains("_S_"))
					{
						potentialRooms.Add(f);
					}
				}
				// Final room
				else if (rooms.Count == numRooms - 1)
				{
					// makes sure the final room is a boss room
					if (f.Contains("_B_") && e.Contains(lookingFor))
					{
						potentialRooms.Add(f);
					}
				}
				// Middle rooms
				else
				{
					// Room must have the required entrance to connect with the previous room and must have another exit in addition to that
					if (e.Contains(lookingFor) && e.Length == 2)
					{
						if (noRoomRepeats)	// don't include rooms we have used previously
						{
							bool foundRepeat = false;

							foreach (RoomPrefab rp in rooms)
							{
								if (rp.prefabName == f)
								{
									foundRepeat = true;
									break;
								}
							}

							if (foundRepeat)
							{
								roomsNotToUse.Add(f);
								continue;
							}
						}

						if (roomBalance)	// carry out various actions to balance the room selection
						{
							if (f.Contains(rooms[rooms.Count-1].type))	// don't do the same type of room twice in a row
							{
								roomsNotToUse.Add(f);
								continue;
							}

							if (f.Contains("_H_") && numRoomTypes["_H_"] == goalHordeRooms)
							{
								roomsNotToUse.Add(f);
								continue;
							}
							if (f.Contains("_P_") && numRoomTypes["_P_"] == goalPuzzleRooms)
							{
								roomsNotToUse.Add(f);
								continue;
							}
							if (f.Contains("_R_") && numRoomTypes["_R_"] == goalReactionRooms)
							{
								roomsNotToUse.Add(f);
								continue;
							}
						}

						// If a puzzle or reaction room, make sure that we are using the right entrance to the room
						// (the required entrance will be the first character in e)
						if (f.Contains("_P_") || f.Contains("_R_"))
						{
							if (e[0].ToString() != lookingFor)
							{
								roomsNotToUse.Add(f);
								continue;
							}
						}

						// If we made it through the optional checks, then go ahead and add the room
						potentialRooms.Add(f);
					}
				}
			}
			// If no rooms work for branching from this one, then we have to scrap this one
			if (potentialRooms.Count == 0)
			{
				// If we have a room to go back to, scrap this room and continue generation from the previous one
				if (rooms.Count > 0)
				{
					rooms.RemoveAt(rooms.Count-1);
					return false;
				}
				// Otherwise, we are at the starter room, and generation has failed for all possible room combos, so we give up :(
				else
				{
					break;
				}
			}
			// Select one of these rooms at random and create the room
			int idx = Random.Range(0, potentialRooms.Count);
			string roomName = potentialRooms[idx];
			string thisExit = roomName.Substring(roomName.LastIndexOf("_") + 1);
			Direction exitDir = Direction.NONE;
			if (lookingFor != "")
			{
				thisExit = thisExit.Replace(lookingFor, "");	// currently only supports rooms with 2 entrances
			}
			switch (thisExit)
			{
			case "N":
				exitDir = Direction.NORTH;
				break;
			case "W":
				exitDir = Direction.WEST;
				break;
			case "E":
				exitDir = Direction.EAST;
				break;
			case "S":
				exitDir = Direction.SOUTH;
				break;
			}
			string type = "_S_";
			if (roomName.Contains("_H_"))
			{
				type = "_H_";
			}
			else if (roomName.Contains("_P_"))
			{
				type = "_P_";
			}
			else if (roomName.Contains("_B_"))
			{
				type = "_B_";
			}
			else if (roomName.Contains("_T_"))
			{
				type = "_T_";
			}
			else if (roomName.Contains("_R_"))
			{
				type = "_R_";
			}
			Dictionary<string, int> newNumRoomTypes = new Dictionary<string, int>(numRoomTypes);
			newNumRoomTypes[type]++;
			RoomPrefab newRoom = new RoomPrefab(roomName + ".unity", nextRoomPos, exitDir, type);
			rooms.Add(newRoom);
			roomsNotToUse.Add(roomName + ".unity");
			// If we have all rooms of the dungeon, return a success
			if (rooms.Count == numRooms)
			{
				return true;
			}
			// Set up generation of the next room if not done
			roomSet = generateRoom(rooms, newNumRoomTypes);
		}
		return true;
	}

	private List<SidePath> setupSidePaths(List<RoomPrefab> rooms)
	{
		List<SidePath> pathList = new List<SidePath>();

		// Get a list of all rooms that can be used for branching
		List<string> potentialRooms = new List<string>();
		foreach (string f in sceneNames.scenes)
		{
			//string fileName = f.Name.Substring(0, f.Name.Length - 6); 	// remove ".unity"
			string e = f.Substring(f.LastIndexOf("_") + 1); // get exit directions string
			
			if (e.Length >= 3)
			{
				potentialRooms.Add(f);
			}
		}

		for (int n = 0; n < numBranches; n++)
		{
			// Pick a random room from the dungeon aside from the start and boss room to replace with a branching room
			List<RoomPrefab> sideRooms = new List<RoomPrefab>();
			int roomToBranch = Random.Range(1, rooms.Count - 1);
			int i = roomToBranch;
			Direction branchDir = Direction.NONE;
			do 
			{
				foreach (SidePath sp in pathList)
				{
					if (i == sp.startingRoomIdx)
					{
						continue;
					}
				}

				// Figure out which branching rooms could be used for replacing this room
				string pName = rooms[i].prefabName.Substring(0, rooms[i].prefabName.Length - 6);
				string se = pName.Substring(pName.LastIndexOf("_") + 1);		// room to replace entrances
				List<string> roomChoices = new List<string>();
				
				foreach (string roomName in potentialRooms)
				{
					string pe = roomName.Substring(roomName.LastIndexOf("_") + 1); 	// branching room entrances
					
					// Make sure that the branching room has the same two entrances that the room it's replacing does
					if (pe.Contains(se[0].ToString()) && pe.Contains(se[1].ToString()))
					{
						roomChoices.Add(roomName);
					}
				}

				bool success = false;
				string branchName = "";
				string blockExit = "";

				//Debug.Log (rooms[i].position[0] + " " + rooms[i].position[1]);
				//Debug.Log ("\n");
				
				foreach (string roomName in roomChoices)
				{
					string pe = roomName.Substring(roomName.LastIndexOf("_") + 1);
					pe = pe.Replace(se[0].ToString(), "");
					pe = pe.Replace(se[1].ToString(), "");

					//Debug.Log (pe + "\n");
					foreach (char e in pe)
					{
						if (generateSidePaths(rooms, sideRooms, e.ToString(), new int[2]{rooms[i].position[0], rooms[i].position[1]}))
						{
							branchName = roomName + ".unity";

							switch (e)
							{
							case 'N':
								branchDir = Direction.NORTH;
								break;
							case 'S':
								branchDir = Direction.SOUTH;
								break;
							case 'E':
								branchDir = Direction.EAST;
								break;
							case 'W':
								branchDir = Direction.WEST;
								break;
							}

							blockExit = pe.Replace(e.ToString(), "");
							success = true;
							break;
						}
					}
					if (success)
					{
						break;
					}
				}

				if (success)
				{
					rooms[i].prefabName = branchName;
					switch (blockExit)
					{
					case "N":
						rooms[i].exitToBlock = Direction.NORTH;
						break;
					case "S":
						rooms[i].exitToBlock = Direction.SOUTH;
						break;
					case "E":
						rooms[i].exitToBlock = Direction.EAST;
						break;
					case "W":
						rooms[i].exitToBlock = Direction.WEST;
						break;
					default:
						rooms[i].exitToBlock = Direction.NONE;
						break;
					}
					SidePath sp = new SidePath (sideRooms, i, branchDir);
					pathList.Add(sp);
					break;
				}
				
				// Continue on to the next room in the range of 1 thru rooms.Count - 2
				i = i == rooms.Count - 2 ? 1 : i + 1;
			} while (i != roomToBranch);
		}

		return pathList;
	}

	private bool generateSidePaths(List<RoomPrefab> mainPath, List<RoomPrefab> sidePath, string lastExit, int[] position)
	{
		// Figure out which entrance the next room has to have to connect with the previous room
		bool roomSet = false; 								// becomes true when the room is safe to use
		List<string> roomsNotToUse = new List<string>(); 	// a list of all the rooms that we have tried that did not work

		if (noRoomRepeats)
		{
			foreach (RoomPrefab rp in mainPath)
			{
				roomsNotToUse.Add(rp.prefabName);
			}
		}

		string lookingFor = "";
		switch (lastExit)
		{
		case "N":
			lookingFor = "S";
			position[1]++;
			break;
		case "W":
			lookingFor = "E";
			position[0]--;
			break;
		case "E":
			lookingFor = "W";
			position[0]++;
			break;
		case "S":
			lookingFor = "N";
			position[1]--;
			break;
		}

		// Make sure that this room won't overlap with any of the other rooms in the dungeon
		bool willOverlap = false;
		foreach (RoomPrefab rp in mainPath)
		{
			if (rp.position[0] == position[0] && rp.position[1] == position[1])
			{
				willOverlap = true;
				break;
			}
		}
		if (willOverlap)
		{
			// If we have a room to go back to, scrap this room and continue generation from the previous one
			if (sidePath.Count > 0)
			{
				sidePath.RemoveAt(sidePath.Count-1);
			}
			
			return false;
		}

		//Debug.Log (position [0] + " " + position [1]);
		
		while (!roomSet)
		{
			// Look through all room prefabs and select all of the rooms that can connect with the previous one that haven't been used yet
			List<string> potentialRooms = new List<string>();
			foreach (string f in sceneNames.scenes)
			{
				if (roomsNotToUse.Contains(f))
				{
					continue;
				}
				
				//string fileName = f.Name.Substring(0, f.Name.Length - 6); // remove ".unity"
				string e = f.Substring(f.LastIndexOf("_") + 1); // get exit directions string

				// Make sure the room has the entrance we need to connect
				if (!e.Contains(lookingFor))
				{
					roomsNotToUse.Add(f);
					continue;
				}

				// First room in side path must be a horde room with two entrances
				if (sidePath.Count == 0 && !(f.Contains("_H_") && e.Length == 2))
				{
					roomsNotToUse.Add(f);
					continue;
				}
				// Second room in side path must be a treasure room with one entrance
				else if (sidePath.Count == 1 && !(f.Contains("_T_") && e.Length == 1))
				{
					roomsNotToUse.Add(f);
					continue;
				}

				// If we got here, the room is good to use
				potentialRooms.Add(f);
			}

			// If no rooms work for branching from this one, then we have to scrap this one
			if (potentialRooms.Count == 0)
			{
				// If we have a room to go back to, scrap this room and continue generation from the previous one
				if (sidePath.Count > 0)
				{
					sidePath.RemoveAt(sidePath.Count-1);
				}
				return false;
			}

			// Select one of these rooms at random and create the room
			int idx = Random.Range(0, potentialRooms.Count);
			string roomName = potentialRooms[idx];
			string thisExit = roomName.Substring(roomName.LastIndexOf("_") + 1);
			Direction exitDir = Direction.NONE;
			string before = thisExit;
			if (lookingFor != "")
			{
				thisExit = thisExit.Replace(lookingFor, "");
			}
			//Debug.Log (before + " " + lookingFor + " " + thisExit);
			switch (thisExit)
			{
			case "N":
				exitDir = Direction.NORTH;
				break;
			case "W":
				exitDir = Direction.WEST;
				break;
			case "E":
				exitDir = Direction.EAST;
				break;
			case "S":
				exitDir = Direction.SOUTH;
				break;
			}
			string type = "_H_";
			if (sidePath.Count > 0)
			{
				type = "_T_";
			}
			RoomPrefab newRoom = new RoomPrefab(roomName, new int[2]{position[0], position[1]}, exitDir, type);
			sidePath.Add(newRoom);
			roomsNotToUse.Add(roomName + ".unity");
			// If we have all rooms of the dungeon, return a success
			if (sidePath.Count == 2)
			{
				return true;
			}
			// Set up generation of the next room if not done
			roomSet = generateSidePaths(mainPath, sidePath, thisExit, position);
		}
		return true;
	}
	
	// Loads the room asynchronously and puts it in a queue to be positioned on the next update call. Used for streaming in rooms
	private IEnumerator loadRoomAsync(RoomNode roomNode)
	{
		// Begin loading the room and continue once it is done loading
		roomNode.isBeingLoaded = true;
		AsyncOperation async = Application.LoadLevelAdditiveAsync(roomNode.name);
		yield return async;

		GameObject room = GameObject.Find(roomNode.name);
		roomNode.obj = room;
		map.roomsActive.Add(roomNode);
		roomNode.hasBeenLoaded = true;

		// Spawn the door(s) for the room
		List<GameObject> newRoomDoors = new List<GameObject>();
		foreach (Direction d in roomNode.mainExits)
		{
			if (d != Direction.NONE)
			{
				newRoomDoors.Add(spawnDoor(room, d, doorPrefab));
			}
		}
		// For rooms with branching paths, make opening one door open the rest
		if (newRoomDoors.Count > 1)
		{
			newRoomDoors[0].transform.FindChild("doorTrigger").GetComponent<AutoTrigger>().obj.Add(newRoomDoors[1]);
			newRoomDoors[1].transform.FindChild("doorTrigger").GetComponent<AutoTrigger>().obj.Add(newRoomDoors[0]);
		}
		if (roomNode.exitBlocked != Direction.NONE)
		{
			spawnDoor(room, roomNode.exitBlocked, blockPrefab);
		}
		// Gather all of the room's respawn points
		GameObject[] respawns = GameObject.FindGameObjectsWithTag("Respawn");
		foreach (GameObject r in respawns)
		{
			if (r.transform.root == roomNode.obj.transform)
			{
				roomNode.playerRespawns.Add(r);
			}
		}
		// Set up enemy spawners if they exist in the room
		EnemySpawner[] es = room.GetComponentsInChildren<EnemySpawner>();
		for (int i = 0; i < es.Length; i++)
		{
			roomNode.enemySpawners.Add(es[i]);
		}

		// Set up the key spawn for rooms that have more than one door
		if (roomNode.getNumNeighbors() > 2)
		{
			room.GetComponent<HordeRoom>().multiKey = true;
		}

		// Once the first room has been loaded, notify the player manager so it can spawn the players
		if (map.rooms.IndexOf(roomNode) == 0)
		{
			PlayerManager.current.assignNewSpawnPoints(map.rooms[0].playerRespawns.ToArray());
			PlayerManager.current.setup();
		}
		else
		{
			room.SetActive(false);
		}

		yield return null;
	}

	// Looks at all of the neighbors of the given RoomNode and loads them up if they aren't already loaded. Also performs any extra room setup needed
	public void loadNeighbors(RoomNode roomNode)
	{
		if (roomNode.north != null && !map.roomsActive.Contains(roomNode.north))
		{
			if (!roomNode.north.isBeingLoaded)
			{
				StartCoroutine(loadRoomAsync(roomNode.north));
				roomsToConnect.Enqueue(new ConnectGroup(roomNode, Direction.NORTH));
			}
			else if (roomNode.north.hasBeenLoaded)
			{
				roomNode.north.obj.SetActive(true);
			}
		}
		if (roomNode.south != null && !map.roomsActive.Contains(roomNode.south))
		{
			if (!roomNode.south.isBeingLoaded)
			{
				StartCoroutine(loadRoomAsync(roomNode.south));
				roomsToConnect.Enqueue(new ConnectGroup(roomNode, Direction.SOUTH));
			}
			else if (roomNode.south.hasBeenLoaded)
			{
				roomNode.south.obj.SetActive(true);
			}
		}
		if (roomNode.east != null && !map.roomsActive.Contains(roomNode.east))
		{
			if (!roomNode.east.isBeingLoaded)
			{
				StartCoroutine(loadRoomAsync(roomNode.east));
				roomsToConnect.Enqueue(new ConnectGroup(roomNode, Direction.EAST));
			}
			else if (roomNode.east.hasBeenLoaded)
			{
				roomNode.east.obj.SetActive(true);
			}
		}
		if (roomNode.west != null && !map.roomsActive.Contains(roomNode.west))
		{
			if (!roomNode.west.isBeingLoaded)
			{
				StartCoroutine(loadRoomAsync(roomNode.west));
				roomsToConnect.Enqueue(new ConnectGroup(roomNode, Direction.WEST));
			}
			else if (roomNode.west.hasBeenLoaded)
			{
				roomNode.west.obj.SetActive(true);
			}
		}
	}

	// Called every time a player walks into a new room. Looks at all currently loaded rooms and unloads the ones that players aren't in or adjacent to
	public void unloadEmptyRooms()
	{
		return; // put in place to work around a bug where rooms don't properly load or unload occasionally

		List<RoomNode> roomsToUnload = new List<RoomNode>();
		foreach (RoomNode loadedRoom in map.roomsActive)
		{
			bool keep = false;
			foreach (RoomNode playerRoom in getRoomsPlayersIn())
			{
				if (playerRoom == loadedRoom)
				{
					keep = true;
				}
				foreach (RoomNode neighbor in playerRoom.neighbors)
				{
					if (neighbor == loadedRoom)
					{
						keep = true;
					}
				}
			}
			if (!keep)
			{
				roomsToUnload.Add(loadedRoom);
			}
		}
		foreach (RoomNode n in roomsToUnload)
		{
			unloadRoom(n);
		}
	}

	private void unloadRoom(RoomNode roomNode)
	{
		roomNode.obj.SetActive (false);
		map.roomsActive.Remove(roomNode);
	}

	// spawns the given prefab at the transition point between two rooms (used to spawn doors and door blockers for procedural generation)
	public GameObject spawnDoor(GameObject room, Direction d, GameObject prefab)
	{
		GameObject obj = null;
		if (d == Direction.NORTH)
		{
			obj = Instantiate(prefab, room.transform.FindChild("N_transition").position, Quaternion.Euler(0.0f, 180.0f, 0.0f)) as GameObject;
			obj.transform.position += obj.transform.position - obj.transform.FindChild("spawnRef").position;
			obj.transform.parent = room.transform;
		}
		if (d == Direction.SOUTH)
		{
			obj = Instantiate(prefab, room.transform.FindChild("S_transition").position, Quaternion.identity) as GameObject;
			obj.transform.position += obj.transform.position - obj.transform.FindChild("spawnRef").position;
			obj.transform.parent = room.transform;
		}
		if (d == Direction.EAST)
		{
			obj = Instantiate(prefab, room.transform.FindChild("E_transition").position, Quaternion.Euler(0.0f, 270.0f, 0.0f)) as GameObject;
			obj.transform.position += obj.transform.position - obj.transform.FindChild("spawnRef").position;
			obj.transform.parent = room.transform;
		}
		if (d == Direction.WEST)
		{
			obj = Instantiate(prefab, room.transform.FindChild("W_transition").position, Quaternion.Euler(0.0f, 90.0f, 0.0f)) as GameObject;
			obj.transform.position += obj.transform.position - obj.transform.FindChild("spawnRef").position;
			obj.transform.parent = room.transform;
		}
		return obj;
	}

	// Called whenever a player enters a room. Activates the spawners for that room if a player is entering it for the first time
	public void notifySpawners(RoomNode roomEntered)
	{
		foreach (EnemySpawner es in map.rooms[map.rooms.IndexOf(roomEntered)].enemySpawners)
		{
			if (!es.ableToSpawn)
			{
				es.enableSpawning();
			}
		}
	}

	// Returns a list of RoomNodes of the rooms that the players are currently located in
	public List<RoomNode> getRoomsPlayersIn()
	{
		List<RoomNode> roomsIn = new List<RoomNode>();
		foreach (PlayerBase player in PlayerManager.current.players)
		{
			RoomNode location = player.roomIn;
			if (!roomsIn.Contains(location))
			{
				roomsIn.Add(location);
			}
		}
		return roomsIn;
	}

	// Called whenever a player enters a room. If this room is closer to the end of the dungeon, it assigns new respawn points for the players
	public void updateRespawnPoints(RoomNode room)
	{
		playerSpawnRoom = map.rooms.IndexOf(room);
		PlayerManager.current.assignNewSpawnPoints(room.playerRespawns.ToArray());
	}
}
