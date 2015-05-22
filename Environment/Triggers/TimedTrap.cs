using UnityEngine;
using System.Collections;

public class TimedTrap : MonoBehaviour 
{
	public float timeInterval = 3.0f;	// time in seconds between the firing of traps
	private float timer;				// time until next firing of trap
	public GameObject trapObject; 		// not a self reference, but a prefab that the trap instantiates upon activation
										// for example, an arrow from an arrow trap or spikes for a spike trap

	void Start()
	{
		timer = timeInterval;
	}

	void Update()
	{
		timer -= Time.deltaTime;
		if (timer <= 0.0f)
		{
			fireTrap();
		}
	}

	private void fireTrap()
	{
		Instantiate(trapObject, transform.position, Quaternion.LookRotation(transform.forward));
		timer += timeInterval;
	}
}
