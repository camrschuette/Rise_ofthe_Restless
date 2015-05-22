using UnityEngine;
using System.Collections;

public class LightFlicker : MonoBehaviour {
	
	private float minIntensity = 0.25f;
	private float maxIntensity = 0.75f;
	private float speed = 1.2f;
	private float random;
	void Start()
	{
		random = Random.Range(0.0f, 65535.0f);
	}
		
	void Update()
	{

		float noise = Mathf.PerlinNoise (random, Time.time * speed);
		light.intensity = Mathf.Lerp (minIntensity, maxIntensity, noise);
	}
}
