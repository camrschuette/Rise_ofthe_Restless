using UnityEngine;
using System.Collections;

public class DropGold : MonoBehaviour {
	
	public float multiplyer = 1.0f;
	float numGold;
	float numCoins;
	public float numGoldOveride = -1.0f;
	public float numCoinOveride = -1.0f;
	public int goldDropRate;
	public int potionDropRate;
	public GameObject gld;
	public GameObject Coin;
	public GameObject Ptn;

	void Start(){
		numGold = Random.Range(0.0f,3.0f);
		numCoins = Random.Range(0.0f,10.0f);
	}
	
	public void Reward()
	{
		//Debug.Log("drop");
		numGold = Mathf.Floor(numGold * multiplyer);
		numCoins = Mathf.Floor(numCoins * multiplyer);
		float yPos = transform.position.y + 0.25f;
		int item = Random.Range(0, 100);
		if(item < goldDropRate)
		{
			if(numGoldOveride >= 0)
			{
				numGold = numGoldOveride;
			}
			if(numCoinOveride >= 0)
			{
				numCoins = numCoinOveride;
			}
			for(int g = 0; g < numCoins; g++)
				GameObject.Instantiate(Coin, new Vector3(transform.position.x + (Random.Range(-1.0F,1.0F)*(Mathf.Floor(multiplyer/2))),yPos,transform.position.z - (Random.Range(-1.0F,1.0F)*(Mathf.Floor(multiplyer/2)))), Quaternion.identity);
			for(int g = 0; g < numGold; g++)
				GameObject.Instantiate(gld, new Vector3(transform.position.x + (Random.Range(-1.0F,1.0F)*(multiplyer)),yPos,transform.position.z - (Random.Range(-1.0F,1.0F)*(multiplyer))), Quaternion.identity);
		}
		if(item >= 100 - potionDropRate)
		{
			GameObject.Instantiate(Ptn, new Vector3(transform.position.x + Random.Range(-1.0F,1.0F),yPos,transform.position.z - Random.Range(-1.0F,1.0F)), Quaternion.identity);
		}
		
	}
}
