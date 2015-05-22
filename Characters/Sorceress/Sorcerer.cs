using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sorcerer : PlayerBase
{
	public static Sorcerer current = null;

	private int attackType = 1;
	private float timeButtonHeld;
	private float blizzardDamage = 60.0f;
	private bool charging = false;

	public float manaRegenRate = 3.5f;
	public float iceSpikeMana = 12.0f;
	public float fireballMana = 20.0f;
	public float blizzardMana = 40.0f;
	public float meteorMana = 60.0f;

	public AudioClip fireClip;
	public AudioClip iceClip;
	public AudioClip blizzardClip;
	public AudioClip switchClip;

	public ParticleSystem bookCharge;

	public override void Awake()
	{
		if(current == null)
		{
			base.Awake();
			classType = playerClass.SORCERER;
			current = this;
		}
		else
		{
			Destroy(this);
		}
	}

	protected override void Start() 
	{
		base.Start();
		anim.SetBool("IceMode", false);
		anim.SetBool("FireMode", true);
	}
	
	public void OnEnable()
	{
		if(attackType == 0)
		{
			anim.SetBool("IceMode", true);
			anim.SetBool("FireMode", false);
		}
		else if(attackType == 1)
		{
			anim.SetBool("IceMode", false);
			anim.SetBool("FireMode", true);
		}
	}

	protected override void Update(){
		base.Update();
		manaRegen(manaRegenRate);

		if(anim.GetBool("Run"))
	    {
			anim.SetLayerWeight(1, 1f);
		}
		else if(!anim.GetBool("Run"))
	    {
			anim.SetLayerWeight(1, 0f);
		}

		if (charging && !special && Time.time - attackStarted > 0.3f && !bookCharge.isPlaying)
		{
			if (attackType == 0 && checkForMana(blizzardMana))
			{
				bookCharge.Play();
			}
			else if (attackType == 1 && checkForMana(meteorMana))
			{
				bookCharge.Play();
			}
		}
	}

	public override void basicAttack(string dir)
	{
		if(!anim.GetBool("Jump"))
		{
			if(dir == "down")
			{
				//Check enemy facing
				attackStarted = Time.time;
				charging = true;
			}
			float timeSinceAttack = Time.time - attackStarted;
			if (dir == "up")
			{
				//Check with attackType to see which element to use
				if(attackType == 0)
				{
					//When the attack key is released, check to see how long it was
					//held to determine what attack to do.
					if(timeSinceAttack >= 1.0f / attackSpeed && checkForMana(blizzardMana))
					{
						if(!special)
						{
							StartCoroutine(Blizzard());

							GameObject soundObj = new GameObject("sorcblizzard");
							soundObj.transform.position = transform.position;
							AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
							src.clip = blizzardClip;
							src.Play();
							Destroy(soundObj, src.clip.length);
							bookCharge.Stop();
						}
					}
					else if(checkForMana(iceSpikeMana))
					{
						if(!normal)
						{
							StartCoroutine(IceSpike());

							GameObject soundObj = new GameObject("sorciceblast");
							soundObj.transform.position = transform.position;
							AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
							src.clip = iceClip;
							src.Play();
							Destroy(soundObj, src.clip.length);
							bookCharge.Stop();
						}
					}
					charging = false;
				}
				else if(attackType == 1)
				{
					//When the attack key is released, check to see how long it was
					//held to determin what attack to do.
					if(timeSinceAttack >= 1.0f / attackSpeed && checkForMana(meteorMana))
					{
						if(!special)
						{
							StartCoroutine(Meteor());

							GameObject soundObj = new GameObject("sorcmeteor");
							soundObj.transform.position = transform.position;
							AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
							src.clip = fireClip;
							src.Play();
							Destroy(soundObj, src.clip.length);
							bookCharge.Stop();
						}
					}
					else if(checkForMana(fireballMana))
					{
						if(!normal)
						{
							StartCoroutine(Fireball());

							GameObject soundObj = new GameObject("sorcfireball");
							soundObj.transform.position = transform.position;
							AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
							src.clip = fireClip;
							src.Play();
							Destroy(soundObj, src.clip.length);
							bookCharge.Stop();
						}
					}
					charging = false;
				}
			}
		}
	}
	
	public override void classAbility(string dir)
	{
		//When the key is pushed, switch the attack type
		if (dir == "down")
		{
			if(attackType == 1)
			{
				attackType = 0;
				anim.SetBool("IceMode", true);
				anim.SetBool("FireMode", false);
				GetComponentInChildren<Light>().color = new Color(0.6f, 0.6f, 1.0f);
				ChangeClass();
			}
			else
			{
				attackType = 1;
				anim.SetBool("IceMode", false);
				anim.SetBool("FireMode", true);
				GetComponentInChildren<Light>().color = new Color(1.0f, 0.6f, 0.6f);
				ChangeClass();
			}
			GameObject soundObj = new GameObject("sorcswitch");
			soundObj.transform.position = transform.position;
			AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
			src.clip = switchClip;
			src.Play();
			Destroy(soundObj, src.clip.length);
		}
	}

	private IEnumerator Blizzard(){
		special = true;

		useMana (blizzardMana);
		anim.SetTrigger ("SpellSpecial");
		Quaternion startAngle = Quaternion.AngleAxis (-30, Vector3.up);
		Quaternion stepAngle = Quaternion.AngleAxis (5, Vector3.up);

		Quaternion angle = transform.rotation * startAngle;
		Vector3 direction = angle * Vector3.forward;
		Vector3 pos = transform.position;

		List<GameObject> enemies = new List<GameObject> ();

		//Creates an angle of 90 degrees of Raycasting
		for (int i = 0; i < 13; i++) {
			RaycastHit hit;
			if(Physics.Raycast(pos + new Vector3(0,0.5f,0), direction, out hit, 7, LayerMask.GetMask("Enemy")))
				if(!enemies.Contains(hit.transform.gameObject))
					enemies.Add (hit.transform.gameObject);

			direction = stepAngle * direction;
		}

		for(int i=0; i<enemies.Count; i++)
		{
			EnemyBase eBase = enemies[i].GetComponent<EnemyBase>();
			eBase.takeDamage(blizzardDamage * attackMultiplier);
			eBase.freeze();
		}

		direction = angle * Vector3.forward * 7;

		for (int i = 0; i < 13; i++) {
			direction = stepAngle * direction;
		}
		//////////////////////////////////////////

		//This is where we create the animation for the Blizzard
		//attack with ice coming out of the ground
		GameObject Bliz = Instantiate (Resources.Load ("Prefabs/Character/Sorceress/SorceressAbilities/Blizzard"), pos, transform.rotation) as GameObject;
		/*foreach (Transform child in Bliz.transform) {
			foreach (Transform c in child)
				if(c.renderer)
					c.renderer.material.color = new Color (255, 0.0f, 0.0f, 0.0f); 
		}*/
		Destroy (Bliz, 5.0f);

		yield return StartCoroutine (Wait (5.0f / attackSpeed));
		special = false;
	}

	private IEnumerator Fireball(){
		Vector3 mousePos = Input.mousePosition;
		Vector3 sorcPos = Camera.main.WorldToScreenPoint(transform.position);
		Vector3 forPos = Camera.main.WorldToScreenPoint(transform.position + transform.forward);
		Vector3 rightPos = Camera.main.WorldToScreenPoint(transform.position + transform.right);

		Vector3 mouseVec = mousePos - sorcPos;
		Vector3 forVec = forPos - sorcPos;
		Vector3 rightVec = rightPos - sorcPos;
		float angle = Vector2.Angle(forVec, mouseVec);

		if(Vector2.Angle(rightVec, mouseVec) > 90)
			angle = -angle;


		normal = true;

		useMana(fireballMana);
		GetComponent<Animator> ().SetTrigger ("SpellBasic");
		Transform pos = transform.Find("shootPos");
		GameObject Fireball = Instantiate (Resources.Load ("Prefabs/Character/Sorceress/SorceressAbilities/Fireball"), pos.position, transform.rotation) as GameObject;
		if(Mathf.Abs(angle) < 30)
			Fireball.transform.Rotate (transform.up, angle);

		yield return StartCoroutine (Wait (1.5f / attackSpeed));
		normal = false;
	}

	private IEnumerator Meteor(){
		special = true;

		useMana (meteorMana);
		GetComponent<Animator> ().SetTrigger ("SpellSpecial");
		Vector3 pos = transform.position;
		GameObject Meteor = Instantiate (Resources.Load ("Prefabs/Character/Sorceress/SorceressAbilities/Meteor"), pos, transform.rotation) as GameObject;

		yield return StartCoroutine (Wait (5.0f / attackSpeed));
		special = false;
	}

	private IEnumerator IceSpike(){
		normal = true;

		useMana(iceSpikeMana);
		GetComponent<Animator> ().SetTrigger ("SpellBasic");
		Transform pos = transform.Find("shootPos");
		GameObject icicle = Instantiate (Resources.Load ("Prefabs/Character/Sorceress/SorceressAbilities/Icicle_Shot"), pos.position, transform.rotation) as GameObject;
		icicle.transform.up = transform.forward;

		yield return StartCoroutine (Wait (0.5f / attackSpeed));
		normal = false;
	}

	public void AttackStart()
	{
		attacking = true;
	}

	public void AttackEnd()
	{
		attacking = false;
	}
}
