using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyArchtypeMelee : EnemyBase 
{
	public Transform target;
	private Transform mTransform;
	
	public float chaseRange = 20.0f;
	public float pDistance;
	public PlayerBase player;

	public AudioClip attackClip;
	public AudioClip deathClip;
	public AudioClip hitClip;

	// Behavior / Rates
	private bool chasing = false;

	void Awake() 
	{
		mTransform = transform;
	}

	public override void kill ()
	{
		base.kill();

		GameObject soundObj = new GameObject("zombiedeath");
		soundObj.transform.position = transform.position;
		AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
		src.clip = deathClip;
		src.Play();
		Destroy(soundObj, src.clip.length);
	}

	protected override void FixedUpdate() 
	{
		base.FixedUpdate();
		player = findClosestPlayerInRange(chaseRange);
		if (player)
			checkForSight (player);
		if (player != null && !attacking && playerSighted)
		{
			target = player.transform;
			pDistance = (target.position - mTransform.position).magnitude;
		
			if (chasing) 
			{
				if(pDistance > giveUpThreshold)
				{
					chasing = false;
					GetComponent<Animator>().SetBool("walking", false);
				}

				if(!attacking && pDistance <= attackDistance)
				{
					GetComponent<Animator>().SetTrigger("attack");
					attacking = true;

					GameObject soundObj = new GameObject("zombieattack");
					soundObj.transform.position = transform.position;
					AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
					src.clip = attackClip;
					src.Play();
					Destroy(soundObj, src.clip.length);
				}
				else
				{
					cc.Move(mTransform.forward * moveSpeed * Time.deltaTime * moveMulti);
					rotateTowardsPlayer(player, Time.deltaTime);
				}
			}
			else
			{
				if(pDistance < chaseRange)
				{
					chasing = true;
					GetComponent<Animator>().SetBool("walking", true);
				}
			}
		}
	}

	private void Attack()
	{
		Collider[] hit = Physics.OverlapSphere(transform.position + transform.forward, 1.0f, LayerMask.GetMask("Player"));
		foreach (Collider c in hit)
		{
			c.GetComponent<PlayerBase>().takeDamage(attackDamage, transform);
		}
		if (hit.Length > 0)
		{
			GameObject soundObj = new GameObject("zombiehit");
			soundObj.transform.position = transform.position;
			AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
			src.clip = hitClip;
			src.Play();
			Destroy(soundObj, src.clip.length);
		}
	}

	public void notifyAttackEnd()
	{
		attacking = false;
	}
}