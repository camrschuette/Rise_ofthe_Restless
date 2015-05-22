using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HawkAI2 : MonoBehaviour
{
	public static HawkAI2 current = null;

	// init variables
	public int mode;
	public GameObject woodsman;
	public Woodsman script;
	public float dmg = 10.0f;
    public float attackMultiplier = 1.0f;
	public GameObject hawkTarget;

	// vectors needed 
	private Vector3 paddingVector;
	private Vector3 facingVector;
	public Vector3 initialPoint;
	
	private bool arrived = false;
	public bool arrivedPerch = false;
	private bool attacking = false;
	private bool woodsDead = false;

	// Transform needed where to perch on woodsman shoulder
	private Transform perchPos;

	// timer variables
	private float timerPerch;
	private float timerIdle;
	private float timerEnemy;
	private float damageTimer;
	private float speed = 8.0f;

	private bool canScreech = true;
	public AudioClip attackClip;

	// a variable used to store the current enemy the hawk can attack
	public List<GameObject> enemiesToAttack;

	private void Awake()
	{
		if(current == null)
		{
			current = this;
		}
		else
		{
			Destroy(this);
		}
	}

	// Use this for initialization
	void Start () 
	{
		// list of enemies to keep track of for the hawk to attack
		// the hawk will only attack the one that the woodsman has done the most damage to
		enemiesToAttack = new List<GameObject>();

		// get the woodsman to use its forward for movement
		if(script == null || woodsman == null)
		{
			script = Woodsman.current;
			woodsman = script.gameObject;
		}

		// setting initial mode to rotating around woodsman
		mode = 1;

		// initializing the initalPoint which is the point the hawk wants to fly to
		// when sent out by player to zero
		initialPoint = Vector3.zero;

		// finding the perch position that is a child of the woodsman to use
		// for when the hawk is perched on the woodsman shoulder
		perchPos = woodsman.transform.Find ("perchPos");

		// initially setting the time the hawk perchs in mode 4, and 
		// how long it idles in mode 1
		timerPerch = Random.Range(15.0f,22.0f);
		timerIdle = Random.Range (15.0f,22.0f);

		// Setting how long the hawk will attack an enemy when set out by player
		// and it comes into contact with an enemy
		timerEnemy = 2.0f;

	}
	
	// Update is called once per frame
	void Update () 
	{
		if(script.dead)
		{
			woodsDead = true;
		}

		if(!script.dead && woodsDead)
		{
			transform.position = woodsman.transform.Find("hawkSpawn").position;
			woodsDead = false;

		}

		if (enemiesToAttack.Count > 0 && !hawkTarget) 
		{
			mode = 2;

			if (canScreech)
			{
				GameObject soundObj = new GameObject("hawkattack");
				soundObj.transform.position = transform.position;
				AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
				src.clip = attackClip;
				src.Play();
				Destroy(soundObj, src.clip.length);
				canScreech = false;
			}
		}

		if(hawkTarget)
		{
			mode = 4;
		}

		// This mode will take the hawk to a trigger and activate it then return to the woodsman
		if(mode == 4)
		{
			Vector3 targetXZ = new Vector3(hawkTarget.transform.position.x,transform.position.y,hawkTarget.transform.position.z);
			Vector3 targetVec = targetXZ - transform.position;
			targetVec.Normalize ();
			transform.forward = -targetVec;
			if(Vector3.Distance(transform.position,targetXZ) > 0.1f)
			{
				transform.position += (targetVec * speed * Time.deltaTime);

			}
			else
			{
				mode = 3;
				hawkTarget = null;
			}
		}


		// Mode 2 is the hawk being sent out by the player using the woodsman
		// class ability, It goes towards the initial point which is just a set distance
		// in front of the woodsman using its forward vector.  If it sees an enemy it then will 
		// attack the enemy and circle it for five seconds before coming back to the woodsman
		if (mode == 2) 
		{
			if(enemiesToAttack.Count == 0)
			{
				mode = 3;
			}

			for(int i=0;i<enemiesToAttack.Count;i++)
			{
				if(enemiesToAttack[i] == null)
				{
					enemiesToAttack.Remove(enemiesToAttack[i]);
				}
			}

			GameObject mostDamaged = null;
			float temp = 0.0f;
			for(int i=0;i<enemiesToAttack.Count;i++)
			{
				if(enemiesToAttack[i] == null)
				{
					continue;
				}
				EnemyBase scr = enemiesToAttack[i].GetComponent<EnemyBase>();
				if(temp == 0.0f || scr.damageTaken > temp)
				{
					mostDamaged = enemiesToAttack[i];
					temp = scr.damageTaken;
				}
			}

			if(enemiesToAttack.Count == 0)
			{
				mode = 3;
			}

			if(mostDamaged)
			{
				Vector3 towardsEnemy = mostDamaged.transform.position - transform.position;
				towardsEnemy.Normalize();
				transform.forward = new Vector3(towardsEnemy.x,0.0f,towardsEnemy.z);
				if(Vector3.Distance(transform.position,mostDamaged.transform.position) > 2.0f)
				{
					transform.position += (towardsEnemy * speed * Time.deltaTime);
				}
//				else
//				{
//					Vector3 enemyPaddingVector = (new Vector3 (transform.position.x, 0, transform.position.z)) - (new Vector3 (mostDamaged.transform.position.x, 0, mostDamaged.transform.position.z));
//					enemyPaddingVector.Normalize();
//					
//					Vector3 enemyFacing = Vector3.Cross(enemyPaddingVector,Vector3.up);
//					transform.up = enemyFacing;
//					
//					transform.RotateAround(mostDamaged.transform.position,Vector3.up,120 * Time.deltaTime);
//
//				}
				timerEnemy = timerEnemy - Time.deltaTime;
				if(timerEnemy <= 0.0f)
				{
                    ifCriticalHit(1.5f, 10);
					mostDamaged.GetComponent<EnemyBase>().takeDamage(dmg);
                    resetAttackMultiplier();
					timerEnemy = 2.0f;
				}

			}

//
//
//			// Making sure the hawk is not a child of the woodsman as it effects movement and
//			// rotation
//			transform.parent = null;
//
//			// Checking to see if there is a enemy that the hawk's sphere collider has 
//			// come into contact with. If it has it sets attacking= true and the only 
//			// movement that it is affected by inside this mode is the stuff in this if block.
//			if(enemyToAttack)
//			{
//				attacking = true;
//
//				// Keep track of the timer for attacking the enemy, if it is zero set enemyToAttack to null
//				// and attacking to false, and switch the mode to 3 which returns it to woodsman.
//				timerEnemy = timerEnemy - Time.deltaTime;
//				if(timerEnemy <= 0.0f)
//				{
//					attacking = false;
//					//enemyToAttack = null;
//					initialPoint = Vector3.zero;
//					timerEnemy = 5.0f;
//					mode = 3;
//				}
//
//				if(attacking)
//				{
//					
//					// This is the timer for damage, it adds time to it and if it hits 1 second
//					// it resets and deals damage to the enemyToAttack the hawk has.
//					damageTimer = damageTimer + Time.deltaTime;
//					if(damageTimer >= 1.0f && enemyToAttack)
//					{
//						EnemyBase scr = enemyToAttack.GetComponent<EnemyBase>();
//						if(scr)
//						{
//							scr.takeDamage (10.0f);
//						}
//						damageTimer = 0.0f;
//					}
//
//					// This just creates a vector towards the enemy to either increase or decrease the hawk's rotation
//					// around the enemyToAttack
//					Vector3 paddingEnemy = (new Vector3 (transform.position.x, 0, transform.position.z)) - (new Vector3 (enemyToAttack.transform.position.x, 0, enemyToAttack.transform.position.z));
//					paddingEnemy.Normalize();
//
//					// This is a vector used to keep the hawk's transform.up to update to make it look
//					// like it is flying around the enemyToAttack properly.
//					Vector3 facingEnemy = Vector3.Cross (paddingEnemy, Vector3.up);
//					transform.up = facingEnemy;
//
//					// Just calculating distance to see if the hawk's rotation radius needs increased or decreased
//					float dist = Vector3.Distance(new Vector3 (transform.position.x, 0, transform.position.z),new Vector3 (enemyToAttack.transform.position.x, 0, enemyToAttack.transform.position.z));
//					
//					if(dist < 1f)
//					{
//						transform.position += paddingEnemy * 1.5f * Time.deltaTime;
//					}
//					else if(dist  > 2.2f)
//					{
//						transform.position += -paddingEnemy * 1.5f * Time.deltaTime;
//					}
//
//					// A call to just rotate around the enemyToAttack
//					transform.RotateAround(enemyToAttack.transform.position,Vector3.up,120 * Time.deltaTime);
//				}
//
//			}
//
//			// Making sure that if the hawk is within a certain distance to the initialPoint
//			// to just set arrived = true
//			if (Vector3.Distance(transform.position,initialPoint) <= 0.1 && !arrived && !attacking)
//			{
//				transform.position = initialPoint;
//				initialPoint = Vector3.zero;
//				arrived = true;
//			}
//
//			// If the hawk isn't currently attacking an enemy make sure to update its 
//			// position towards the initialPoint
//			else if(!attacking && mode == 2)
//			{
//
//
//				Vector3 hawkXZ = new Vector3(transform.position.x,0,transform.position.z);
//				Vector3 woodsXZ = new Vector3(woodsman.transform.position.x,0,woodsman.transform.position.z);
//				
//				if(initialPoint == Vector3.zero)
//				{
//					initialPoint = transform.position + (woodsman.transform.forward * 12.0f) + (woodsXZ-hawkXZ);
//				}
//				Vector3 moveVec = (initialPoint-transform.position);
//				moveVec.Normalize();
//				transform.up = moveVec;
//				transform.position = transform.position + (moveVec * speed * Time.deltaTime);
//			}
//
//			// If the hawk arrived at the initialPoint and hasn't hit an enemy
//			// then depending on how far from the woodsman it currently is, set its
//			// mode to the corresponding value and update the variables needed.
//			if (arrived && !attacking)
//			{
//				if(Vector3.Distance(woodsman.transform.position,transform.position) > 2.4f)
//				{
//					mode = 3;
//				}
//				else 
//				{
//					mode = 1;
//				}
//				initialPoint = Vector3.zero;
//				arrived = false;
//			}
//
//			// Decrement the woodsman's energy as you go out, if you hit zero come back
//			script.useMana(script.hawkCost);
//			if(script.mana <= script.hawkCost)
//			{
//				initialPoint = Vector3.zero;
//				arrived = false;
//				enemyToAttack = null;
//				attacking = false;
//				mode = 3;
//			}
//			
		}

		// Mode 1 is the idle which just rotates around the woodsman.
		if (mode == 1) 
		{
			canScreech = true;
			transform.parent = null;
//			timerPerch = timerPerch - Time.deltaTime;
//			if(timerPerch <= 0.0f)
//			{
//				mode = 0;
//				timerPerch = Random.Range(15.0f,22.0f);
//			}
			paddingVector = (new Vector3 (transform.position.x, 0, transform.position.z)) - (new Vector3 (woodsman.transform.position.x, 0, woodsman.transform.position.z));
			paddingVector.Normalize();
			
			facingVector = Vector3.Cross (paddingVector, Vector3.up);
			transform.forward = facingVector * -1;
			
			float dist = Vector3.Distance(new Vector3 (transform.position.x, 0, transform.position.z),new Vector3 (woodsman.transform.position.x, 0, woodsman.transform.position.z));
			
			if(dist < 2.0f)
			{
				transform.position += paddingVector * 1.5f * Time.deltaTime;
			}
			else if(dist  > 2.2f)
			{
				transform.position += -paddingVector * 4.9f * Time.deltaTime;
			}
			if(dist > 4.3f)
			{
				mode = 3;
			}
			transform.RotateAround (woodsman.transform.position, Vector3.up, 120 * Time.deltaTime);

		}

		// Mode 3 is the return to woodsman, which just creates a vector
		// towards the woodsman and heads there, if it is close enough it switches
		// to Mode 1.
		if (mode == 3) 
		{
			
			Vector3 movement = new Vector3(woodsman.transform.position.x,0.0f,woodsman.transform.position.z) - new Vector3(transform.position.x,0.0f,transform.position.z);
			if(Vector3.Distance(new Vector3(woodsman.transform.position.x,0.0f,woodsman.transform.position.z),new Vector3(transform.position.x,0.0f,transform.position.z)) < 2.4f)
			{
				mode = 1;
			}
			movement.Normalize();
			transform.forward = -movement;
			transform.position = transform.position + (movement * speed * Time.deltaTime);
		}

		// Mode 0 is the perch mode, which just makes it go towards the local transform
		// of perchPos(which is a child of woodsman) and make itself a child of the woodsman to make sure it follows 
		// along with the woodsman as it moves.
//		if (mode == 0) 
//		{
//			if(Vector3.Distance(perchPos.position,transform.position) < 0.3f)
//			{
//				Debug.Log ("Arrived at perch position.");
//				arrivedPerch = true;
//			}
//			if(!arrivedPerch)
//			{
//				Vector3 movement = perchPos.position - transform.position;
//				movement.Normalize();
//				transform.up = movement;
//				transform.position = transform.position + (movement * speed * Time.deltaTime);
//			}
//			if (arrivedPerch)
//			{
//				transform.position = perchPos.position;
//				transform.forward = woodsman.transform.forward;
//				transform.parent = woodsman.transform;
//			}
//			timerIdle = timerIdle - Time.deltaTime;
//			if(timerIdle <= 0.0f)
//			{
//				mode = 1;
//				timerIdle = Random.Range(15.0f,22.0f);
//				transform.parent = null;
//				arrivedPerch = false;
//			}
//
//			float dist = Vector3.Distance(new Vector3 (transform.position.x, 0, transform.position.z),new Vector3 (woodsman.transform.position.x, 0, woodsman.transform.position.z));
//			if (dist > 3.0f)
//			{
//				mode = 3;
//			}
//		}
		
	}


	// Helper function used by the sphere collider of the hawk to set 
	// the enemyToAttack.
	void setTarget(GameObject c)
	{
		hawkTarget = c;
	}
	
	private void OnLevelWasLoaded(int level)
	{
		if(!woodsman.activeSelf)
		{
			gameObject.SetActive(false);
		}
	}
    void ifCriticalHit(float dmgMultiplier, int randomChance)
    {
        // The higher the int that is passed in randomChance, the lower the crit possibility is
        int i = 0;
        int r = Random.Range(0, randomChance);
        if (i == r)
        {
            attackMultiplier = dmgMultiplier;
        }
    }
    
    void resetAttackMultiplier()
    {
        attackMultiplier = 1.0f;
    }}
