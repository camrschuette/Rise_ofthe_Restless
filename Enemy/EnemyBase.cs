using UnityEngine;
using System.Collections;
using Exploder;

public enum enemyArchtype
{
	MELEE,
	RANGED,
	HORDE,
	INTELLIGENT,
	MINDLESS,
	BOSS
}

public class EnemyBase : CharacterBase 
{
	// Init Enemy Variables
	public float respawnTimer = 0.0f;
	public int enemyNumber = -1;
	public enemyArchtype enemyType;
	public float damageTaken = 0.0f;
	public bool playerSighted = false;

	// Attack Variables
	public bool attacking = false;
	public float attackDistance = 0f;
	public float giveUpThreshold = 0f;
	public float attackDamage = 0f;

	// Enemy Control Variables
	public bool partOfHorde = false;

	// Manager Code
	public EnemyManager manager;
	protected EnemyArchtypeHorde hordeManager;

	protected override void Start()
	{
		base.Start();
		manager = GameObject.Find("EnemyManager").GetComponent<EnemyManager>();
		//renderer.material.color = Color.blue;
		if (transform.parent != null)
		{
			hordeManager = transform.parent.GetComponent<EnemyArchtypeHorde>();
			//partOfHorde = true;
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (cc.isGrounded)
		{
			forces = new Vector3(forces.x, Mathf.Max(0.0f, forces.y), forces.z);
		}		
		else 
		{
			addForce(new Vector3(0.0f, Physics.gravity.y * 2.0f * Time.deltaTime, 0.0f));
		}
		if (!dead && transform.position.y < -20.0f)
		{
			kill();
		}
	}

	protected void checkForSight(PlayerBase player)
	{
		Vector3 pPos = player.transform.position;
		Vector3 zPos = transform.position;
		pPos.y += 1.0f;
		zPos.y += 1.0f;
		Vector3 direction = Vector3.Normalize(pPos - zPos);
		RaycastHit hit;
		if (Physics.Raycast(zPos, direction, out hit, 30.0f))
	    {
			//Debug.DrawLine(zPos, hit.point);
			if (hit.transform.gameObject.tag == "Player")
				playerSighted = true;
		}

	}

	public override void kill()
	{
		if (!dead)
		{
			dead = true;
			DropGold dg = gameObject.GetComponent<DropGold> ();
			if (dg != null)
			{
				dg.Reward ();
			}
		}
		Destroy(gameObject);
		//GetComponent<Explodable> ().SendMessage ("Boom");
	}

	protected void moveToPosition(Vector3 pos, float dt)
	{
		Vector3 moveVector = pos - transform.position;
		cc.Move(moveVector.normalized * dt * moveSpeed * moveMulti);
	}

	protected void rotateTowardsPoint(Vector3 pos, float dt)
	{
		transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation(new Vector3(pos.x, 0.0f, pos.z) - new Vector3(transform.position.x, 0.0f, transform.position.z)), rotationSpeed*Time.deltaTime);
	}

	protected void moveTowardsPlayer(PlayerBase player, float dt)
	{
		Vector3 toPlayer = Vector3.Normalize(player.transform.position - transform.position);
		Vector3 toCenter = Vector3.zero;
		if (partOfHorde)
		{
			toCenter = Vector3.Normalize(hordeManager.centerPoint - transform.position);
		}
		else
		{
			toCenter = toPlayer;
		}
		// check for obstacles
		Vector3 moveVector = toPlayer * 0.7f + toCenter * 0.3f;
		Vector3 dodgeVector = Vector3.zero;
		if (Physics.Raycast(transform.position, moveVector, Time.deltaTime * moveSpeed))
		{
			dodgeVector = transform.right * Time.deltaTime;
		}
		else
		{
			dodgeVector = toPlayer;
		}
		moveVector = moveVector * 0.4f + dodgeVector * 0.6f;
		// move towards destination
		cc.Move(moveVector * dt * moveSpeed * moveMulti);
	}

	protected void rotateTowardsPlayer(PlayerBase player, float dt)
	{
		transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation(new Vector3(player.transform.position.x, 0.0f, player.transform.position.z) - new Vector3(transform.position.x, 0.0f, transform.position.z)), rotationSpeed*Time.deltaTime);
	}

	protected PlayerBase findClosestPlayer()
	{
		// Find the closest player and see if they are in range
		PlayerBase[] players = PlayerManager.current.players.ToArray();
		float shortestRange = float.PositiveInfinity;
		int closestPlayerIdx = -1;
		for (int i = 0; i < players.Length; i++)
		{
			if (players[i].dead || players[i].visibility == 0 || !PlayerManager.current.selectedCharacters[i])
			{
				continue;
			}
			float sqrRange = Vector3.SqrMagnitude(transform.position - players[i].transform.position);	// squared magnitude is faster
			if (sqrRange < shortestRange)
			{
				shortestRange = sqrRange;
				closestPlayerIdx = i;
			}
		}
		if (closestPlayerIdx == -1)
		{
			return null;
		}
		return players[closestPlayerIdx];
	}

	protected PlayerBase findClosestPlayerInRange(float range)
	{
		// Find the closest player and see if they are in range
		PlayerBase[] players = PlayerManager.current.players.ToArray();
		float shortestRange = float.PositiveInfinity;
		int closestPlayerIdx = 0;
		for (int i = 0; i < players.Length; i++)
		{
			if (players[i].dead || players[i].visibility == 0 || !PlayerManager.current.selectedCharacters[i])
			{
				continue;
			}
			float sqrRange = Vector3.SqrMagnitude(transform.position - players[i].transform.position);	// squared magnitude is faster
			if (sqrRange < shortestRange)
			{
				shortestRange = Mathf.Min(sqrRange, shortestRange);
				closestPlayerIdx = i;
			}
		}
		if (shortestRange <= range * range) // squaring range is faster than square rooting every distance
		{
			return players[closestPlayerIdx];
		}
		return null;
	}

	public IEnumerator slow(){
		moveMulti = 0.5f;

		yield return StartCoroutine(Wait(5.0f));

		moveMulti = 1.0f;
	}

	public void freeze()
	{
		if (moveMulti != 0.0f)
		{
			StartCoroutine(freezeCR());
		}
	}

	private IEnumerator freezeCR()
	{
		moveMulti = 0.0f;

		yield return StartCoroutine (Wait (5.0f));
		
		moveMulti = 1.0f;
	}
}
