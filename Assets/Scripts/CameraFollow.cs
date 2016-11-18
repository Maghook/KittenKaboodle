using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {

	public Transform cameraTarget; //create a public field to attach the camera to an object
	public float smoothTime = 0.25f; //amount of delay when camera follows the object
	public Vector3 cameraOffset = new Vector3(0, 0, -5.0f); //camera offset so that the camera isn't stuck inside the object
	public Vector3 cameraOffsetZoom = new Vector3(0, 0, -10.0f); //camera offset to pull camera back from object

	private Vector3 velocity = Vector3.zero; //set camera velocity to (0, 0, 0)

	// Use this for initialization
	private void Start () {
		//Debug.Log ("CameraFollow Script Loaded");
		//Debug.Log ("Camera Currently Looking At " + lookAt.name);

		Camera.main.orthographic = false; //set camera projection to perspective
	}
	
	// LateUpdate is called once per frame, after the update
	private void LateUpdate () {

		//(previous code) transform.position = lookAt.transform.position + offset; //stick the camera to the object's position, minus the offset

		//smooth camera follow code
		Vector3 targetPosition = cameraTarget.TransformPoint (cameraOffset);
		//zooms camera out while button is held
		//adapts to either orthographic or perspective projections
			if (Camera.main.orthographic) {
				if (Input.GetKey (KeyCode.C)) {
					Camera.main.orthographicSize = 10;
				} else {
					Camera.main.orthographicSize = 5;
				}
			} else {
				if (Input.GetKey (KeyCode.C)) {
					targetPosition = cameraTarget.TransformPoint (cameraOffsetZoom);
				}
			}
		transform.position = Vector3.SmoothDamp (transform.position, targetPosition, ref velocity, smoothTime); //smoothens the camera follow movement
	}
}