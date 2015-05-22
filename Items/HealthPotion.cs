using UnityEngine;
using System.Collections;

public class HealthPotion : MonoBehaviour 
{
	private float healValue = 160.0f;

	public void usePotion(PlayerBase p)
	{
		p.health += healValue;
		p.health = Mathf.Clamp(p.health, 0, p.maxHealth);
		Destroy(this);
	}
}
