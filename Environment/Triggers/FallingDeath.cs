using UnityEngine;
using System.Collections;

public class FallingDeath : MonoBehaviour 
{
	void OnTriggerEnter(Collider c)
	{
		if (c.tag == "Player")
		{
			c.GetComponent<PlayerBase>().kill();
		}
	}
}
