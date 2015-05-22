using UnityEngine;
using System.Collections;

public class EnemyLifeManager : MonoBehaviour 
{
	public GameObject manager;

	public void setManager(GameObject m)
	{
		manager = m;
		manager.SendMessage("notifySpawn", gameObject, SendMessageOptions.DontRequireReceiver);
	}

	void OnDestroy()
	{
		if (manager != null)
		{
			manager.SendMessage("notifyDeath", gameObject, SendMessageOptions.DontRequireReceiver);
		}
	}
}
