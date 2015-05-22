using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Exploder;

public class Warrior : PlayerBase
{
	public static Warrior current = null;

	private bool blockProjectiles = false;
	private int count = 0;
	private int attackType = 1;
	private float comboStarted;
	private bool canAttack = true;
	private bool charging = false;

	private float specialChargeTime = 0.7f;

	private float whirlwindMana = 35.0f;

	public AudioClip swipeClip;
	public AudioClip whirlwindClip;
	public AudioClip blockClip;

	public ParticleSystem shield_charge;
	public ParticleSystem sword_charge;

	public override void Awake()
	{
		if(current == null)
		{
			base.Awake();
			classType = playerClass.WARRIOR;
			comboStarted = Time.time - 10.0f;
			current = this;
		}
		else
		{
			Destroy(this);
		}
	}

	protected override void Update()
	{
		base.Update();

		if(anim.GetBool("Run") && blockProjectiles)
			anim.SetLayerWeight(1, 1f);
		else if(!anim.GetBool("Run") || !blockProjectiles)
			anim.SetLayerWeight(1, 0f);

		if(anim.GetBool("Run") && attacking)
			anim.SetLayerWeight(2, 1f);
		else if(!anim.GetBool("Run") || !attacking)
			anim.SetLayerWeight(2, 0f);

		if (charging && Time.time - attackStarted > 0.3f && checkForMana(whirlwindMana) && !sword_charge.isPlaying)
		{
			sword_charge.Play();
		}
	}

	public override void basicAttack(string dir)
	{
		if(!anim.GetBool("Jump"))
		{
			if (dir == "down")
			{
				attackStarted = Time.time;
				if (canAttack)
				{
					charging = true;
				}
			}
			float currentTime = Time.time;
			float timeSinceAttack = currentTime - attackStarted;
			float timeSinceCombo = currentTime - comboStarted;
			if (dir == "up" && canAttack) {
				//When the attack key is released, check to see how long it was held to determine what attack to do.
				if (timeSinceAttack > specialChargeTime / attackSpeed && checkForMana(whirlwindMana)){
					// special attack
					anim.SetTrigger("Whirlwind");
				}
				else {
					// continue the combo
					anim.SetTrigger("Attack");
					attacking = true;
					sword_charge.Stop();
				}
				charging = false;
			}
		}
		else if(dir == "up")
		{
			sword_charge.Stop();
			attackStarted = Mathf.Infinity;
		}
	}

	public override void classAbility(string dir)
	{
		if(!anim.GetBool("Jump"))
		{
			if (dir == "down")
			{
				anim.SetBool("Shield", true);
				blockProjectiles = true;
				ChangeClass();
			} 
			else if (dir == "up") 
			{
				anim.SetBool("Shield", false);
				ChangeClass();
			}
		}
	}

	public override void takeDamage(float amount, Transform enemy=null)
	{
		bool damage = true;
		if(enemy != null && blockProjectiles)
		{
			Vector3 vec = (enemy.position - transform.position).normalized;
			float angle = Vector3.Angle(transform.forward, vec);
			if(angle < 90)
			{
				damage = false;

				GameObject soundObj = new GameObject("warriorblock");
				soundObj.transform.position = transform.position;
				AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
				src.clip = blockClip;
				src.Play();
				Destroy(soundObj, src.clip.length);
			}
		}

		if(damage)
			base.takeDamage(amount);
	}

	public override void Jump (bool jumpDown)
	{
		if(!blockProjectiles)
		{
			base.Jump(jumpDown);
		}
		else
		{
			base.Jump(false);
		}
	}

	//called at the beginning of the Shield_Start animation
	public void ShieldStart()
	{
		moveMulti = 0.5f;
		blockProjectiles = true;
		AttackEnd();
		shield_charge.Play ();
	}

	//called at the end of the Shield_End animation
	public void ShieldEnd()
	{
		moveMulti = 1.0f;
		blockProjectiles = false;
		shield_charge.Stop ();
	}

	public void AttackStart(int attackMode)
	{
		if(attackMode < 4)
		{
			GameObject soundObj = new GameObject("warriorswipe");
			soundObj.transform.position = transform.position;
			AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
			src.clip = swipeClip;
			src.Play();
			Destroy(soundObj, src.clip.length);
		}
		else if(attackMode == 4)
		{
			useMana(whirlwindMana);
			GameObject soundObj = new GameObject("warriorwhirlwind");
			soundObj.transform.position = transform.position;
			AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
			src.clip = whirlwindClip;
			src.Play();
			Destroy(soundObj, src.clip.length);
		}

		Sword.current.damaging = true;
		attacking = true;
		canAttack = false;
		Sword.current.attackMode = attackMode;
		ShieldEnd();
	}

	public void AttackEnd()
	{
		Sword.current.damaging = false;
		attacking = false;
		canAttack = true;
		Sword.current.attackMode = 0;
		sword_charge.Stop ();
	}

	public void ComboStart()
	{
		canAttack = true;
	}

	public void DamageEnd()
	{
		Sword.current.damaging = false;
	}
}