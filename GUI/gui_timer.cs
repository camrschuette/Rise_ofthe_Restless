using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class gui_timer : MonoBehaviour {

	public float timerValue = 30.0f;

	private Text time;
	private bool timerStart = false;
	private GameObject pm;
	
	void Start () {
		//init all of the timer stuff
		time = GetComponent<Text> ();
		int tmp = (int)timerValue;
		time.text = tmp.ToString();

		pm = GameObject.FindGameObjectWithTag("PlayerManager");
	}

	void Update () {
		//update the timer
		if (timerStart) 
		{
			timerValue -= Time.deltaTime;
			int tmp = (int)timerValue;
			time.text = tmp.ToString ();

			/*if (tmp <= 0)
				pm.GetComponent<PlayerManager>();*/
		} 
		else 
		{
			int nPlayers = pm.GetComponent<PlayerManager>().numPlayers;

			if (nPlayers > 0)
				timerStart = true;
		}
	}
}
