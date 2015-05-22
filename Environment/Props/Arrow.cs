using UnityEngine;
using System.Collections;

public class Arrow : MonoBehaviour {

	public float velocity = 20.0f;
	public float rotVel = 45.0f;

	void Start(){
		transform.Rotate (90, 0, 0, Space.Self);
	}
	
	void Update () {
		//transform.Rotate (0, rotVel * Time.deltaTime, 0, Space.Self);
		transform.position = transform.position + (transform.up * velocity * Time.deltaTime);
		Destroy (gameObject, 3.0f);
	}
	
	/*void onCollisionEnter(Collision c){
		if (c.gameObject.CompareTag ("Player")) 
		{
			c.gameObject.SendMessage("TakeDamage", 0.1f);
		}
	}*/
}
