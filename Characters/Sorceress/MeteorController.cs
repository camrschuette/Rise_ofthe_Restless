using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Exploder;

public class MeteorController : MonoBehaviour 
{
	public float rotVel = 45.0f;
	public ParticleSystem outer;
	public ParticleSystem expl;

	private ParticleSystem p;
	private float radius = 3.0f;
	private float damageAmount = 90.0f;

	void OnTriggerEnter(Collider c)
	{
		List<GameObject> objectsHit = new List<GameObject>();
		// Do an overlap sphere to get all enemies this attack will hi
		Collider[] hit = Physics.OverlapSphere(transform.position, radius, LayerMask.GetMask("Enemy"));

		foreach (Collider e in hit)
		{
			if (objectsHit.Contains(e.gameObject))
			{
				continue;
			}
			else
			{
				objectsHit.Add(e.gameObject);
			}
			if (e.tag == "Enemy")
			{
				e.GetComponent<EnemyBase>().takeDamage(damageAmount);
			}
			if (e.GetComponent<Explodable>() != null)
			{
				e.SendMessage("Boom");
			}
		}

		if (c.GetComponent<Explodable> ()) {
			c.GetComponent<Explodable> ().SendMessage("Boom");
		}
		// Set up the destruction of this object
		impact ();

	}

	private void impact(){
		p = transform.parent.gameObject.GetComponentInChildren<ParticleSystem>();
		p.loop = false;
		outer.loop = false;
		expl.Play ();
		transform.GetComponent<Explodable> ().SendMessage ("Boom");
		Destroy (transform.parent.gameObject, 10.0f);
		this.enabled = false;
	}

   
}
