using UnityEngine;
using System.Collections;
using Exploder;

public class KingCollapseBoulder : MonoBehaviour 
{
	private float boulderDamage = 20.0f;
	private float fallSpeed = 3.5f;

	void Start()
	{
		Destroy(gameObject, 8.0f);	// delete the boulder in case it misses the stage
	}

	void OnTriggerEnter(Collider c)
	{
		if (c.tag == "Player")
		{
			c.GetComponent<PlayerBase>().takeDamage(boulderDamage);
		}
		if (c.tag == "Player" || c.gameObject.layer == LayerMask.GetMask("Default"))
		{
			GetComponent<Explodable>().SendMessage("Boom");
		}
	}
}
