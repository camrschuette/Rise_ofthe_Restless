using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
	private Transform cameraTransform;
	private Canvas newStatBar;

	private void Start()
	{
		cameraTransform = Camera.main.transform;
		newStatBar = GameObject.Find ("newStatBar").GetComponent<Canvas> ();
	}
	private void LateUpdate()
	{
		if (PlayerManager.current.playersSpawned)
		{
			foreach(PlayerBase pb in PlayerManager.current.players)
			{
				CharacterController cc = pb.GetComponent<CharacterController>();

				float offset = cc.height / 2.0f;

				Vector3 end = pb.transform.position + pb.transform.up * offset;
				Vector3 dir = (end - cameraTransform.position);
				float dist = dir.magnitude;
				float scale = newStatBar.planeDistance / dist;
				offset -= cc.radius;
				Vector3 p1 = cameraTransform.position + cameraTransform.up * offset * scale;
				Vector3 p2 = cameraTransform.position - cameraTransform.up * offset * scale;
				RaycastHit[] hits = Physics.CapsuleCastAll(p1, p2, cc.radius * scale, dir.normalized, dist, 1 << LayerMask.NameToLayer("UI"));

				foreach(RaycastHit rh in hits)
				{
					if(rh.transform.gameObject.CompareTag("StatsGUI"))
					{
						rh.transform.GetComponent<CanvasGroup>().alpha -= Time.deltaTime * 2;
					}
				}
			}
			foreach(Transform t in newStatBar.transform)
			{
				if(t.gameObject.activeSelf)
					t.GetComponent<CanvasGroup>().alpha = Mathf.Clamp(t.GetComponent<CanvasGroup>().alpha + Time.deltaTime, 0.4f, 1.0f);
			}
		}
	}
}
