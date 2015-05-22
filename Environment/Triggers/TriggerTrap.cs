using UnityEngine;
using System.Collections;

public class TriggerTrap : MonoBehaviour 
{
	public float cooldown = 0.0f;		// the cooldown (if any) between triggers setting off the trap
	private float timeTilReady = 0.0f;	// time until cooldown is finished
	public Transform objSpawnLocation;	// the location where the trap will spawn (if the trap obj spawns in a different location from its trigger)
	public GameObject trapObject; 		// not a self reference, but a prefab that the trap instantiates upon activation
										// for example, an arrow from an arrow trap or spikes for a spike trap

	void FixedUpdate()
	{
		if (timeTilReady > 0.0f)
		{
			timeTilReady -= Time.deltaTime;
		}
	}

	// Whenever a player enters the trigger, set off the trap
	void OnTriggerEnter(Collider c)
	{
		if (c.tag == "Player" && timeTilReady <= 0.0f)
		{
			fireTrap();
		}
	}

	private void fireTrap()
	{
		timeTilReady = cooldown;
		if (objSpawnLocation)
		{
			Instantiate(trapObject, objSpawnLocation.position, objSpawnLocation.rotation);
		}
		else
		{
			Instantiate(trapObject, transform.position, transform.rotation);
		}
	}
}
