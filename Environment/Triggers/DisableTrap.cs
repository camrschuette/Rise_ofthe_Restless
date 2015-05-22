using UnityEngine;
using System.Collections;

public class DisableTrap : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	protected void ActivateTrigger(bool state)
	{
		this.gameObject.SetActive (false);
	}
}
