using UnityEngine;
using System.Collections;

public class rotation : MonoBehaviour {

	public float speed = 1.0f;

	void Update(){
		transform.Rotate (transform.up * Time.deltaTime * speed);
	}
}
