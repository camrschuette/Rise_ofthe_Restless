using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class KingRestless : EnemyBase
{
	private Animator myAnimator;
	private PlayerBase closestPlayer;
	public CharacterStats cStat;

	public AudioClip bossMusic;

	public AudioClip basicClip;
	public AudioClip whirlwindClip;
	public AudioClip shockwaveClip;
	public AudioClip roarClip;

	// basic attack
	private float basicAttackRange = 3f;

	// home run 
	private float homeRunAttackRange = 3f;

	// whirlwind
	private float whirlwindSpeedMod = 0.8f;
	private float whirlwindAttackRange = 4.5f;
	private float whirlwindDamageRange = 1.5f;
	private float whirlwindDamage = 15.0f;
	private float whirlwindForceRange = 8.0f;
	private float whirlwindForceMagnitude = 3.0f;
	private float whirlwindInterval = 0.2f;
	private bool spinning = false;
	private GameObject whirl;

	// shockwave
	public GameObject shockwavePrefab;
	private float shockwaveAttackRange = 3.0f;
	private float shockwaveSpawnDistance = 0.25f;

	// room collapse
	public GameObject ceilingBoulder;
	private bool roomCollapsing = false;
	private float boulderError = 4.0f;
	private float boulderFallIntervalLow = 0.4f;
	private float boulderFallIntervalHigh = 0.8f;
	private float boulderFallHeight = 14.0f;

	//firestorm
	public GameObject firestormPrefab;
	private GameObject firestormInstance;
	private bool firestorming = false;

	// general
	public bool attackInProgress = false;
	private Transform roomCenter;
	public int lastAttack = -1;
	public int currentAttack = -1;			// the index of the attack in progress
											// -1 = no attack in progress
											// 0 = basic attack
											// 1 = homerun
											// 2 = shockwave
											// 3 = whirlwind


	protected override void Start()
	{
		base.Start();
		myAnimator = GetComponent<Animator>();
		roomCenter = GameObject.Find("Boss Room Center").transform;
		GameObject gui = GameObject.Find("Boss");
		foreach(Transform t in gui.transform)
		{
			t.gameObject.SetActive(true);
		}
		cStat = gui.GetComponent<CharacterStats>();
		GameObject.Find("Boss/BossName").GetComponent<Text>().text = name.Substring(0, name.Length-7);

		//Start the Boss Music
		AudioSource[] temp = Camera.main.GetComponentsInChildren<AudioSource> ();
		foreach (AudioSource backgroundMusic in temp) 
		{
			backgroundMusic.clip = bossMusic;
			backgroundMusic.Play();
		}

		maxHealth = maxHealth + (PlayerManager.current.players.Count - 1) * 500;
		health = maxHealth;

		GameObject soundObj = new GameObject("kingroar");
		soundObj.transform.position = transform.position;
		AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
		src.clip = roarClip;
		src.Play();
		Destroy(soundObj, src.clip.length);
	}

	void OnDestroy()
	{
		if(cStat.healthBar)
		{
			foreach(Transform t in cStat.healthBar.transform.parent)
			{
				t.gameObject.SetActive(false);
			}
		}

		AudioSource[] temp = gameObject.GetComponentsInParent<AudioSource> ();
		foreach (AudioSource backgroundMusic in temp) 
		{
			backgroundMusic.Stop();
		}

		if (PlayerManager.current != null)
		{
			PlayerManager.current.end_screen ();
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();

		if (!attackInProgress)
		{
			// Check for phase attacks first
			if (health <= maxHealth * 0.3f && !firestorming)
			{
				myAnimator.SetBool("lowhp", true);

				// Move to the center of the room
				//Debug.Log ("WALK IS TRUE");
				myAnimator.SetBool("walk", true);
				moveToPosition(roomCenter.position, Time.deltaTime);
				rotateTowardsPoint(roomCenter.position, Time.deltaTime);

				//do attack
				float dist = Vector3.Distance(transform.position, roomCenter.position);
				if (dist < 0.4f)
				{
					//Debug.Log ("WALK IS FALSE");
					myAnimator.SetBool("walk", false);
					attackInProgress = true;
					firestorming = true;
					myAnimator.SetTrigger("firestorm");

					Vector3 firestormCenter = roomCenter.position;
					firestormInstance = Instantiate(firestormPrefab, firestormCenter, Quaternion.identity) as GameObject;
					firestormInstance.transform.parent = roomCenter.root;

					GameObject soundObj = new GameObject("kingroar");
					soundObj.transform.position = transform.position;
					AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
					src.clip = roarClip;
					src.Play();
					Destroy(soundObj, src.clip.length);
				}
			}
			else if (health <= maxHealth * 0.6f && !roomCollapsing)
			{
				// Move to the center of the room
				//Debug.Log ("WALK IS TRUE");
				myAnimator.SetBool("walk", true);
				moveToPosition(roomCenter.position, Time.deltaTime);
				rotateTowardsPoint(roomCenter.position, Time.deltaTime);

				// HULK SMASH
				float dist = Vector3.Distance(transform.position, roomCenter.position);
				if (dist < 0.4f)
				{
					//Debug.Log ("WALK IS FALSE");
					myAnimator.SetBool("walk", false);
					attackInProgress = true;
					roomCollapsing = true;
					myAnimator.SetTrigger("roomCollapse");

					GameObject soundObj = new GameObject("kingroar");
					soundObj.transform.position = transform.position;
					AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
					src.clip = roarClip;
					src.Play();
					Destroy(soundObj, src.clip.length);
				}
			}

			// If no phase attacks, set up another attack
			else
			{
				// If no attack currently selected, pick one at random
				if (currentAttack == -1)
				{
					float randomTemp = Random.Range(0.0f, 100.0f);

					if (randomTemp < 55.0f)
					{
						currentAttack = 0; // basic attack
					}
					else if (randomTemp < 70.0f)
					{
						currentAttack = 1; // homerun
					}
					else if (randomTemp < 85.0f)
					{
						currentAttack = 2; // shockwave
					}
					else
					{
						currentAttack = 3; // whirlwind
					}
					// if we picked the same attack as last time, just do a basic attack
					if (currentAttack == lastAttack)
					{
						currentAttack = 0;
					}
					lastAttack = currentAttack;
				}

				// Once an attack is set up, move to the appropriate location and begin attack when close enough
				if (currentAttack == 0)
				{
					if (find(basicAttackRange))
					{
						attackInProgress = true;
						myAnimator.SetTrigger("basicAttack");
					}
				}
				else if (currentAttack == 1)
				{
					if (find(homeRunAttackRange))
					{
						attackInProgress = true;
						myAnimator.SetTrigger("homerun");
					}
				}
				else if (currentAttack == 2)
				{
					if (find(shockwaveAttackRange))
					{
						attackInProgress = true;
						myAnimator.SetTrigger("shockwave");
					}
				}
				else if (currentAttack == 3)
				{
					if (find(whirlwindAttackRange))
					{
						attackInProgress = true;
						myAnimator.SetTrigger("whirlwind");
					}
				}
			}
		}
	}

	protected void LateUpdate()
	{
		cStat.ResizeHealthBar(health / maxHealth);
	}

	private bool find(float attackRange)
	{
		closestPlayer = findClosestPlayer();
		if (closestPlayer != null)
		{
			moveTowardsPlayer(closestPlayer, Time.deltaTime);
			rotateTowardsPlayer(closestPlayer, Time.deltaTime);
			if (Vector3.Magnitude(closestPlayer.transform.position - transform.position) < attackRange)
			{
				//Debug.Log ("WALK IS FALSE");
				myAnimator.SetBool("walk", false);
				return true;
			}
		}
		//Debug.Log ("WALK IS TRUE");
		myAnimator.SetBool("walk", true);
		return false;
	}

	public override void kill()
	{
		StopAllCoroutines();
		Destroy(firestormInstance);
		base.kill();
	}

	public void startShockwave()
	{
		GameObject shockwave = Instantiate(shockwavePrefab, transform.position + shockwaveSpawnDistance * transform.forward, Quaternion.LookRotation(transform.forward)) as GameObject;
		shockwave.transform.parent = roomCenter.root;

		GameObject soundObj = new GameObject("kingshockwave");
		soundObj.transform.position = transform.position;
		AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
		src.clip = shockwaveClip;
		src.Play();
		Destroy(soundObj, src.clip.length);
	}

	public void startWhirlwind()
	{
		spinning = true;
		whirl = Instantiate (Resources.Load ("Prefabs/Particles/Wind"), transform.position, transform.rotation) as GameObject;

		GameObject soundObj = new GameObject("kingwhirlwind");
		soundObj.transform.position = transform.position;
		AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
		src.clip = whirlwindClip;
		src.Play();
		Destroy(soundObj, src.clip.length);

		StartCoroutine(whirlwindAttack());
		StartCoroutine(whirlwindPlayerSeek());
	}

	public void endWhirlwind()
	{
		spinning = false;
	}

	private IEnumerator whirlwindAttack()
	{
		//whirl = Instantiate (Resources.Load ("Prefabs/Particles/Wind"), transform.position, transform.rotation) as GameObject;
		//Destroy (whirl, 5.0f);
		LayerMask playerMask = LayerMask.GetMask(new string[]{"Player"});

		while (spinning)
		{
			// Draw all players in within a large range
			Collider[] hit = Physics.OverlapSphere(transform.position, whirlwindForceRange, playerMask);
			foreach (Collider c in hit)
			{
				Vector3 fromPlayer = (transform.position - c.transform.position).normalized;
				fromPlayer *= whirlwindForceMagnitude;
				c.GetComponent<CharacterBase>().addForce(fromPlayer);
			}
			// Damage all players in a small sphere
			hit = Physics.OverlapSphere(transform.position, whirlwindDamageRange, playerMask);
			foreach (Collider c in hit)
			{
				c.GetComponent<PlayerBase>().takeDamage(whirlwindDamage, transform);
			}
			yield return new WaitForSeconds(whirlwindInterval);
		}
		Destroy(whirl);
	}

	private IEnumerator whirlwindPlayerSeek()
	{
		moveMulti *= whirlwindSpeedMod;
		while (spinning)
		{
			whirl.transform.position = transform.position;
			PlayerBase target = findClosestPlayer();
			if (target != null)
			{
				moveTowardsPlayer(target, Time.deltaTime);
				whirl.transform.position = Vector3.MoveTowards(whirl.transform.position, transform.position, Time.deltaTime);
			}
			yield return new WaitForEndOfFrame();
		}
		moveMulti /= whirlwindSpeedMod;
		yield return null;
	}

	public void startRoomCollapse()
	{
		StartCoroutine(roomCollapseAttack());
	}

	private IEnumerator roomCollapseAttack()
	{
		while (roomCollapsing)
		{
			List<PlayerBase> playerList = PlayerManager.current.players;
			int playerIdx = Random.Range(0, playerList.Count);
			Vector3 playerPos = playerList[playerIdx].transform.position;
			Vector3 boulderPos = new Vector3(playerPos.x + Random.Range(-boulderError, boulderError), playerPos.y + boulderFallHeight, playerPos.z + Random.Range(-boulderError, boulderError));
			GameObject boulder = Instantiate(ceilingBoulder, boulderPos, Quaternion.identity) as GameObject;
			boulder.transform.parent = roomCenter.root; // make sure the boulder is spawned as a child of the current room
			yield return new WaitForSeconds(Random.Range(boulderFallIntervalLow, boulderFallIntervalHigh));
		}
		yield return null;
	}

	public void AttackStart(int attackMode)
	{
		MeleeWeapon.current.attackMode = attackMode;
		MeleeWeapon.current.damaging = true;

		GameObject soundObj = new GameObject("kingswing");
		soundObj.transform.position = transform.position;
		AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
		src.clip = basicClip;
		src.Play();
		Destroy(soundObj, src.clip.length);
	}

	public void DamageEnd()
	{
		MeleeWeapon.current.damaging = false;
	}

	public void notifyAttackEnd()
	{
		currentAttack = -1;
		attackInProgress = false;
	}
}
