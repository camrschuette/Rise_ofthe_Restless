using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum playerClass
{
	WARRIOR, 
	SORCERER, 
	WOODSMAN,
	ROGUE
};

public class PlayerBase : CharacterBase 
{
	public const float maxDistX = 12f;
	public const float maxDistZ = 10f;

	public float attackStarted = 0.0f;
	public bool attacking = false;

	public float respawnTimer = 0.0f;
	public float timeToRespawn = 0.5f;

	public int playerNum = -1;
	public int playerId = -1;

	public bool controllable = true;

	public bool canJump = false;
	public float jumpForce = 15.0f;
	public float verticalVelocity = 0.0f;
	public playerClass classType;

	public PotionType item;

	public float mana = 100.0f;
	public float maxMana = 100.0f;

	public int score = 0;

	protected bool special = false;
	protected bool normal = false;

	public float attackSpeed = 1.0f;

	public RoomNode roomIn;
	
	public CharacterStats cStat;

	public AudioClip drinkClip;

	public int mode = 0;

	public Transform headTransform;

	protected override void Start()
	{
		base.Start();
		jumpForce = 8f;
		moveSpeed = 6.5f;
		rotationSpeed = 1080f;
	}

	protected virtual void Update()
	{
		// Handle respawn timer
		if (dead)
		{
			respawnTimer -= Time.deltaTime;
			if (respawnTimer <= 0.0f)
			{
				respawn();
			}
		}
		else if (transform.position.y < -20.0f)
		{
			kill();
		}
	}

	protected virtual void FixedUpdate()
	{
		base.FixedUpdate();

		if (cc.isGrounded)
		{
			anim.SetBool("Jump", false);
			canJump = true;
			forces.y = Mathf.Max(0.0f, forces.y);
		}

		forces.y += Physics.gravity.y * 2f * Time.deltaTime;
	}

	protected void LateUpdate()
	{
		if(cStat)
		{
			cStat.ResizeHealthBar(health / maxHealth);
			cStat.ResizeManaBar(mana / maxMana);
			cStat.DisplayPotion(item);
			cStat.UpdateScore(score);
		}
	}

	public override void takeDamage(float amount, Transform enemy=null)
	{
		// Gives the players only an invuln period after being hit
		if (currentDamageCooldown > 0.0f || dead)
		{
			return;
		}

		base.takeDamage(amount);
	}

	public override void kill()
	{
		score -= 25;
		if (score < 0)
		{
			score = 0;
		}
		base.kill();
		respawnTimer = timeToRespawn;
	}

	public override void respawn()
	{
		transform.position = PlayerManager.current.getRespawnPoint();
		transform.rotation = Quaternion.identity;

		health = maxHealth;

		verticalVelocity = 0.0f;

		dead = false;
		canJump = false;

		base.respawn();
	}

	public void enterRoom(RoomNode room)
	{
		roomIn = room;
		MapManager.mapManager.notifySpawners(room);
		MapManager.mapManager.loadNeighbors(room);
		MapManager.mapManager.unloadEmptyRooms();
		MapManager.mapManager.updateRespawnPoints(room);

		HordeRoom h = room.obj.GetComponent<HordeRoom>();
		if (h != null)
		{
			h.startSpawning();
		}
	}

	public void addItem(PotionType p)
	{
		item = p;
	}

	public void addScore(int value)
	{
		score += value;
	}

	public void itemAbility()
	{
		if (item != PotionType.NONE) 
		{
			GetComponent<PlayerPotion>().usePotion(this);
			item = PotionType.NONE;

			GameObject soundObj = new GameObject("potiondrink");
			soundObj.transform.position = transform.position;
			AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
			src.clip = drinkClip;
			src.Play();
			Destroy(soundObj, src.clip.length);
		}
	}

	public void useMana(float amt)
	{
		mana -= amt;
		mana = Mathf.Clamp(mana, 0, maxMana);
	}

	public void addMana(float amt)
	{
		mana += amt;
		mana = Mathf.Clamp(mana, 0, maxMana);
	}

	public bool checkForMana(float amt)
	{
		//takes in an amount of mana to check if attack can occur
		if(mana - amt >= 0)
			return true;
		else
			return false;
	}

	public void manaRegen(float perSec)
	{
		//mana regeneration function for any players with mana regenerate.
		mana += perSec * Time.deltaTime;
		mana = Mathf.Clamp(mana, 0, maxMana);
	}

	public virtual void Jump(bool jumpDown)
	{
		//Handle jumping and add it to the movement vector
		if(jumpDown && canJump && !attacking)
		{
			anim.SetBool("Jump", true);
			canJump = false;
			forces.y += jumpForce;
		}
	}

	public virtual void Movement(float x, float z)
	{
		if(dead)
			return;

		Vector3 moveVector = new Vector3(x, 0.0f, z);

		// Rotate the character to face in the direction that they will move
		if(moveVector.magnitude > 0)
			transform.rotation = Quaternion.RotateTowards (transform.rotation, Quaternion.LookRotation(moveVector), rotationSpeed * Time.deltaTime);

		// Process movement

		//if the player's distance from the group's center exceeds maxDist on the x or z
		//axis, they are stopped from moving on that axis. If they're already too far away,
		//they are only allowed to move closer to the center.
		Vector3 movement = moveVector;
		if(movement.magnitude > 1)
			movement.Normalize();
		movement *= moveSpeed * Time.deltaTime * moveMulti;
		Vector3 oldDistVec = transform.position - PlayerManager.current.playersCenter;
		Vector3 newDistVec = oldDistVec + movement;
		
		float newX = Mathf.Abs(newDistVec.x);
		float oldX = Mathf.Abs(oldDistVec.x);
		
		float newZ = Mathf.Abs(newDistVec.z);
		float oldZ = Mathf.Abs(oldDistVec.z);
		
		if(newX > maxDistX && newX > oldX)
		{
			movement.x = 0.0f;
		}
		if(newZ > maxDistZ && newZ > oldZ)
		{
			movement.z = 0.0f;
		}
			
		if(moveVector.magnitude > 0.0f)
		{
			anim.SetBool("Run", true);
		}
		else
		{
			anim.SetBool("Run", false);
		}

		cc.Move(movement);
	}

	public void ChangeClass()
	{
		if (mode == 0) //normal
		{
			cStat.class_abil.sprite = cStat.class_pics [(int)classType+4];
			mode = 1;
			return;
		}

		if (mode == 1) //class ability used
		{
			cStat.class_abil.sprite = cStat.class_pics [(int)classType];
			mode = 0;
			return;
		}
	}

	public virtual void basicAttack(string dir){}
	public virtual void specialAttack(){}
	public virtual void classAbility(string dir){}


}
