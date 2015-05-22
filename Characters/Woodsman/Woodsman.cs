using UnityEngine;
using System.Collections;
using Exploder;

public class Woodsman : PlayerBase
{
	public static Woodsman current = null;

	// objects init
	private Transform shootPosition;
	public GameObject hawk;
	private Transform hawkPos;
	public HawkAI2 hawkScripts;	
	public GameObject bomb;
	public ObjectPool pool;

	// bool inits
	private bool canAttack = true;
	public bool bombActive = false;

	private float specialMana = 30f;
	private float bombMana = 50f;

	// init variables
	private float timeBNAttacks = 5.0f;

	public AudioClip shootClip;
	public AudioClip chargeClip;
	public AudioClip bombClip;

	public ParticleSystem charge;

	public override void Awake()
	{
		if(current == null)
		{
			base.Awake();

			// Initialize the class type of the object
			classType = playerClass.WOODSMAN;

			bomb = Instantiate(bomb) as GameObject;
			bomb.SetActive(false);

			health = 100;
			maxHealth = health;

			mana = maxMana;

			// instantiate the hawk at the hawkspawn position
			hawkPos = transform.Find ("hawkSpawn");

			if(hawk == null || hawkScripts == null)
			{
				hawk = Instantiate(Resources.Load("Prefabs/Character/WoodsMan/Hawk"),hawkPos.position,Quaternion.identity) as GameObject;
				// Get the hawk script to be able to set modes
				hawkScripts = hawk.GetComponent<HawkAI2> ();
			}

			// acquire the position from where to shoot arrows
			shootPosition = transform.Find("shootPos");

			current = this;
		}
		else
		{
			Destroy(this);
		}
	}

	void OnEnable()
	{
		if(enabled)
			hawk.gameObject.SetActive(true);
	}

	protected override void Update()
	{
		// call update of parent class
		base.Update();

		timeBNAttacks -= Time.deltaTime;
		if(timeBNAttacks <= 0.0f)
		{
			timeBNAttacks = 5.0f;
			hawkScripts.enemiesToAttack.Clear ();
		}

		hawk.transform.position = new Vector3 (hawk.transform.position.x, hawkPos.position.y, hawk.transform.position.z);

		if(anim.GetBool("Run") && !anim.GetBool("Jump") && attacking)
			anim.SetLayerWeight(1, 1f);
		else
			anim.SetLayerWeight(1, 0f);
	}

	public override void basicAttack(string dir)
	{
		if(canAttack && !anim.GetBool("Jump"))
		{
			if (dir == "down") 
			{
				attackStarted = Time.time;
				timeBNAttacks = 5.0f;
				if (checkForMana(specialMana))
				{
					Invoke("chargeFX", 0.3f);
				}
			}
			if (dir == "up")
			{
				CancelInvoke("chargeFX");
				float temp = Time.time - attackStarted;
				attackStarted = Time.time;
				if(temp > 0.8f / attackSpeed && checkForMana(specialMana))
				{
					charge.Stop();
					useMana(specialMana);
					anim.SetTrigger("Attack");
					special = true;
					canAttack = false;
					attacking = true;

					GameObject soundObj = new GameObject("woodscharge");
					soundObj.transform.position = transform.position;
					AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
					src.clip = chargeClip;
					src.Play();
					Destroy(soundObj, src.clip.length);
				}
				else
				{
					charge.Stop();
					anim.SetTrigger("Attack");
					special = false;
					canAttack = false;
					attacking = true;

					GameObject soundObj = new GameObject("woodsarrow");
					soundObj.transform.position = transform.position;
					AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
					src.clip = shootClip;
					src.Play();
					Destroy(soundObj, src.clip.length);
				}
			}
		}
	}

	private void chargeFX()
	{
		charge.Play();
	}

	public void GetBasicArrow()
	{
		GameObject arrow = pool.New();
		if(arrow != null)
		{
			arrow.GetComponent<BasicArrow>().basic = true;
			arrow.transform.forward = transform.forward;
			arrow.transform.position = shootPosition.transform.position;
		}
	}
	
	public override void specialAttack()
	{	
		GameObject[] arrows = new GameObject[3];
		for(int i = 0; i < arrows.Length; i++)
		{
			arrows[i] = pool.New();
		}
		if(arrows[0] != null)
		{
			arrows[0].GetComponent<BasicArrow>().basic = false;
			arrows[0].transform.forward = transform.forward;
			arrows[0].transform.position = shootPosition.transform.position;
		}
		Vector3 angle = new Vector3 (0.0f, 12.0f, 0.0f);
		if(arrows[1] != null)
		{
			arrows[1].GetComponent<BasicArrow>().basic = false;
			arrows[1].transform.forward = transform.forward;
			arrows[1].transform.position = shootPosition.transform.position;
			arrows[1].transform.Rotate(angle);
		}
		if(arrows[2] != null)
		{
			arrows[2].GetComponent<BasicArrow>().basic = false;
			arrows[2].transform.forward = transform.forward;
			arrows[2].transform.position = shootPosition.transform.position;
			arrows[2].transform.Rotate(angle * -1);
		}
	}
	
	public override void classAbility(string dir)
	{
		if (dir == "down" && !anim.GetBool("Jump") && canAttack) 
		{
			if(!bombActive && checkForMana(bombMana))
			{
				useMana(bombMana);
				bombActive = true;
				bomb.SetActive(true);
				anim.SetTrigger("Bomb");
				canAttack = false;
				attacking = true;
				ChangeClass();

				GameObject soundObj = new GameObject("woodsbomb");
				soundObj.transform.position = transform.position;
				AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
				src.clip = bombClip;
				src.Play();
				Destroy(soundObj, src.clip.length);
			}
		}
	}

//	public override void Jump(bool jumpDown)
//	{
//		if(!attacking)
//		{
//			base.Jump(jumpDown);
//		}
//		else
//		{
//			base.Jump(false);
//		}
//	}

	public void AttackStart()
	{
		if(special)
		{
			specialAttack();
		}
		else
		{
			GetBasicArrow();
		}
	}

	public void AttackEnd()
	{
		canAttack = true;
		attacking = false;
	}

	public void BombStart()
	{
		canAttack = false;
		attacking = true;
	}

	public void DropBomb()
	{
		bombBehavior.current.DropBomb();
	}
}
