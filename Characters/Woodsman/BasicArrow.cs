using UnityEngine;
using System.Collections;
using Exploder;

public class BasicArrow : ProjectileTrapObj
{
	public bool basic = true;
    private float attackMultiplier = 1.0f;

	public AudioClip hitClip;

	private void Awake()
	{
	}
	
	protected override void HitObject (Transform t)
	{
		if(t.collider.GetComponent<Explodable>() != null)
		{
			t.collider.SendMessage("Boom");
		}
		if (t.gameObject.CompareTag("Enemy"))
		{
			EnemyBase scr = t.gameObject.GetComponent<EnemyBase>();
			float bonus = 1.0f;
			if(!basic)
			{
				bonus = 1.2f;
			}
            ifCriticalHit(1.5f, 10);
			scr.takeDamage(damage * bonus * attackMultiplier);
			scr.damageTaken += damage * bonus * attackMultiplier;
            resetAttackMultiplier();
			if (HawkAI2.current.enemiesToAttack.Contains(t.gameObject) == false)
			{
				HawkAI2.current.enemiesToAttack.Add (t.gameObject);
			}
			Woodsman.current.addMana(5f);

			GameObject soundObj = new GameObject("arrowhit");
			soundObj.transform.position = transform.position;
			AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
			src.clip = hitClip;
			src.Play();
			Destroy(soundObj, src.clip.length);
		}
		else if(t.gameObject.CompareTag("wall"))
		{
			gameObject.SetActive(false);
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
