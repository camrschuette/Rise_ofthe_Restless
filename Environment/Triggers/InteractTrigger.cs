using UnityEngine;
using System.Collections;

//fires when the player quota is met
//NOT the same as the AutoTrigger
public class InteractTrigger : TriggerController
{
	public void OnTriggerEnter(Collider other)
	{
		if(CanTrigger(other.gameObject))
		{
			playersIn++;
			if(playersIn >= playersNeeded)
			{
				Trigger();
			}
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
