using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum PotionType
{
	NONE,
	HEALTH,
	HASTE,
	ATTACK,
	//DEFENSE
}

public class PotionBase : MonoBehaviour 
{	
	public PotionType type;	// set in the inspector to assign potion type

	public void Start()
	{
	}

	void Update()
	{
		transform.Rotate (new Vector3 (0, 30, 0) * Time.deltaTime);
	}

	void OnTriggerEnter(Collider player)
	{
		if (player.gameObject.CompareTag("Player")) 
		{
			if (player.GetComponent<PlayerBase>().item != PotionType.NONE)
			{
				PotionType oldItem = player.GetComponent<PlayerBase>().item;
				player.GetComponent<PlayerBase>().addItem(type);
				player.GetComponent<PlayerBase>().itemAbility();
				player.GetComponent<PlayerBase>().addItem(oldItem);
			} 
			else 
			{
				player.GetComponent<PlayerBase>().addItem(type);
			}
			Destroy(gameObject);
		}
	}
}
