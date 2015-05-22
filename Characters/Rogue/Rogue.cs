using UnityEngine;
using System.Collections;
using Exploder;

public class Rogue : PlayerBase
{
	public static Rogue current = null;
	private bool dash = false;
	public float dashDur = 0.25f;
	public float dashSpeed = 25.0f;
	public float dashMana = 30.0f;
	public float stealthMana = 8.0f;
	public float manaRegenRate = 5f;
	private bool charging = false;

	public AudioClip attackClip;
	public AudioClip dashClip;
	public AudioClip stealthClip;

	public ParticleSystem dash_p;
	public ParticleSystem daggerCharge;

	public Material stealth_mat;
	public Material regular_mat;
	
	public override void Awake()
	{
		if(current == null)
		{
			base.Awake();
			classType = playerClass.ROGUE;
			current = this;
		}
		else
		{
			Destroy(this);
		}
	}

	public override void basicAttack(string dir)
	{
		if(dir == "down")
		{
			attackStarted = Time.time;
			charging = true;
		}
		float currentTime = Time.time;
		float timeSinceAttack = currentTime - attackStarted;
		//When the attack key is released, check to see how long it was held to determine what attack to do.
		if (dir == "up")
		{
			if(timeSinceAttack >= 0.7f / attackSpeed && checkForMana(dashMana))
			{
				//Dash Attack
				dash = true;
				useMana(dashMana);
				specialAttack();
				dash_p.Play();
				anim.SetBool("Dash", true);
			}
			else if(!anim.GetBool("Jump"))
			{
				//Basic Attack
				anim.SetTrigger("Attack");
				attacking = true;
				daggerCharge.Stop();
			}
			charging = false;
		}
	}

	public override void specialAttack()
	{
		ToggleStealth(false);
		StartCoroutine(Dash());
	}

	public IEnumerator Dash()
	{
		GameObject soundObj = new GameObject("roguedash");
		soundObj.transform.position = transform.position;
		AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
		src.clip = dashClip;
		src.Play();
		Destroy(soundObj, src.clip.length);

		yield return new WaitForSeconds(dashDur);

		dash = false;
		anim.SetBool("Dash", false);
		dash_p.Stop ();
		daggerCharge.Stop();
	}

	public override void Movement(float x, float z)
	{
		if(dash)
		{
			float speed = moveSpeed;
			moveSpeed = dashSpeed;
			base.Movement(transform.forward.x + x, transform.forward.z + z);
			moveSpeed = speed;

			Collider[] cols = Physics.OverlapSphere(transform.position + transform.forward + Vector3.up, 1f);
			foreach(Collider c in cols)
			{
				if(c.GetComponent<Explodable>())
					c.SendMessage("Boom");
			}
		}
		else
		{
			base.Movement(x, z);
		}
	}
	
	public override void classAbility(string dir)
	{
		//Make the rogue invisible
		if(dir == "down")
		{
			if(visibility == 1.0f)
			{
				ToggleStealth(true);
			}
			else if (visibility == 0.0f)
			{
				ToggleStealth(false);
			}

			GameObject soundObj = new GameObject("roguestealth");
			soundObj.transform.position = transform.position;
			AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
			src.clip = stealthClip;
			src.Play();
			Destroy(soundObj, src.clip.length);
		}
	}

	public void ToggleStealth(bool turnOn)
	{
		if(turnOn)
		{
			mode = 0;
			visibility = 0.0f;
			anim.SetBool("Stealth", true);
			ChangeClass();
			characterMesh.renderer.materials = new Material[2]{regular_mat, stealth_mat};
		}
		else
		{
			mode = 1;
			visibility = 1.0f;
			anim.SetBool("Stealth", false);
			ChangeClass();
			characterMesh.renderer.materials = new Material[1]{regular_mat};
		}
	}

	protected override void Update()
	{
		base.Update();

		//Increase the rogue's energy if it is not full and he is visible
		if(visibility == 1.0f && mana < 100.0f)
		{
			manaRegen(manaRegenRate);
		}
		//Deplete the rogue's energy if he is invisible
		else if(visibility == 0.0f)
		{
			if(checkForMana(stealthMana * Time.deltaTime))
			{
				useMana(stealthMana * Time.deltaTime);
			}
			else
			{
				//If the rogue runs out of energy, he becomes visible
				ToggleStealth(false);
			}
		}

		if(anim.GetBool("Run") && attacking)
			anim.SetLayerWeight(1, 1f);
		else
			anim.SetLayerWeight(1, 0f);

		if (charging && Time.time - attackStarted > 0.3f && checkForMana(dashMana) && !daggerCharge.isPlaying)
		{
			daggerCharge.Play();
		}
	}

	protected void OnTriggerEnter(Collider c)
	{
		if(c.CompareTag("Enemy"))
		{
			if(dash)
			{
				//if the player cannot go behind the enemy, attempt to go to the right then left
				dash = false;

				//center the enemy's collider
				Vector3 colCenter = c.transform.position;
				colCenter.y = this.transform.position.y;

				//behind facing
				Vector3 colBehind = c.transform.forward;
				colBehind.y = 0.0f;
				//right facing
				Vector3 colRight = c.transform.right;
				colRight.y = 0.0f;
				//left facing
				Vector3 colLeft = -1 * colRight;
				colLeft.y = 0.0f;

				//scales the radius of the enemy's bounding volume
				Vector3 sc = c.transform.localScale;
				float largest = Mathf.Max(sc.x, sc.y, sc.z);
				float colRad = (c.GetComponent<CharacterController>().radius + 0.22f) * largest;
				//scales the radius of the player's bounding volume
				sc = this.transform.localScale;
				largest = Mathf.Max(sc.x, sc.y, sc.z);
				float plRad = cc.radius * largest;
				colRad += plRad;

				//coordinates to move behind the enemy
				Vector3 moveBehind = -1 * colRad * colBehind;
				moveBehind.y = 0;
				//coordinates to move to the left side of the enemy
				Vector3 moveRight = -1 * colRad * colRight;
				moveRight.y = 0;
				//coordinates to move to the right side of the enemy
				Vector3 moveLeft = -1 * colRad * colLeft;
				moveLeft.y = 0;



				//list of collisions for the potential positions of the player
				Collider[][] colList = {Physics.OverlapSphere(colCenter + moveBehind, plRad),
										Physics.OverlapSphere(colCenter + moveRight, plRad),
										Physics.OverlapSphere(colCenter + moveLeft, plRad)};

				bool move = true;
				int i = 0;
				for(i = 0; i < colList.Length; i++)
				{
					move = true;
					for(int j = 0; j < colList[i].Length; j++)
					{
						if(colList[i][j].name.Contains("Wall") || colList[i][j].tag == "Enemy")
						{
							move = false;
							break;
						}
					}
					if(move)
					{
						break;
					}
				}
				if(move)
				{
					if(i == 0)
					{
						this.transform.forward = colBehind;
						this.transform.position = colCenter;
						this.transform.Translate(moveBehind, Space.World);
					}
					else if(i == 1)
					{
						this.transform.forward = colRight;
						this.transform.position = colCenter;
						this.transform.Translate(moveRight, Space.World);
					}
					else if(i == 2)
					{
						this.transform.forward = colLeft;
						this.transform.position = colCenter;
						this.transform.Translate(moveLeft, Space.World);
					}
				}
				anim.SetTrigger("Attack");
			}
		}
		if(c.GetComponent<Explodable>())
		{
			c.SendMessage("Boom");
		}
	}

	public void AttackStart()
	{
		attacking = true;
		Knife.current.damaging = true;

		ToggleStealth(false);

		GameObject soundObj = new GameObject("rogueattack");
		soundObj.transform.position = transform.position;
		AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
		src.clip = attackClip;
		src.Play();
		Destroy(soundObj, src.clip.length);
	}

	public void DamageEnd()
	{
		Knife.current.damaging = false;
	}

	public void AttackEnd()
	{
		Knife.current.damaging = false;
		attacking = false;
		daggerCharge.Stop();
	}
}