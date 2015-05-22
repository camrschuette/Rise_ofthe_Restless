using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour 
{
	private bool openDoor;
	private float movedSoFar;

	public AudioClip openClip;

	public void ActivateTrigger(bool state)
	{
		if (this.CompareTag("door"))
		{
			PlayerManager pm = PlayerManager.current;
			if (pm.haveKey)
			{
				pm.haveKey = false;
				openDoor = true;
				playOpenSound();
			}
			else if (pm.multiKey)
			{
				pm.multiKey = false;
				openDoor = true;
				playOpenSound();
			}
		}
		else
		{
			Destroy(this.gameObject);
		}
	}

	public void Start() 
	{
		openDoor = false;
		movedSoFar = 0.0f;
	}

	public void Update() 
	{
		if (openDoor) 
		{
			float angleToMove = Mathf.LerpAngle(movedSoFar, -90.0f, 0.2f);
			transform.RotateAround(transform.position, transform.up, angleToMove - movedSoFar);
			movedSoFar = angleToMove;
			if (movedSoFar >= 89.0f) {
				openDoor = false;
			}
		}

	}

	private void playOpenSound()
	{
		GameObject soundObj = new GameObject("dooropen");
		soundObj.transform.position = transform.position;
		AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
		src.clip = openClip;
		src.Play();
		Destroy(soundObj, src.clip.length);
	}

}
