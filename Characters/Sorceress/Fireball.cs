using UnityEngine;
using System.Collections;
using Exploder;

public class Fireball : MonoBehaviour {

	public float speed = 15.0f;
	private float fireballDamage = 35.0f;
    private float attackMultiplier = 1.0f;

	void Start(){
		Destroy (gameObject, 5.0f);
	}

	void Update () 
	{
		transform.position = transform.position + (transform.forward * speed * Time.deltaTime);
		//transform.RotateAround (transform.position, transform.up, speed * Time.deltaTime);
	}

	void OnTriggerEnter(Collider c){
		if (c.gameObject.CompareTag ("Enemy")) {
            ifCriticalHit(1.5f, 10);
			c.GetComponent<EnemyBase>().takeDamage(fireballDamage * attackMultiplier);
            resetAttackMultiplier();
			gameObject.GetComponent<Explodable>().SendMessage("Boom");
			//Destroy (gameObject);
		}
		if (c.gameObject.CompareTag ("IcePillar")) {
			Destroy (c.gameObject);
			gameObject.GetComponent<Explodable>().SendMessage("Boom");
		}
		if (c.gameObject.CompareTag ("wall")) {
			gameObject.GetComponent<Explodable>().SendMessage("Boom");
		}
		if (c.GetComponent<Explodable>() != null)
		{
			c.SendMessage("Boom");
		}

	}

    void ifCriticalHit(float dmgMultiplier, int randomChance)
    {
        // The higher the int that is passed in randomChance, the lower the crit possibility is
        int i = 0;
        int r = Random.Range(0, randomChance);
        if (i == r)
        {
            attackMultiplier = dmgMultiplier;
        }
    }
    
    void resetAttackMultiplier()
    {
        attackMultiplier = 1.0f;
    }
}
