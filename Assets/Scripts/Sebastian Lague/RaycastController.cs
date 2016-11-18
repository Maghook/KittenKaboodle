using UnityEngine;
using System.Collections;

[RequireComponent (typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour {

	public LayerMask collisionMask;

	public const float skinWidth = 0.015f; //constant value that can not be change once set

	//define how many rays are being fired horizontally and vertically
	public int horizontalRaycount = 4;
	public int verticalRaycount = 4;

	//calculate spacing between each horizontal and vertical ray, depending on how many there are and their bound size
	[HideInInspector]
	public float horizontalRaySpacing;
	[HideInInspector]
	public float verticalRaySpacing;

	[HideInInspector]
	public new BoxCollider2D collider; //changed to 'new' to stop CS0108 warning //now there's a CS0109 warning, wtf?
	public RaycastOrigins raycastOrigins;

	// Use this for initialization
	public virtual void Start () {
		collider = GetComponent<BoxCollider2D> (); //force script to require component
		CalculateRaySpacing ();
	}

	//method to fire and update the raycast origins
	public void UpdateRaycastOrigins(){
		Bounds bounds = collider.bounds; //get the bounds of the collider
		bounds.Expand (skinWidth * -2); //shrink bounds on all sides by skin width

		//create the raycasts from each corner
		raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
	}

	//method to calculate spacing between rays
	public void CalculateRaySpacing() {
		Bounds bounds = collider.bounds; //get the bounds of the collider
		bounds.Expand (skinWidth * -2); //shrink bounds on all sides by skin width

		//ensure that horizontal and vertical ray count are greater than or equal to 2
		horizontalRaycount = Mathf.Clamp(horizontalRaycount, 2, int.MaxValue);
		verticalRaycount = Mathf.Clamp(verticalRaycount, 2, int.MaxValue);

		//calculate the spacing between each ray
		horizontalRaySpacing = bounds.size.y / (horizontalRaycount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRaycount - 1);
	}

	//store corners of the box collider with raycasting
	public struct RaycastOrigins {
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}
}
