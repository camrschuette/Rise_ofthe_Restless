using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TriggerController : MonoBehaviour
{
	//list of objects to affect
	public List<GameObject> obj;
	//whether the trigger can be used
	public bool on = true;
	//whether the trigger can be used multiple times
	public bool multi = false;
	//two positions for the trigger to allow two different actions
	public bool state = true;
	//# of players that need to be within the trigger's collider
	protected int playersIn = 0;
	//# of players (or objects) to activate the trigger
	public int playersNeeded = 1;
	//# of seconds before the trigger can be used again
	public float coolDownMax = 1.5f;
	//whether the trigger is in cool down
	public bool inCD = false;

	public void Start()
	{
//		foreach(TriggerObject t in trobj)
//		{
//			t.funcs = Action<bool>[trobj.Length];
//		}
		if(inCD)
		{
			StartCoroutine(CoolDownWait());
		}
	}

	public bool CanTrigger(GameObject go)
	{
		bool ok = false;

		//follow this format
		if(go.CompareTag("Player") || go.CompareTag("Hawk"))
		{
			if(gameObject.CompareTag("DoorTrigger"))
			{
				ok = true;
			}
			else
			{
				ok = true;
			}
		}

		return ok;
	}

	public void Trigger()
	{
		if(on && !inCD)
		{
			//Debug.Log("triggering...");
			for(int i = 0; i < obj.Count; i++)
			{
				//"ActivateTrigger" can be changed and is simply the
				//name of the function that will be called on obj[i]
				obj[i].SendMessage("ActivateTrigger", state, SendMessageOptions.DontRequireReceiver);
			}
			state = !state;
			if(!multi)
			{
				on = false;
			}
			inCD = true;
			StartCoroutine(CoolDownWait());
		}
	}

	public IEnumerator CoolDownWait()
	{
		yield return new WaitForSeconds(coolDownMax);
		inCD = false;
		//this is used with interact and auto triggers
		//makes the trigger "undo" what it previously did when it was triggered
		//NOT the same as the TimedTrigger
		if(tag == "DurationTrigger" && !state)
		{
			Trigger();
		}
	}

	public void ActivateTrigger(bool state)
	{
		this.on = true;
	}
}
