using UnityEngine;
using System.Collections;

public class hawkSphere : MonoBehaviour {
	
	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	void OnTriggerEnter(Collider c)
	{
		if (c.gameObject.CompareTag("Enemy"))
		{
			gameObject.SendMessageUpwards("setEnemy",c.gameObject);
		}
	}	
}
