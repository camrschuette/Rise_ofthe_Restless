using UnityEngine;
using System.Collections;

public class CheckpointCollider : MonoBehaviour {

	void OnTriggerEnter(Collider other)
	{
		other.gameObject.transform.position = new Vector3 (0.0f, 0.5f, 0.0f);
	}
}
