using UnityEngine;
using System.Collections;

public class EnemyArchtypeRanged : EnemyBase {
	/* The Ranged AI Class Follows the following rule Set
	 * 1. Get in Range to call Fire() Based on Distance to the player
	 * 2. If Enemy is in Your "Radius" Then you Move Away.
	 * 3. Otherwise Fire() until Dead
	 */	
	public Transform target;
	private Transform mTransform;

	public float eRange = 10f;
	public float pDistance;
	public PlayerBase player;
	public Transform shootPos;

	// Behavior / Rates
	private bool chasing = false;
	private float attackTime = 0.0f;
	public float aR;
	
	// Use this for initialization
	void Awake() 
	{
		mTransform = transform;
		shootPos = transform.Find ("shootPos");
		aR = 2f;
	}
	// Update is called once per frame
	protected override void FixedUpdate() 
	{
		base.FixedUpdate();
		player = findClosestPlayerInRange(eRange);
		target = player.transform;
		pDistance = (target.position - mTransform.position).magnitude;

		if (chasing) 
		{
			//Debug.Log("Should be Chasing");
			if(pDistance > giveUpThreshold)
			{
				chasing = false;
			}

			else if(pDistance <= eRange && pDistance >= attackDistance)
			{
				//Debug.Log ("Should be Attacking");
				cc.Move(mTransform.forward * moveSpeed * Time.deltaTime * moveMulti);
				rotateTowardsPlayer(player, Time.deltaTime);
				attackTime = Time.deltaTime + attackTime;
				if(attackTime >= aR)
				{
					//Debug.Log ("Firing Arrows!");
					//Attack(aR);
					attackTime = 0.0f;
				}
				moveMulti = 0.2f;
			}

			else if(pDistance <= attackDistance)
			{
				// rotate 180 degrees and go to 1/2 the distance of the attack 'sphere'
				//mTransform.position += mTransform.forward*-1 * moveSpeed * Time.deltaTime;
				mTransform.rotation = Quaternion.Slerp(mTransform.rotation, target.rotation, Time.deltaTime * rotationSpeed);
				cc.Move(mTransform.forward * moveSpeed * Time.deltaTime * moveMulti);
				moveMulti = 3.0f;
				//Debug.Log("Should be running");
			}

		}
		else
		{
			if(pDistance < eRange)
			{
				chasing = true;
			}
		}
	}

	public void Attack ()
	{
		GameObject bullet = Instantiate(Resources.Load ("Prefabs/Enemies/RestlessBullet"),shootPos.position,Quaternion.identity) as GameObject;
		bullet.transform.up = transform.forward;
	}
}