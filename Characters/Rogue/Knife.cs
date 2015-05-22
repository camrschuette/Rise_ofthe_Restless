using UnityEngine;
using System.Collections;
using Exploder;

public class Knife : MonoBehaviour
{
	public static Knife current;

	public bool damaging = false;
	public float damage = 25f;

	public AudioClip hitClip;

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
			if(c.CompareTag("Enemy"))
			{
				Vector3 vec = (Rogue.current.transform.position - c.transform.position).normalized;
				float angle = Vector3.Angle(c.transform.forward, vec);
				float bonus = 1.0f;
				if(angle > 90)
				{
					bonus = 2f;
				}
				//restore more on back attack
				Rogue.current.addMana(1.5f * bonus);
				c.GetComponent<EnemyBase>().takeDamage(damage * bonus * Rogue.current.attackMultiplier);

				GameObject soundObj = new GameObject("rogueslash");
				soundObj.transform.position = transform.position;
				AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
				src.clip = hitClip;
				src.Play();
				Destroy(soundObj, src.clip.length);
			}
			if(c.GetComponent<Explodable>())
			{
				c.SendMessage("Boom");
			}
		}
	}
}
