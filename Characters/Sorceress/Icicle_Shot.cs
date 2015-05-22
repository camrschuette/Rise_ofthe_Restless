using UnityEngine;
using System.Collections;
using Exploder;

public class Icicle_Shot : MonoBehaviour 
{

	public float velocity = 45.0f;
	public float rotVel = 45.0f;
	private float iceDamage = 25.0f;
    private float attackMultiplier = 1.0f;

	public AudioClip shatterClip;

	void Start(){
		Destroy (gameObject, 5.0f);
	}

	void Update () {
		transform.position += (transform.up * velocity * Time.deltaTime);
	}


	void OnTriggerEnter(Collider c){
		if (c.gameObject.CompareTag ("Enemy")) {
			EnemyBase eBase = c.GetComponent<EnemyBase>();
            ifCriticalHit(1.5f, 10);
			eBase.takeDamage(iceDamage * attackMultiplier);
            resetAttackMultiplier();
			eBase.slow();
			gameObject.GetComponent<Explodable>().SendMessage("Boom");

			GameObject soundObj = new GameObject("icebreak");
			soundObj.transform.position = transform.position;
			AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
			src.clip = shatterClip;
			src.Play();
			Destroy(soundObj, src.clip.length);
		} 
		if (c.gameObject.CompareTag ("wall")) {
			gameObject.GetComponent<Explodable>().SendMessage("Boom");

			GameObject soundObj = new GameObject("icebreak");
			soundObj.transform.position = transform.position;
			AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
			src.clip = shatterClip;
			src.Play();
			Destroy(soundObj, src.clip.length);
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
