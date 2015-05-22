using UnityEngine;
using System.Collections;

public class Coin : MonoBehaviour {
	
	public int goldValue = 1;
	float start, current;

	public void Start()
	{
		start = Time.time;
	}
	
	void Update ()
	{
		current = Time.time;
		float delta = current - start;
		if (delta > 60)
		{
			Destroy(gameObject);
		}
		transform.Rotate (new Vector3 (0, 30, 0) * Time.deltaTime);
	}
	
	void OnTriggerEnter(Collider player)
	{
		if (player.gameObject.CompareTag ("Player")) 
		{
			player.gameObject.SendMessage ("addScore", goldValue);
			Destroy(gameObject);
		}
		
	}
}
