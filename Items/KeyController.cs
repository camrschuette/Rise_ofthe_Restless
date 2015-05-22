using UnityEngine;
using System.Collections;

public class KeyController : MonoBehaviour 
{
	public bool multiKey = false; // set to true for rooms that have multiple doors that need to be opened by one key
	public AudioClip pickupClip;
		
	void Update() 
	{
		transform.Rotate (new Vector3 (0, 0, 30) * Time.deltaTime);
	}

	void OnTriggerEnter(Collider player)
	{
		if (player.gameObject.CompareTag ("Player")) 
		{
			PlayerManager.current.haveKey = true;
			PlayerManager.current.multiKey = multiKey;

			GameObject soundObj = new GameObject("keypickup");
			soundObj.transform.position = transform.position;
			AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
			src.clip = pickupClip;
			src.Play();
			Destroy(soundObj, src.clip.length);

			Destroy(gameObject);
		}
	}
}
