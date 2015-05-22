using UnityEngine;
using System.Collections;

public class PlayerPotion : MonoBehaviour
{
	public void usePotion(PlayerBase player)
	{
		switch(player.item)
		{
		case PotionType.ATTACK:
			StartCoroutine(Attack(player, 2f, 10f));
			break;
//		case PotionType.DEFENSE:
//			break;
		case PotionType.HASTE:
			StartCoroutine(Haste(player, 1.5f, 10f));
			break;
		case PotionType.HEALTH:
			Heal(player, 60f);
			break;
		case PotionType.NONE:
			break;
		}
		player.item = PotionType.NONE;
	}

	private void Heal(PlayerBase p, float healValue)
	{
		p.health += healValue;
		p.health = Mathf.Clamp(p.health, 0, p.maxHealth);
	}

	private IEnumerator Haste(PlayerBase p, float speedIncrease, float effectDuration)
	{
		p.GetComponent<Animator>().speed = speedIncrease;
		p.attackSpeed = speedIncrease;
		yield return new WaitForSeconds(effectDuration);
		p.GetComponent<Animator>().speed = 1.0f;
		p.attackSpeed = 1.0f;
		yield return null;
	}

	private IEnumerator Attack(PlayerBase p, float attackIncrease, float effectDuration)
	{
		p.attackMultiplier= attackIncrease;
		yield return new WaitForSeconds(effectDuration);
		p.attackMultiplier = 1.0f;
		yield return null;
	}
}
