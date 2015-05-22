using UnityEngine;
using System.Collections;

//fires when the player quota is met
//fires again when it no longer meets the quota
public class AutoTrigger : TriggerController
{
	public void Update()
	{
		if(state && playersIn >= playersNeeded)
		{
			Trigger();
		}
		if(!state && playersIn < playersNeeded)
		{
			Trigger();
		}

	}

	public void OnTriggerEnter(Collider other)
	{
		if(CanTrigger(other.gameObject))
		{
			playersIn++;
		}
	}

	public void OnTriggerExit(Collider other)
	{
		if(CanTrigger(other.gameObject))
		{
			playersIn--;
		}
	}
}
