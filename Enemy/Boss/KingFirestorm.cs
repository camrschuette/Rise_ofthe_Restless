using UnityEngine;
using System.Collections;

public class KingFirestorm : MonoBehaviour 
{
	private float firestormDamage = 10.0f;
	private float rotationSpeed = 0.15f;

	void Start()
	{
		StartCoroutine(warmup());
	}

	void FixedUpdate()
	{
		Vector3 targetRotation = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + 0.1f, transform.eulerAngles.z);
		transform.eulerAngles = Vector3.RotateTowards(transform.eulerAngles, targetRotation, rotationSpeed, rotationSpeed);
	}

	void OnTriggerStay(Collider c)
	{
		if (c.tag == "Player")
		{
			c.GetComponent<PlayerBase>().takeDamage(firestormDamage);
		}
	}

	private IEnumerator warmup()
	{
		yield return new WaitForSeconds(3.0f);
		GetComponent<MeshCollider>().enabled = true;
		yield return null;
	}
}
