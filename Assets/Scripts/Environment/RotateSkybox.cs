using UnityEngine;
using System.Collections;

public class RotateSkybox : MonoBehaviour {

	private float rotateSkybox = 0.0f;
	public float rotateSkyboxAmount = 0.5f;

	void Update () {
		rotateSkybox += rotateSkyboxAmount * Time.deltaTime;
		rotateSkybox %= 360;
		RenderSettings.skybox.SetFloat("_Rotation", rotateSkybox);	
	}
}
