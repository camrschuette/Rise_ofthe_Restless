using UnityEngine;
using System.Collections;

//continously fires
public class TimedTrigger : TriggerController
{
	void Update()
	{
		if(!inCD)
		{
			Trigger();
		}
	}
}
