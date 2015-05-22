using UnityEngine;
using System.Collections;
using Exploder;

public class bombBehavior : MonoBehaviour
{
	public static bombBehavior current;

	public float dmgRadius = 3.5f;
	public float dmg = 37.5f;
	public Transform dropPos;

	public AudioClip explodeClip;

	public void Awake()
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

	public void Start()
	{
		dropPos = GameObject.Find("DropPos").transform;
	}

	public void DropBomb()
	{
		transform.position = dropPos.position;

		foreach(Transform t in transform)
		{
			t.gameObject.SetActive(true);
			foreach(Transform child in t)
			{
				child.gameObject.SetActive(true);
			}
		}
		Invoke("Explosion", 8f);
	}

	public void OnDisable()
	{
		CancelInvoke("Explosion");
	}

	void OnTriggerEnter(Collider c)
	{
		CancelInvoke("Explosion");
		Explosion();
	}

	private void Explosion()
	{
		GameObject soundObj = new GameObject("BOOM!");
		soundObj.transform.position = transform.position;
		AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
		src.clip = explodeClip;
		src.Play();
		Destroy(soundObj, src.clip.length);

		Collider[] hitColliders = Physics.OverlapSphere(transform.position,dmgRadius);
		for(int i=0;i<hitColliders.Length;i++)
		{
			if(hitColliders[i].CompareTag("Enemy"))
			{
				hitColliders[i].GetComponent<EnemyBase>().takeDamage(dmg);
			}
			if(hitColliders[i].CompareTag("Debris"))
			{
				if(hitColliders[i].gameObject.GetComponent<Explodable>())
				{
					hitColliders[i].gameObject.GetComponent<Explodable>().SendMessage("Boom");
				} 
				else 
				{
					Destroy(hitColliders[i].gameObject);
				}
			}
			if(hitColliders[i].gameObject.GetComponent<Explodable>())
			{
				hitColliders[i].gameObject.GetComponent<Explodable>().SendMessage("Boom");
			}
		}
		Woodsman.current.bombActive = false;
		Woodsman.current.ChangeClass ();
		GetComponent<Explodable>().SendMessage("Boom");
		GameObject explo = Instantiate (Resources.Load ("Prefabs/Character/WoodsMan/bomb_explosion"), gameObject.transform.position, gameObject.transform.rotation) as GameObject; 
		Destroy (explo, 2.0f);

	}
}
