using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KingShockwave : MonoBehaviour 
{
	private float coneAngle = 35.0f;
	private float ttl = 2.5f;
	private float currentLifetime = 0.0f;
	private float travelSpeed = 8.5f;
	private float currentDistance = 0.0f;
	private float hitForce = 4.0f;
	private float hitDamage = 25.0f;
	private List<GameObject> playersHit;

	void Start()
	{
		playersHit = new List<GameObject>();
		GameObject shock = Instantiate (Resources.Load ("Prefabs/Enemies/Bosses/shockwave_part"), transform.position, transform.rotation) as GameObject;
	}

	void FixedUpdate()
	{
		// Destroy if its lifetime is over
		currentLifetime += Time.deltaTime;
		if (currentLifetime >= ttl)
		{
			Destroy(gameObject);
		}
		// Advance the shockwave
		currentDistance += travelSpeed * Time.deltaTime;
		// Raycast in the pattern of a cone to hit players
		for (int i = 0; i < 10; i++)
		{
			Quaternion startAngle = Quaternion.AngleAxis (-coneAngle/2.0f, Vector3.up);
			Quaternion stepAngle = Quaternion.AngleAxis (coneAngle/15.0f, Vector3.up);
			Quaternion angle = transform.rotation * startAngle;
			Vector3 direction = (angle * Vector3.forward);
			direction.Normalize();
			Vector3 pos = transform.position;

			RaycastHit hit;
			for (int j = 0; j < 15; j++)
			{
				//Debug.DrawRay (pos + new Vector3(0,0.5f,0), direction * currentDistance, Color.green, 10.0f);
				if (Physics.Raycast(pos + new Vector3(0,0.5f,0), direction, out hit, currentDistance, LayerMask.GetMask("Player")))
				{
					hit.collider.GetComponent<PlayerBase>().addForce(direction * hitForce * Time.deltaTime);
					if (!playersHit.Contains(hit.collider.gameObject))
					{
						hit.collider.GetComponent<PlayerBase>().takeDamage(hitDamage, transform);
						playersHit.Add(hit.collider.gameObject);
					}
				}
				direction = stepAngle * direction;
			}
		}
	}
}
