using UnityEngine;
using System.Collections;

public class Gold : MonoBehaviour {
	
	public int goldValue = 1;
	float start, current;

	public AudioClip pickupClip;
	
	public void Start()
	{
		start = Time.time;
	}
	
	void Update ()
	{
		current = Time.time;
		float delta = current - start;
		/*if (delta > 60)
		{
			Destroy(gameObject);
		}*/
		transform.Rotate (new Vector3 (0, 30, 0) * Time.deltaTime);
	}
	
	void OnTriggerStay(Collider player)
	{
		if (player.gameObject.CompareTag ("Player") && Time.time - start > 0.2f) 
		{	
			player.gameObject.SendMessage ("addScore", goldValue);

			GameObject soundObj = new GameObject("goldpickup");
			soundObj.transform.position = transform.position;
			AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
			src.clip = pickupClip;
			src.Play();
			Destroy(soundObj, src.clip.length);

			Destroy(gameObject);
		}
		
	}
}
