using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class coinCountEnd : MonoBehaviour {

	private float max_coins;

	//num coins
	public float player1_coins = 0.0f;
	public float player2_coins = 0.0f;
	public float player3_coins = 0.0f;
	public float player4_coins = 0.0f;

	//ui elements
	public Image p1;
	public Image p2;
	public Image p3;
	public Image p4;

	//coin text
	public Text text_p1;
	public Text text_p2; 
	public Text text_p3; 
	public Text text_p4;

	//profile pics
	public Image profile_p1;
	public Image profile_p2;
	public Image profile_p3;
	public Image profile_p4;

	public int num_players;

	private bool loading_pics = false;
	private bool loading_coins = false;

	private bool has_coins_1 = false;
	private bool has_coins_2 = false;
	private bool has_coins_3 = false;
	private bool has_coins_4 = false;

	void Start(){
		StartCoroutine (restart ());
	}

	void Update () {
		if (num_players > 0 && loading_pics && loading_coins) {
			if (num_players >= 1){

				p1.fillAmount = Mathf.Lerp (p1.fillAmount, player1_coins / max_coins, Time.deltaTime);
				text_p1.text = System.Math.Ceiling (Mathf.Lerp (float.Parse (text_p1.text), player1_coins, Time.deltaTime)).ToString ();
			
			} if (num_players >=2) {

				p2.fillAmount = Mathf.Lerp (p2.fillAmount, player2_coins / max_coins, Time.deltaTime);
				text_p2.text = System.Math.Ceiling (Mathf.Lerp (float.Parse (text_p2.text), player2_coins, Time.deltaTime)).ToString ();
			
			} if (num_players >= 3) {

				p3.fillAmount = Mathf.Lerp (p3.fillAmount, player3_coins / max_coins, Time.deltaTime);
				text_p3.text = System.Math.Ceiling (Mathf.Lerp (float.Parse (text_p3.text), player3_coins, Time.deltaTime)).ToString ();
			
			} if (num_players >= 4) {

				p4.fillAmount = Mathf.Lerp (p4.fillAmount, player4_coins / max_coins, Time.deltaTime);
				text_p4.text = System.Math.Ceiling (Mathf.Lerp (float.Parse (text_p4.text), player4_coins, Time.deltaTime)).ToString ();
			
			}
		}
	}

	public void load_scores(List<int> L){
		foreach (int x in L){
			if (player1_coins == 0.0f && !has_coins_1){
				player1_coins = (float)x;
				has_coins_1 = true;
				text_p1.text = "0";
				continue;
			}
			
			if (player2_coins == 0.0f && !has_coins_2){
				player2_coins = (float)x;
				has_coins_2 = true;
				text_p2.text = "0";
				continue;
			}
			
			if (player3_coins == 0.0f && !has_coins_3){
				player3_coins = (float)x;
				has_coins_3 = true;
				text_p3.text = "0";
				continue;
			}
			
			if (player4_coins == 0.0f && !has_coins_4){
				player4_coins = (float)x;
				has_coins_4 = true;
				text_p4.text = "0";
				continue;
			}
		}

		max_coins = Mathf.Max (player1_coins, player2_coins, player3_coins, player4_coins);
		loading_coins = true;
	}

	public void load_pics(List<Sprite> L){
		foreach (Sprite x in L){
			if (!profile_p1.sprite){
				profile_p1.sprite = x;
				profile_p1.color = new Color(255.0f, 255.0f, 255.0f, 255.0f);
				continue;
			}
			
			if (!profile_p2.sprite){
				profile_p2.sprite = x;
				profile_p2.color = new Color(255.0f, 255.0f, 255.0f, 255.0f);
				continue;
			}
			
			if (!profile_p3.sprite){
				profile_p3.sprite = x;
				profile_p3.color = new Color(255.0f, 255.0f, 255.0f, 255.0f);
				continue;
			}
			
			if (!profile_p4.sprite){
				profile_p4.sprite = x;
				profile_p4.color = new Color(255.0f, 255.0f, 255.0f, 255.0f);
				continue;
			}
		}
		loading_pics = true;
	}

	private IEnumerator restart(){
		yield return new WaitForSeconds (7.0f);

		foreach(PlayerController pc in PlayerManager.current.playerControllers)
		{
			pc.joinState = PlayerController.JoinState.INACTIVE;
		}
		Application.LoadLevel("StartScreen");
	}
}
