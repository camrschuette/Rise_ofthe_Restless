using UnityEngine;
using System.Collections;

public class MovingPlatform : MonoBehaviour 
{
	public Transform[] destinations;		// an array of transforms set in the inspector, the platform will move along these points
	private int curDest = 0;				// the current destination being traveled to
	public float moveSpeed = 1.0f;

	public float destinationDelay = 0.0f;	// the platform will stop for this many seconds upon reaching its destination before moving to next point
	private float delayTimer = 0.0f;		// current time until end of delay

	public bool freeMove = true;			// if false, the platform must be triggered to move from destination to destination
	private bool canMove = false;			// used only when freeMove is false
	public bool looping = true;				// set to true if you want the platform to loop through its destinations, false if it shouldn't move after reaching the final one
	private bool doneMoving = false;		// used only if not looping, set to true when the platform reaches its final destination

	void FixedUpdate()
	{
		if (freeMove || (!freeMove && canMove))
		{
			if (looping || (!looping && !doneMoving))
			{
				// Move the platform to the next destination if not currently delaying
				if (delayTimer <= 0.0f)
				{
					transform.position = Vector3.MoveTowards(transform.position, destinations[curDest].position, moveSpeed * Time.deltaTime);
				}
				else
				{
					delayTimer -= Time.deltaTime;
				}
			}
		}

		// Check to see if we've reached that destination
		if (transform.position == destinations[curDest].position)
		{
			delayTimer = destinationDelay;
			curDest = (curDest + 1) % destinations.Length;
			if (!looping && curDest == 0)
			{
				doneMoving = true;
			}
			canMove = false; // only affects platforms without freeMove
		}
	}

	// If the platform is not set to freeMove, calling this function will allow the platform to move to its next destination. Should be called by triggers
	public void ActivateTrigger()
	{
		canMove = true;
	}
}
