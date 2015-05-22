using UnityEngine;
using System.Collections;

public class ArrowShooter : MonoBehaviour {

	private float time;
	private bool isFiring = false;

	void Start(){
		time = Random.Range (3.0f, 5.0f);
	}	
	
	void Update()
	{
		if(!isFiring)
		{
			StartCoroutine(Arrow());
		}
	}
	
	IEnumerator Arrow()
	{
		isFiring = true;
		yield return StartCoroutine(Wait());

		GameObject a = Instantiate (Resources.Load ("Prefabs/Environment/Traps/Arrow"), transform.position, transform.rotation) as GameObject;
		a.transform.parent = gameObject.transform;

		isFiring = false;
	}
	
	IEnumerator Wait()
	{
		yield return new WaitForSeconds (time);
	}

}
