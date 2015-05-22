using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class CharacterStats : MonoBehaviour
{
	public Image healthBar = null;
	public Image manaBar = null;
	private static Sprite[] manaImages = null;
	public Image potion = null;
	private static List<Sprite> potionImages = null;
	public Text scoreText = null;
	public Image profile = null;
	private static Sprite[] profileImages = null;
	public Image class_abil;
	public Sprite[] class_pics = null;

	public void Awake()
	{
		if(manaImages == null)
		{
			manaImages = new Sprite[4];
			
			manaImages[(int)playerClass.WOODSMAN] = Resources.Load<Sprite>("Textures/GUI/gui_energy_green");
			manaImages[(int)playerClass.SORCERER] = Resources.Load<Sprite>("Textures/GUI/gui_energy_blue");
			manaImages[(int)playerClass.ROGUE] = Resources.Load<Sprite>("Textures/GUI/gui_energy_yellow");
			manaImages[(int)playerClass.WARRIOR] = Resources.Load<Sprite>("Textures/GUI/gui_energy_orange");
		}
		if(potionImages == null)
		{
			potionImages = new List<Sprite>();
			foreach(String pt in Enum.GetNames(typeof(PotionType)))
			{
				potionImages.Add(Resources.Load<Sprite>("Textures/GUI/Potions/gui_potion_" + pt.ToString().ToLower()));
			}
		}
		if(profileImages == null)
		{
			profileImages = new Sprite[4];

			profileImages[(int)playerClass.WOODSMAN] = Resources.Load<Sprite>("Textures/GUI/archer_profile");
			profileImages[(int)playerClass.SORCERER] = Resources.Load<Sprite>("Textures/GUI/sorc_profile");
			profileImages[(int)playerClass.ROGUE] = Resources.Load<Sprite>("Textures/GUI/rogue_profile");
			profileImages[(int)playerClass.WARRIOR] = Resources.Load<Sprite>("Textures/GUI/warrior_profile");
		}
		if (class_pics.Length == 0) 
		{
			class_pics = new Sprite[8];
			class_pics[(int)playerClass.WOODSMAN] = Resources.Load<Sprite>("Textures/GUI/gui_bomb");
			class_pics[(int)playerClass.WOODSMAN+4] = Resources.Load<Sprite>("Textures/GUI/gui_bomb_armed");

			class_pics[(int)playerClass.SORCERER] = Resources.Load<Sprite>("Textures/GUI/gui_fire");
			class_pics[(int)playerClass.SORCERER+4] = Resources.Load<Sprite>("Textures/GUI/gui_flake");

			class_pics[(int)playerClass.ROGUE] = Resources.Load<Sprite>("Textures/GUI/gui_dagger");
			class_pics[(int)playerClass.ROGUE+4] = Resources.Load<Sprite>("Textures/GUI/gui_stealth");

			class_pics[(int)playerClass.WARRIOR] = Resources.Load<Sprite>("Textures/GUI/gui_shield");
			class_pics[(int)playerClass.WARRIOR+4] = Resources.Load<Sprite>("Textures/GUI/gui_shield_active");
		}
	}

	private void ResizeBar(Image bar, float percent)
	{
		if(bar)
		{
			bar.fillAmount = percent;
		}
	}

	public void ResizeHealthBar(float percent)
	{
		ResizeBar(healthBar, percent);
	}

	public void ResizeManaBar(float percent)
	{
		ResizeBar(manaBar, percent);
	}

	public void DisplayPotion(PotionType type)
	{
		potion.sprite = potionImages[(int)type];
	}

	public void UpdateScore(int score)
	{
		scoreText.text = score + " G";
	}

	public void ChangeCharacter(playerClass pClass)
	{
		healthBar.enabled = true;
		manaBar.sprite = manaImages[(int)pClass];
		manaBar.enabled = true;
		profile.sprite = profileImages[(int)pClass];
		profile.enabled = true;
		potion.enabled = true;
		scoreText.enabled = true;
		class_abil.sprite = class_pics [(int)pClass];
	}
}
