using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyArchtypeHorde : MonoBehaviour
{
	private List<GameObject> members;
	public Vector3 centerPoint;

	void Awake()
	{
		members = new List<GameObject>();
	}

	public void addMember(GameObject enemy)
	{
		members.Add(enemy);
	}

	public void removeMember(GameObject enemy)
	{
		if (members.Contains(enemy))
		{
			members.Remove(enemy);
		}
	}

	void FixedUpdate()
	{
		// Update center point
		Vector3 point = Vector3.zero;
		foreach (GameObject g in members)
		{
			point += g.transform.position;
		}
		centerPoint = point / members.Count;
	}
}