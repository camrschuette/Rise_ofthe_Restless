using UnityEngine;
using System.Collections;
using Exploder;

public class Sword : MonoBehaviour
{
	public static Sword current;

	public bool damaging = false;

	public int attackMode = 0;

	public float firstAttack = 25f;
	public float secondAttack = 50f;
	public float thirdAttack = 75f;
	public float whirlwindAttack = 90f;
	
	public float thirdForce = 10f;
	public float whirlwindForce = 20f;

	public AudioClip hitClip;

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

	void OnTriggerEnter(Collider c)
	{
		if(damaging)
		{
			if(attackMode > 0)
			{
				if(c.CompareTag("Enemy"))
				{
					float damage = 0f;
					float force = 0f;
					if(attackMode == 1)
					{
						damage = firstAttack;
						Warrior.current.addMana(3f);
					}
					else if(attackMode == 2)
					{
						damage = secondAttack;
						Warrior.current.addMana(4f);
					}
					else if(attackMode == 3)
					{
						damage = thirdAttack;
						force = thirdForce;
						Warrior.current.addMana(5f);
					}
					else if(attackMode == 4)
					{
						damage = whirlwindAttack;
						force = whirlwindForce;
					}

					c.GetComponent<EnemyBase>().takeDamage(damage * Warrior.current.attackMultiplier);

					Vector3 forceDir = (c.transform.position - transform.position);
					forceDir.y = 0f;
					c.GetComponent<EnemyBase>().addForce(forceDir.normalized * force);

					GameObject soundObj = new GameObject("warriorslash");
					soundObj.transform.position = transform.position;
					AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
					src.clip = hitClip;
					src.Play();
					Destroy(soundObj, src.clip.length);
				}
				if(c.GetComponent<Explodable>() != null)
					c.SendMessage("Boom");
			}
		}
	}
}
