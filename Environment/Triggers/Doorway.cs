using UnityEngine;
using System.Collections;

public class Doorway : MonoBehaviour 
{
	public RoomNode sideA;	// points away from the forward direction of the doorway trigger
	public RoomNode sideB;	// points towards the forward direction of the doorway trigger

	void OnTriggerExit(Collider c)
	{
		// When a player walks through the doorway, determine which side they are leaving from and notify the player accordingly
		if (c.gameObject.tag == "Player")
		{
			Vector3 toPlayer = (c.transform.position - transform.position).normalized;

			// If player is leaving towards side B
			if (Vector3.Dot(toPlayer, transform.forward) > 0)
			{
				c.GetComponent<PlayerBase>().enterRoom(sideB);
			}
			else // side A
			{
				c.GetComponent<PlayerBase>().enterRoom(sideA);
			}
		}
	}
}
