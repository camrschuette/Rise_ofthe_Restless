using UnityEngine;
using System.Collections;

public class SpikeTrap : TrapBase
{
	public bool spawner = false;
	//while the SpikeTrap uses the spawner attribute, it doesn't actually spawn a new SpikeTrap
	//spawner == true means it's retracting
	//spawner == false means it's advancing 
	private Transform start = null;
	private Transform end = null;

	private static GameObject soundObj;

	public AudioClip advanceClip;
	public AudioClip retractClip;
	public AudioClip hitClip;

	public void Start()
	{
		this.transform.up = Vector3.up;
		Transform plate = transform.parent.FindChild("Plate");
		this.start = plate.FindChild("Start");
		this.end = plate.FindChild("End");
	}

	protected void FixedUpdate()
	{
		if(!spawner)
		{
			this.transform.position += this.transform.up * 3.0f * Time.deltaTime;
			if((this.transform.position - this.start.position).magnitude > (this.end.position - this.start.position).magnitude)
			{
				this.transform.position = this.end.position;
			}
		}
		else
		{
			this.transform.position += this.transform.up * -3.0f * Time.deltaTime;
			if((this.transform.position - this.end.position).magnitude > (this.start.position - this.end.position).magnitude)
			{
				this.transform.position = this.start.position;
			}
		}
	}

	protected override void ActivateTrigger(bool state)
	{
		if(state)
		{
			if(spawner)
			{
				this.spawner = false;

				GameObject soundObj = new GameObject("spikeadvance");
				soundObj.transform.position = transform.position;
				AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
				src.clip = advanceClip;
				src.Play();
				Destroy(soundObj, src.clip.length);
			}
		}
		else
		{
			if(!spawner)
			{
				this.spawner = true;

				GameObject soundObj = new GameObject("spikeretract");
				soundObj.transform.position = transform.position;
				AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
				src.clip = retractClip;
				src.Play();
				Destroy(soundObj, src.clip.length);
			}
		}
	}

	protected override void OnTriggerEnter(Collider c)
	{
		this.HitObject(c.transform);
	}

	protected void OnTriggerStay(Collider c)
	{
		this.HitObject(c.transform);
	}

	protected override void HitObject(Transform t)
	{
		if(t.gameObject.tag == "Player")
		{
			if (soundObj == null)
			{
				soundObj = new GameObject("spikehit");
				soundObj.transform.position = transform.position;
				AudioSource src = soundObj.AddComponent<AudioSource>() as AudioSource;
				src.clip = hitClip;
				src.Play();
				Destroy(soundObj, src.clip.length);
			}

			t.GetComponent<PlayerBase>().takeDamage(this.damage);
			this.trapEffect(t.gameObject);
		}
	}
}
