using UnityEngine;
using System.Collections;
using Rewired;

public class StartScreenCamera : MonoBehaviour
{
	private bool moving = false;

	private float moved = 0.0f;
	private float angle = 270.0f;
	private Vector3 newForward = Vector3.zero;

	public AudioClip switchClip;

	void FixedUpdate ()
	{
		if(moving)
		{
			transform.Rotate(0, angle * Time.deltaTime, 0);
			moved += Mathf.Abs(angle * Time.deltaTime);
			if(moved >= 90)
			{
				moved = 0.0f;
				transform.forward = newForward;
				moving = false;
			}

		}
	}

	public bool CameraRotate(float horizontal)
	{
		bool canRotate = false;
		if(!moving)
		{
			if (horizontal != 0)
			{
				if(horizontal > 0)
				{
					angle = -Mathf.Abs(angle);
					newForward = -transform.right;
					moving = true;
					canRotate = true;
				}
				else if(horizontal < 0)
				{
					angle = Mathf.Abs(angle);
					newForward = transform.right;
					moving = true;
					canRotate = true;
				}

				GameObject soundObj = new GameObject("switchcharacter");
				soundObj.transform.position = transform.position;
				AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
				src.clip = switchClip;
				src.Play();
				Destroy(soundObj, src.clip.length);
			}
		}
		return canRotate;
	}
}
































