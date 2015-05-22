using UnityEngine;
using System.Collections;

public class ProjectileTrapObj : TrapBase
{
	public float lifetime = 5.0f;	// how long this trap object has to live
	public float travelSpeed = 5.0f;	// speed in units per second that this projectile travels

	protected void OnEnable()
	{
		StartCoroutine(WaitToDisable());
	}

	protected void FixedUpdate()
	{
		this.transform.position += this.transform.forward * this.travelSpeed * Time.deltaTime;
	}

	protected IEnumerator WaitToDisable()
	{
		yield return new WaitForSeconds(this.lifetime);
		this.gameObject.SetActive(false);
	}

	protected override void OnTriggerEnter(Collider c)
	{
		this.HitObject(c.transform);
	}

	protected override void HitObject(Transform t)
	{
		if(t.gameObject.tag == "Player")
		{
			t.GetComponent<PlayerBase>().takeDamage(this.damage, transform);
			this.trapEffect(t.gameObject);
			this.gameObject.SetActive(false);
		}
		else if(t.name.Contains("Wall"))
		{
			this.gameObject.SetActive(false);
		}
	}
}
