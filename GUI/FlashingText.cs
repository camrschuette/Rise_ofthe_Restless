using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FlashingText : MonoBehaviour
{
	public Text textField;
	public float interval = 0.1f;

	private Color color;
	private int mode = 0;

	private void Start()
	{

		//InvokeRepeating("Flash", interval, interval);
	}

	void Update(){
		color = textField.color;

		if (mode == 1) {
			//Debug.Log("Mode 1");
			color.a = Mathf.Lerp(color.a, 1.0f, Time.deltaTime * 1f);
			textField.color = color;
			if (color.a >= 0.91f)
				mode = 0;
		} else if (mode == 0) {
			//Debug.Log(color.a);
			color.a = Mathf.Lerp(color.a, 0.0f, Time.deltaTime * 1f);
			textField.color = color;
			if (color.a <= 0.09f)
				mode = 1;
		}
	}

	private void Flash()
	{
		//Color color = textField.color;
		//color.a = ((int)color.a) ^ 1;
		//textField.color = color;

		if (mode == 1) {
			//Debug.Log("Mode 1");
			color.a = color.a + 0.1f;
			textField.color = color;
			if (color.a >= 1.0f)
				mode = 0;
		} else if (mode == 0) {
			//Debug.Log(color.a);
			color.a = color.a - 0.1f;
			textField.color = color;
			if (color.a <= 0.0f)
				mode = 1;
		}
	}
}
