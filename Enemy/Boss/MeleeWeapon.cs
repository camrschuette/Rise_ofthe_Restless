using UnityEngine;
using System.Collections;
using Exploder;

public class MeleeWeapon : MonoBehaviour
{
	public static MeleeWeapon current;

	public bool damaging = false;
	public float basicDamage = 20f;
	public float homeRunDamage = 80f;

	public float basicForce = 2f;
	public float homeRunForce = 35f;

	public int attackMode = 0;

	public AudioClip homerunClip;

	void Awake()
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
				if(c.CompareTag("Player"))
				{
					float damage = 0f;
					float force = 0f;
					if(attackMode == 1)
					{
						damage = basicDamage;
						force = basicForce;
					}
					else if(attackMode == 2)
					{
						damage = homeRunDamage;
						force = homeRunForce;

						GameObject soundObj = new GameObject("kinghomerun");
						soundObj.transform.position = transform.position;
						AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
						src.clip = homerunClip;
						src.Play();
						Destroy(soundObj, src.clip.length);
					}
					c.GetComponent<PlayerBase>().takeDamage(damage);
					Vector3 forceDir = (c.transform.position - transform.position);
					forceDir.y = 0f;
					c.GetComponent<PlayerBase>().addForce(forceDir.normalized * force);
				}
				if(c.GetComponent<Explodable>() != null)
					c.SendMessage("Boom");
			}
		}
	}
}
