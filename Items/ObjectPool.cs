using UnityEngine;
using System;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
	private List<GameObject> objectList;
	//model is a refernce to a prefab and is only used for cloning
	public GameObject model;
	public int maxSize;

	private void Awake()
	{
		objectList = new List<GameObject>(maxSize);

		for(int i = 0; i < maxSize; i++)
		{
			GameObject go = Instantiate(model) as GameObject;
			go.SetActive(false);
			objectList.Add(go);
		}
	}
	
	public GameObject New()
	{
		foreach(GameObject go in objectList)
		{
			if(!go.activeSelf)
			{
				go.transform.position = transform.position;
				go.transform.rotation = transform.rotation;
				go.transform.parent = null;
				go.gameObject.SetActive(true);
				return go;
			}
		}
		//consider expanding pool size
		return null;//
	}

	public void ActivateTrigger(bool state)
	{
		New();
	}
}
