using UnityEngine;
using System.Collections;

public class LoadManager : MonoBehaviour
{
	public static LoadManager current = null;

	void Awake()
	{
		if(current == null)
		{
			DontDestroyOnLoad(this);
			current = this;
		}
		else
		{
			Destroy(this);
		}
	}

	void Start()
	{
		LoadStartScene();
	}

	public void LoadStartScene()
	{
		Application.LoadLevel("StartScreen");
	}
}
